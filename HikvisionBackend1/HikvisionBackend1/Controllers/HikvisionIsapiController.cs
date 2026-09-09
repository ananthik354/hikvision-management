using HikvisionBackend1.Data;
using HikvisionBackend1.Models;
using HikvisionBackend1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Xml.Linq;

namespace HikvisionBackend1.Controllers
{
    [ApiController]
    [Route("api/hikvision")]
    public class HikvisionIsapiController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<HikvisionIsapiController> _logger;
        private readonly VehicleMatchingService _vehicleMatchingService;
        private readonly VehicleTriggerService _vehicleTriggerService;

        public HikvisionIsapiController(
            AppDbContext db,
            ILogger<HikvisionIsapiController> logger,
            VehicleMatchingService vehicleMatchingService,
            VehicleTriggerService vehicleTriggerService)
        {
            _db = db;
            _logger = logger;
            _vehicleMatchingService = vehicleMatchingService;
            _vehicleTriggerService = vehicleTriggerService;
        }

        // ============================================================
        // TEST ENDPOINT
        // GET /api/hikvision/test
        // ============================================================

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new
            {
                success = true,
                message = "Hikvision ISAPI endpoint is working",
                time = DateTime.Now
            });
        }

        // ============================================================
        // TEST RELAY
        // POST /api/hikvision/test-relay
        // ============================================================

        [HttpPost("test-relay")]
        public async Task<IActionResult> TestRelay()
        {
            try
            {
                // Temporary test for Camera 1.
                // Later this will become camera-specific from the UI.
                int cameraId = 1;

                int registeredVehicleId = 0;

                string plateNumber = "TEST-PLATE";

                _logger.LogInformation(
                    "Testing relay for CameraId={CameraId}",
                    cameraId);

                bool success =
                    await _vehicleTriggerService.TriggerAsync(
                        cameraId,
                        registeredVehicleId,
                        plateNumber);

                if (success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Hikvision barrier opened successfully.",
                        cameraId,
                        plateNumber
                    });
                }

                return StatusCode(500, new
                {
                    success = false,
                    message = "Hikvision barrier trigger failed.",
                    cameraId,
                    plateNumber
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Hikvision test relay failed.");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Hikvision barrier trigger failed.",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // HIKVISION ISAPI RECEIVER
        // POST /api/hikvision/anpr
        // ============================================================

        [HttpPost("anpr")]
        [RequestSizeLimit(100_000_000)]
        public async Task<IActionResult> ReceiveAnpr()
        {
            try
            {
                // ====================================================
                // 1. LOG EVERY INCOMING ANPR REQUEST
                // ====================================================

                _logger.LogInformation(
                    "========== HIKVISION ANPR REQUEST RECEIVED ==========");

                var remoteAddress =
                    HttpContext.Connection.RemoteIpAddress;

                string? remoteIp =
                    remoteAddress == null
                        ? null
                        : remoteAddress.MapToIPv4().ToString();

                _logger.LogInformation(
                    "ANPR request received from IP: {RemoteIp}",
                    remoteIp);

                _logger.LogInformation(
                    "HTTP Method: {Method}",
                    Request.Method);

                _logger.LogInformation(
                    "Content-Type: {ContentType}",
                    Request.ContentType);

                _logger.LogInformation(
                    "Content-Length: {ContentLength}",
                    Request.ContentLength);

                _logger.LogInformation(
                    "Received At: {Time}",
                    DateTime.Now);

                _logger.LogInformation(
                    "=====================================================");

                // ====================================================
                // 2. CHECK CAMERA IP
                // ====================================================

                if (string.IsNullOrWhiteSpace(remoteIp))
                {
                    _logger.LogWarning(
                        "Could not determine camera IP.");

                    return BadRequest(new
                    {
                        success = false,
                        message = "Camera IP could not be determined."
                    });
                }

                // ====================================================
                // 3. FIND CAMERA
                // ====================================================

                Camera? camera =
                    await _db.Cameras
                        .FirstOrDefaultAsync(x =>
                            x.IpAddress == remoteIp &&
                            x.IsapiEnabled);

                // ====================================================
                // 4. CAMERA NOT FOUND
                // ====================================================

                if (camera == null)
                {
                    _logger.LogWarning(
                        "Unknown or disabled Hikvision camera IP: {RemoteIp}",
                        remoteIp);

                    return NotFound(new
                    {
                        success = false,
                        message =
                            "Camera not registered or ISAPI is disabled.",
                        cameraIp = remoteIp
                    });
                }

                // ====================================================
                // 5. UPDATE CAMERA ISAPI STATUS
                // ====================================================

                camera.IsIsapiOnline = true;

                camera.LastIsapiReceivedAt =
                    DateTime.Now;

                camera.LastIsapiError = null;

                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "Camera identified: {CameraName} ({CameraIp}) - CameraId={CameraId}",
                    camera.Name,
                    camera.IpAddress,
                    camera.Id);

                // ====================================================
                // 6. GET CONTENT TYPE
                // ====================================================

                string contentType =
                    Request.ContentType ?? string.Empty;

                // ====================================================
                // 7. MULTIPART REQUEST
                // ====================================================

                if (contentType.StartsWith(
                    "multipart/",
                    StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation(
                        "Processing Hikvision multipart request.");

                    await ProcessMultipartRequest(camera);

                    return Ok(new
                    {
                        success = true,
                        message = "Hikvision ISAPI event received.",
                        cameraId = camera.Id,
                        camera = camera.Name
                    });
                }

                // ====================================================
                // 8. RAW XML REQUEST
                // ====================================================

                if (contentType.Contains(
                    "xml",
                    StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation(
                        "Processing Hikvision raw XML request.");

                    using var reader =
                        new StreamReader(
                            Request.Body,
                            Encoding.UTF8);

                    string xml =
                        await reader.ReadToEndAsync();

                    _logger.LogInformation(
                        "Hikvision XML received:\n{Xml}",
                        xml);

                    await ProcessXml(
                        camera,
                        xml,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null);

                    return Ok(new
                    {
                        success = true,
                        message = "Hikvision XML received.",
                        cameraId = camera.Id,
                        camera = camera.Name
                    });
                }

                // ====================================================
                // 9. UNKNOWN CONTENT TYPE
                // ====================================================

                _logger.LogWarning(
                    "Unknown Hikvision Content-Type: {ContentType}",
                    contentType);

                return Ok(new
                {
                    success = true,
                    message =
                        "Request received but content type is not recognized.",
                    contentType
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ERROR PROCESSING HIKVISION ISAPI REQUEST");

                return StatusCode(500, new
                {
                    success = false,
                    message =
                        "Error processing Hikvision ISAPI request.",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // PROCESS MULTIPART REQUEST
        // ============================================================

        private async Task ProcessMultipartRequest(
            Camera camera)
        {
            string contentType =
                Request.ContentType ?? string.Empty;

            string boundary =
                GetBoundary(contentType);

            if (string.IsNullOrWhiteSpace(boundary))
            {
                throw new Exception(
                    "Multipart boundary not found.");
            }

            _logger.LogInformation(
                "Multipart boundary detected.");

            var reader =
                new MultipartReader(
                    boundary,
                    Request.Body);

            MultipartSection? section;

            string? xmlData = null;

            byte[]? detectionImage = null;
            byte[]? plateImage = null;
            byte[]? plateBinaryImage = null;

            string? detectionImageName = null;
            string? plateImageName = null;
            string? plateBinaryImageName = null;

            // ========================================================
            // READ EVERY MULTIPART SECTION
            // ========================================================

            while ((section =
                await reader.ReadNextSectionAsync()) != null)
            {
                string sectionContentType =
                    section.ContentType ?? string.Empty;

                string contentId = string.Empty;

                if (section.Headers.TryGetValue(
                    "Content-ID",
                    out var contentIdHeader))
                {
                    contentId =
                        contentIdHeader.ToString();
                }

                string contentDisposition = string.Empty;

                if (section.Headers.TryGetValue(
                    "Content-Disposition",
                    out var contentDispositionHeader))
                {
                    contentDisposition =
                        contentDispositionHeader.ToString();
                }

                using var memoryStream =
                    new MemoryStream();

                await section.Body.CopyToAsync(
                    memoryStream);

                byte[] data =
                    memoryStream.ToArray();

                _logger.LogInformation(
                    "ISAPI Multipart Section: " +
                    "ContentType={ContentType}, " +
                    "ContentId={ContentId}, " +
                    "ContentDisposition={ContentDisposition}, " +
                    "Size={Size}",
                    sectionContentType,
                    contentId,
                    contentDisposition,
                    data.Length);

                // ====================================================
                // XML
                // ====================================================

                if (sectionContentType.Contains(
                        "xml",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    contentId.Contains(
                        "xml",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    contentDisposition.Contains(
                        ".xml",
                        StringComparison.OrdinalIgnoreCase))
                {
                    xmlData =
                        Encoding.UTF8.GetString(data);

                    _logger.LogInformation(
                        "ISAPI XML received. Size={Size}",
                        data.Length);

                    continue;
                }

                // ====================================================
                // IMAGE
                // ====================================================

                if (sectionContentType.Contains(
                    "image",
                    StringComparison.OrdinalIgnoreCase))
                {
                    string extension =
                        GetImageExtension(
                            sectionContentType);

                    string fileName =
                        $"ISAPI_{DateTime.Now:yyyyMMdd_HHmmssfff}_{Guid.NewGuid():N}{extension}";

                    // =================================================
                    // PLATE BINARY IMAGE
                    // =================================================

                    if (contentDisposition.Contains(
                            "plateBinaryPicture",
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        contentId.Contains(
                            "plateBinaryPicture",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        plateBinaryImage =
                            data;

                        plateBinaryImageName =
                            fileName;

                        _logger.LogInformation(
                            "Plate binary image received: {FileName}",
                            fileName);
                    }

                    // =================================================
                    // LICENSE PLATE IMAGE
                    // =================================================

                    else if (contentDisposition.Contains(
                                "licensePlatePicture",
                                StringComparison.OrdinalIgnoreCase)
                             ||
                             contentId.Contains(
                                "licensePlatePicture",
                                StringComparison.OrdinalIgnoreCase))
                    {
                        plateImage =
                            data;

                        plateImageName =
                            fileName;

                        _logger.LogInformation(
                            "License plate image received: {FileName}",
                            fileName);
                    }

                    // =================================================
                    // DETECTION / FULL VEHICLE IMAGE
                    // =================================================

                    else if (contentDisposition.Contains(
                                "detectionPicture",
                                StringComparison.OrdinalIgnoreCase)
                             ||
                             contentId.Contains(
                                "detectionPicture",
                                StringComparison.OrdinalIgnoreCase))
                    {
                        detectionImage =
                            data;

                        detectionImageName =
                            fileName;

                        _logger.LogInformation(
                            "Detection image received: {FileName}",
                            fileName);
                    }

                    // =================================================
                    // UNKNOWN IMAGE
                    // =================================================

                    else
                    {
                        _logger.LogInformation(
                            "Unknown image received: {FileName}",
                            fileName);
                    }
                }
            }

            // ========================================================
            // PROCESS XML + IMAGES
            // ========================================================

            if (!string.IsNullOrWhiteSpace(xmlData))
            {
                await ProcessXml(
                    camera,
                    xmlData,
                    detectionImage,
                    plateImage,
                    plateBinaryImage,
                    detectionImageName,
                    plateImageName,
                    plateBinaryImageName);
            }
            else
            {
                _logger.LogWarning(
                    "ISAPI request received but XML was not found.");

                camera.LastIsapiError =
                    "ISAPI request received without XML.";

                await _db.SaveChangesAsync();
            }
        }

        // ============================================================
        // PROCESS XML + IMAGES
        // ============================================================

        private async Task ProcessXml(
            Camera camera,
            string xmlData,
            byte[]? fullVehicleImage,
            byte[]? plateImage,
            byte[]? plateBinaryImage,
            string? fullImageName,
            string? plateImageName,
            string? plateBinaryImageName)
        {
            try
            {
                // ====================================================
                // 1. PARSE XML
                // ====================================================

                XDocument document =
                    XDocument.Parse(xmlData);

                // ====================================================
                // 2. HEARTBEAT
                // ====================================================

                bool isHeartbeat =
                    xmlData.Contains(
                        "heartBeat",
                        StringComparison.OrdinalIgnoreCase);

                if (isHeartbeat)
                {
                    camera.IsIsapiOnline = true;

                    camera.LastIsapiReceivedAt =
                        DateTime.Now;

                    camera.LastIsapiEventType =
                        "Heartbeat";

                    camera.LastIsapiError = null;

                    await _db.SaveChangesAsync();

                    _logger.LogInformation(
                        "HIKVISION HEARTBEAT RECEIVED - no VehicleDetection created");

                    return;
                }

                // ====================================================
                // 3. FIND ANPR NODE
                // ====================================================

                XElement? anprNode =
                    document
                        .Descendants()
                        .FirstOrDefault(x =>
                            x.Name.LocalName.Equals(
                                "ANPR",
                                StringComparison.OrdinalIgnoreCase));

                if (anprNode == null)
                {
                    _logger.LogWarning(
                        "ANPR node was not found in Hikvision XML.");

                    _logger.LogInformation(
                        "Received XML:\n{Xml}",
                        xmlData);

                    camera.LastIsapiError =
                        "ANPR node not found in XML.";

                    await _db.SaveChangesAsync();

                    return;
                }

                // ====================================================
                // 4. READ HIKVISION VALUES
                // ====================================================

                string? plateNumber =
                    FindValue(
                        anprNode,
                        "licensePlate");

                string? vehicleType =
                    FindValue(
                        anprNode,
                        "vehicleType");

                string? confidenceText =
                    FindValue(
                        anprNode,
                        "confidenceLevel");

                string? direction =
                    FindValue(
                        anprNode,
                        "direction");

                if (string.IsNullOrWhiteSpace(direction))
                {
                    direction =
                        FindValue(
                            anprNode,
                            "detectDir");
                }

                string? country =
                    FindValue(
                        anprNode,
                        "country");

                string? uuid =
                    FindValue(
                        document.Root,
                        "UUID");

                string? dateTimeText =
                    FindValue(
                        document.Root,
                        "dateTime");

                // ====================================================
                // 5. CONFIDENCE
                // ====================================================

                int? confidence = null;

                if (int.TryParse(
                    confidenceText,
                    out int confidenceValue))
                {
                    confidence =
                        confidenceValue;
                }

                // ====================================================
                // 6. DETECTION TIME
                // ====================================================

                DateTime detectedAt =
                    DateTime.Now;

                if (DateTimeOffset.TryParse(
                    dateTimeText,
                    out DateTimeOffset parsedDate))
                {
                    detectedAt =
                        parsedDate.LocalDateTime;
                }

                // ====================================================
                // 7. CLEAN / NORMALIZE VALUES
                // ====================================================

                plateNumber =
                    NormalizePlateNumber(
                        plateNumber);

                vehicleType =
                    CleanValue(
                        vehicleType) ?? "Unknown";

                direction =
                    CleanValue(
                        direction);

                country =
                    CleanValue(
                        country);

                uuid =
                    CleanValue(
                        uuid);

                // ====================================================
                // 8. UPDATE CAMERA STATUS
                // ====================================================

                camera.IsIsapiOnline = true;

                camera.LastIsapiReceivedAt =
                    DateTime.Now;

                camera.LastIsapiEventType =
                    "ANPR";

                camera.LastIsapiPlateNumber =
                    plateNumber;

                camera.LastIsapiError = null;

                // ====================================================
                // 9. LOG DETECTION
                // ====================================================

                _logger.LogInformation(
                    "==========================================");

                _logger.LogInformation(
                    "HIKVISION ANPR DETECTION");

                _logger.LogInformation(
                    "Camera ID: {CameraId}",
                    camera.Id);

                _logger.LogInformation(
                    "Camera: {CameraName}",
                    camera.Name);

                _logger.LogInformation(
                    "Camera IP: {CameraIp}",
                    camera.IpAddress);

                _logger.LogInformation(
                    "Plate: {Plate}",
                    plateNumber);

                _logger.LogInformation(
                    "Vehicle Type: {VehicleType}",
                    vehicleType);

                _logger.LogInformation(
                    "Confidence: {Confidence}",
                    confidence);

                _logger.LogInformation(
                    "Direction: {Direction}",
                    direction);

                _logger.LogInformation(
                    "Country: {Country}",
                    country);

                _logger.LogInformation(
                    "UUID: {UUID}",
                    uuid);

                _logger.LogInformation(
                    "Full Vehicle Image: {HasFullImage}",
                    fullVehicleImage != null);

                _logger.LogInformation(
                    "Plate Image: {HasPlateImage}",
                    plateImage != null);

                _logger.LogInformation(
                    "Plate Binary Image: {HasPlateBinaryImage}",
                    plateBinaryImage != null);

                _logger.LogInformation(
                    "==========================================");

                // ====================================================
                // 10. DUPLICATE UUID CHECK
                // CAMERA-SPECIFIC
                // ====================================================

                if (!string.IsNullOrWhiteSpace(uuid))
                {
                    bool exists =
                        await _db.VehicleDetections
                            .AnyAsync(x =>
                                x.CameraId == camera.Id &&
                                x.UUID == uuid);

                    if (exists)
                    {
                        _logger.LogInformation(
                            "Duplicate UUID ignored. CameraId={CameraId}, UUID={UUID}",
                            camera.Id,
                            uuid);

                        await _db.SaveChangesAsync();

                        return;
                    }
                }

                // ====================================================
                // 11. CONVERT IMAGES TO BASE64
                // ====================================================

                string? fullVehicleBase64 =
                    fullVehicleImage != null
                        ? Convert.ToBase64String(
                            fullVehicleImage)
                        : null;

                string? plateImageBase64 =
                    plateImage != null
                        ? Convert.ToBase64String(
                            plateImage)
                        : null;

                string? plateBinaryImageBase64 =
                    plateBinaryImage != null
                        ? Convert.ToBase64String(
                            plateBinaryImage)
                        : null;

                // ====================================================
                // 12. CREATE VEHICLE DETECTION
                // ====================================================

                var detection =
                    new VehicleDetection
                    {
                        CameraId =
                            camera.Id,

                        CameraName =
                            camera.Name,

                        PlateNumber =
                            plateNumber,

                        VehicleType =
                            vehicleType,

                        ConfidenceLevel =
                            confidence,

                        Direction =
                            direction,

                        Country =
                            country,

                        UUID =
                            uuid,

                        FullVehicleImageBase64 =
                            fullVehicleBase64,

                        PlateImageBase64 =
                            plateImageBase64,

                        PlateBinaryImageBase64 =
                            plateBinaryImageBase64,

                        XmlData =
                            xmlData,

                        XmlFileName =
                            "anpr.xml",

                        ImageFileName =
                            fullImageName,

                        DetectedAt =
                            detectedAt
                    };

                // ====================================================
                // 13. SAVE VEHICLE DETECTION
                // ====================================================

                _logger.LogInformation(
                    "Saving VehicleDetection: " +
                    "CameraId={CameraId}, " +
                    "Plate={Plate}, " +
                    "DetectedAt={DetectedAt}",
                    detection.CameraId,
                    detection.PlateNumber,
                    detection.DetectedAt);

                _db.VehicleDetections.Add(
                    detection);

                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "SUCCESS: VehicleDetection saved. " +
                    "DetectionId={DetectionId}, CameraId={CameraId}",
                    detection.Id,
                    detection.CameraId);

                // ====================================================
                // 14. CHECK REGISTERED VEHICLE
                // ====================================================

                _logger.LogInformation(
                    "Checking registered vehicles for " +
                    "CameraId={CameraId}, Plate={Plate}",
                    detection.CameraId,
                    detection.PlateNumber);

                bool matched =
                    await _vehicleMatchingService
                        .CheckAndStoreMatchAsync(
                            detection);

                if (matched)
                {
                    _logger.LogInformation(
                        "REGISTERED VEHICLE MATCHED: " +
                        "CameraId={CameraId}, Plate={Plate}",
                        detection.CameraId,
                        detection.PlateNumber);
                }
                else
                {
                    _logger.LogInformation(
                        "VEHICLE NOT REGISTERED: " +
                        "CameraId={CameraId}, Plate={Plate} - BARRIER WILL NOT OPEN",
                        detection.CameraId,
                        detection.PlateNumber);
                }

                // ====================================================
                // 15. FINAL SUCCESS LOG
                // ====================================================

                _logger.LogInformation(
                    "==========================================");

                _logger.LogInformation(
                    "SUCCESS: VehicleDetection processing completed");

                _logger.LogInformation(
                    "Detection ID: {Id}",
                    detection.Id);

                _logger.LogInformation(
                    "Camera ID: {CameraId}",
                    detection.CameraId);

                _logger.LogInformation(
                    "Camera: {CameraName}",
                    camera.Name);

                _logger.LogInformation(
                    "Plate: {Plate}",
                    plateNumber);

                _logger.LogInformation(
                    "UUID: {UUID}",
                    uuid);

                _logger.LogInformation(
                    "Full Image: {Image}",
                    fullImageName);

                _logger.LogInformation(
                    "Plate Image: {PlateImage}",
                    plateImageName);

                _logger.LogInformation(
                    "Plate Binary Image: {PlateBinaryImage}",
                    plateBinaryImageName);

                _logger.LogInformation(
                    "==========================================");
            }
            catch (Exception ex)
            {
                camera.IsIsapiOnline = false;

                camera.LastIsapiError =
                    ex.Message;

                await _db.SaveChangesAsync();

                _logger.LogError(
                    ex,
                    "Failed to process Hikvision XML for CameraId={CameraId}.",
                    camera.Id);

                // IMPORTANT:
                // Re-throw so ReceiveAnpr() knows processing failed.
                throw;
            }
        }

        // ============================================================
        // FIND VALUE FROM XML ELEMENT
        // ============================================================

        private static string? FindValue(
            XElement? parent,
            string elementName)
        {
            if (parent == null)
                return null;

            return parent
                .DescendantsAndSelf()
                .FirstOrDefault(x =>
                    x.Name.LocalName.Equals(
                        elementName,
                        StringComparison.OrdinalIgnoreCase))
                ?.Value;
        }

        // ============================================================
        // CLEAN VALUE
        // ============================================================

        private static string? CleanValue(
            string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value.Trim();
        }

        // ============================================================
        // NORMALIZE PLATE NUMBER
        // ============================================================

        private static string? NormalizePlateNumber(
            string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value
                .Trim()
                .ToUpperInvariant()
                .Replace(" ", "")
                .Replace("-", "");
        }

        // ============================================================
        // MULTIPART BOUNDARY
        // ============================================================

        private static string GetBoundary(
            string contentType)
        {
            string? boundary =
                contentType
                    .Split(';')
                    .Select(x => x.Trim())
                    .FirstOrDefault(x =>
                        x.StartsWith(
                            "boundary=",
                            StringComparison.OrdinalIgnoreCase));

            if (boundary == null)
                return string.Empty;

            return boundary
                .Substring(
                    "boundary=".Length)
                .Trim('"');
        }

        // ============================================================
        // IMAGE EXTENSION
        // ============================================================

        private static string GetImageExtension(
            string contentType)
        {
            if (contentType.Contains(
                "png",
                StringComparison.OrdinalIgnoreCase))
            {
                return ".png";
            }

            if (contentType.Contains(
                "jpeg",
                StringComparison.OrdinalIgnoreCase))
            {
                return ".jpg";
            }

            return ".jpg";
        }

        // ============================================================
        // CAMERA STATUS
        // GET /api/hikvision/status
        // ============================================================

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var cameras =
                await _db.Cameras
                    .AsNoTracking()
                    .Select(x => new
                    {
                        x.Id,
                        x.Name,
                        x.IpAddress,
                        x.Location,
                        x.Status,

                        x.IsapiEnabled,
                        x.SdkEnabled,
                        x.RelayEnabled,
                        x.RelayOutput,

                        x.IsIsapiOnline,
                        x.LastIsapiReceivedAt,
                        x.LastIsapiEventType,
                        x.LastIsapiPlateNumber,
                        x.LastIsapiError
                    })
                    .ToListAsync();

            return Ok(cameras);
        }
    }
}