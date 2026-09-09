using System.Xml.Linq;
using HikvisionBackend1.Data;
using HikvisionBackend1.Models;
using Microsoft.EntityFrameworkCore;

namespace HikvisionBackend1.Services
{
    public class VehicleImageWatcher : BackgroundService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<VehicleImageWatcher> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly GroqPlateService _groqPlateService;
        private FileSystemWatcher? _watcher;
        private static string NormalizePlate(string? plate)
        {
            if (string.IsNullOrWhiteSpace(plate))
                return "";

            return new string(
                plate
                    .Where(char.IsLetterOrDigit)
                    .ToArray()
            ).ToUpperInvariant();
        }
        public VehicleImageWatcher(
    IWebHostEnvironment environment,
    ILogger<VehicleImageWatcher> logger,
    IServiceScopeFactory scopeFactory,
    GroqPlateService groqPlateService)
        {
            _environment = environment;
            _logger = logger;
            _scopeFactory = scopeFactory;
            _groqPlateService = groqPlateService;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            string imagesFolder = Path.Combine(
                _environment.WebRootPath,
                "Images"
            );

            if (!Directory.Exists(imagesFolder))
            {
                Directory.CreateDirectory(imagesFolder);
            }

            _logger.LogInformation(
                "Vehicle image processor started."
            );

            _logger.LogInformation(
                "Watching folder: {Folder}",
                imagesFolder
            );

            _watcher = new FileSystemWatcher(imagesFolder)
            {
                Filter = "*.*",
                NotifyFilter =
                    NotifyFilters.FileName |
                    NotifyFilters.CreationTime |
                    NotifyFilters.Size,

                EnableRaisingEvents = true
            };

            _watcher.Created += OnFileCreated;

            // Process XML files that already exist
            await ProcessExistingFiles(
                imagesFolder,
                stoppingToken
            );

            try
            {
                await Task.Delay(
                    Timeout.Infinite,
                    stoppingToken
                );
            }
            catch (TaskCanceledException)
            {
                // Application is shutting down.
            }
        }

        private async void OnFileCreated(
            object sender,
            FileSystemEventArgs e)
        {
            try
            {
                string extension =
                    Path.GetExtension(e.FullPath)
                        .ToLowerInvariant();

                if (extension != ".xml" &&
                    extension != ".jpg" &&
                    extension != ".jpeg" &&
                    extension != ".png")
                {
                    return;
                }

                _logger.LogInformation(
                    "New file detected: {FileName}",
                    e.Name
                );

                // Give FTP time to finish writing
                await Task.Delay(1000);

                await TryProcessEvent(e.FullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing file: {FileName}",
                    e.Name
                );
            }
        }

        private async Task ProcessExistingFiles(
            string imagesFolder,
            CancellationToken cancellationToken)
        {
            try
            {
                string[] xmlFiles =
                    Directory.GetFiles(
                        imagesFolder,
                        "*.xml"
                    );

                foreach (string xmlFile in xmlFiles)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    await TryProcessEvent(xmlFile);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing existing XML files."
                );
            }
        }

        private async Task TryProcessEvent(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath)!;

            string fileName =
                Path.GetFileNameWithoutExtension(filePath);

            string eventId = GetEventId(fileName);

            if (string.IsNullOrWhiteSpace(eventId))
                return;

            string xmlPath =
                Path.Combine(
                    directory,
                    eventId + ".xml"
                );

            string fullImagePath =
                Path.Combine(
                    directory,
                    eventId + ".jpg"
                );

            string plateImagePath =
                Path.Combine(
                    directory,
                    "Plate" + eventId + ".jpg"
                );

            _logger.LogInformation(
                "Checking event: {EventId}",
                eventId
            );

            bool xmlExists = File.Exists(xmlPath);
            bool fullImageExists = File.Exists(fullImagePath);
            bool plateImageExists = File.Exists(plateImagePath);

            _logger.LogInformation(
                "XML exists: {Exists}",
                xmlExists
            );

            _logger.LogInformation(
                "Full image exists: {Exists}",
                fullImageExists
            );

            _logger.LogInformation(
                "Plate image exists: {Exists}",
                plateImageExists
            );

            // XML is required
            if (!xmlExists)
            {
                _logger.LogInformation(
                    "XML not found for {EventId}. Waiting...",
                    eventId
                );

                return;
            }

            // Full vehicle image is required
            if (!fullImageExists)
            {
                _logger.LogInformation(
                    "Full vehicle image not found for {EventId}. Waiting...",
                    eventId
                );

                return;
            }

            // Plate image is OPTIONAL
            if (plateImageExists)
            {
                _logger.LogInformation(
                    "Plate image found for {EventId}.",
                    eventId
                );
            }
            else
            {
                _logger.LogInformation(
                    "Plate image NOT found for {EventId}. Continuing without plate image.",
                    eventId
                );
            }

            _logger.LogInformation(
                "Required files found for {EventId}. Processing...",
                eventId
            );

            // Make sure required files are completely uploaded
            await WaitForFileReady(xmlPath);

            await WaitForFileReady(fullImagePath);

            // Only wait for plate image if it exists
            if (File.Exists(plateImagePath))
            {
                await WaitForFileReady(plateImagePath);
            }

            await ProcessVehicle(
                eventId,
                xmlPath,
                fullImagePath,
                plateImageExists ? plateImagePath : null
            );
        }

        private async Task ProcessVehicle(
            string eventId,
            string xmlPath,
            string fullImagePath,
            string? plateImagePath)
        {
            try
            {
                // ==========================================
                // READ XML
                // ==========================================

                XDocument xml =
                    XDocument.Load(xmlPath);

                string? plateNumber =
                    xml.Root?
                        .Element("licensePlate")?
                        .Value;

                string? vehicleType =
                    xml.Root?
                        .Element("vehicleType")?
                        .Value;

                string? dateTimeText =
                    xml.Root?
                        .Element("dateTime")?
                        .Value;

                string? confidenceText =
                    xml.Root?
                        .Element("confidenceLevel")?
                        .Value;

                string? direction =
                    xml.Root?
                        .Element("detectDir")?
                        .Value;

                string? country =
                    xml.Root?
                        .Element("country")?
                        .Value;

                string? uuid =
                    xml.Root?
                        .Element("UUID")?
                        .Value;

                // ==========================================
                // CLEAN VALUES
                // ==========================================

                plateNumber =
                    string.IsNullOrWhiteSpace(plateNumber)
                        ? null
                        : plateNumber.Trim();

                vehicleType =
                    string.IsNullOrWhiteSpace(vehicleType)
                        ? "Unknown"
                        : vehicleType.Trim();

                direction =
                    string.IsNullOrWhiteSpace(direction)
                        ? null
                        : direction.Trim();

                country =
                    string.IsNullOrWhiteSpace(country)
                        ? null
                        : country.Trim();

                uuid =
                    string.IsNullOrWhiteSpace(uuid)
                        ? null
                        : uuid.Trim();

                // ==========================================
                // CONFIDENCE
                // ==========================================

                int? confidence = null;

                if (int.TryParse(
                    confidenceText,
                    out int confidenceValue))
                {
                    confidence = confidenceValue;
                }

                // ==========================================
                // DETECTION TIME
                // ==========================================

                DateTime detectedAt =
                    DateTime.Now;

                if (DateTimeOffset.TryParse(
                    dateTimeText,
                    out DateTimeOffset parsedDate))
                {
                    detectedAt =
                        parsedDate.LocalDateTime;
                }

                // ==========================================
                // READ FULL VEHICLE IMAGE
                // ==========================================

                byte[] fullImageBytes =
                    await File.ReadAllBytesAsync(
                        fullImagePath
                    );

                // ==========================================
                // READ PLATE IMAGE
                // ==========================================

                byte[]? plateImageBytes = null;

                if (!string.IsNullOrWhiteSpace(plateImagePath) &&
                    File.Exists(plateImagePath))
                {
                    plateImageBytes =
                        await File.ReadAllBytesAsync(
                            plateImagePath
                        );
                }
                // ==========================================
                // READ PLATE USING GROQ
                // ==========================================
                PlateResult? groqResult = null;

                if (!string.IsNullOrWhiteSpace(plateImagePath) &&
                    File.Exists(plateImagePath))
                {
                    // Priority 1: dedicated plate image
                    _logger.LogInformation(
                        "Plate image found. Sending plate image to Groq.");

                    groqResult =
                        await _groqPlateService.ReadPlateAsync(
                            plateImagePath);
                }
                else
                {
                    // Priority 2: full vehicle image
                    _logger.LogInformation(
                        "Plate image not found. Sending full vehicle image to Groq.");

                    groqResult =
                        await _groqPlateService.ReadPlateAsync(
                            fullImagePath);
                }

                if (groqResult != null &&
                    !string.IsNullOrWhiteSpace(groqResult.PlateNumber))
                {
                    plateNumber =
                        groqResult.PlateNumber.Trim();

                    _logger.LogInformation(
                        "Groq detected plate: {PlateNumber}",
                        plateNumber);

                    if (!string.IsNullOrWhiteSpace(groqResult.VehicleType))
                    {
                        vehicleType =
                            groqResult.VehicleType.Trim();
                    }
                }
                else
                {
                    _logger.LogWarning(
                        "Groq could not read plate. Using Hikvision XML plate: {PlateNumber}",
                        plateNumber);
                }
                // ==========================================
                // FINAL PLATE NUMBER
                // ==========================================

                string finalPlateNumber;

                if (groqResult != null &&
                    !string.IsNullOrWhiteSpace(groqResult.PlateNumber))
                {
                    finalPlateNumber =
                        NormalizePlate(groqResult.PlateNumber);

                    _logger.LogInformation(
                        "✅ Groq plate: {PlateNumber}",
                        finalPlateNumber);
                }
                else
                {
                    finalPlateNumber =
                        NormalizePlate(plateNumber);

                    _logger.LogWarning(
                        "⚠️ Groq could not read plate. Using XML plate: {PlateNumber}",
                        finalPlateNumber);
                }


                // ----------------------------------------------------
                // FINAL VEHICLE TYPE
                // ----------------------------------------------------

                string finalVehicleType =
                    groqResult != null &&
                    !string.IsNullOrWhiteSpace(groqResult.VehicleType) &&
                    groqResult.VehicleType != "Unknown"
                        ? groqResult.VehicleType.Trim()
                        : vehicleType ?? "Unknown";


                _logger.LogInformation(
                    "FINAL PLATE USED FOR MATCHING: {PlateNumber}",
                    finalPlateNumber);


                // ----------------------------------------------------
                // CONVERT TO BASE64
                // ----------------------------------------------------

                string fullVehicleBase64 =
                    Convert.ToBase64String(fullImageBytes);

                string? plateImageBase64 =
                    plateImageBytes != null
                        ? Convert.ToBase64String(plateImageBytes)
                        : null;


                // ----------------------------------------------------
                // XML DATA
                // ----------------------------------------------------

                string xmlData =
                    await File.ReadAllTextAsync(xmlPath);


                // ----------------------------------------------------
                // DATABASE SCOPE
                // ----------------------------------------------------

                using var scope =
     _scopeFactory.CreateScope();

                var db =
                    scope.ServiceProvider
                       .GetRequiredService<AppDbContext>();

                var vehicleMatchingService =
                    scope.ServiceProvider
                       .GetRequiredService<VehicleMatchingService>();


                // ----------------------------------------------------
                // CAMERA
                // ----------------------------------------------------

                Camera? camera =
                    await db.Cameras.FirstOrDefaultAsync();

                if (camera == null)
                {
                    _logger.LogWarning(
                        "⚠️ No camera found in database.");

                    return;
                }


                // ----------------------------------------------------
                // DUPLICATE UUID CHECK
                // ----------------------------------------------------

                bool detectionExists =
                    await db.VehicleDetections
                        .AnyAsync(x => x.UUID == uuid);

                if (detectionExists)
                {
                    _logger.LogInformation(
                        "⏭️ Duplicate UUID ignored: {UUID}",
                        uuid);

                    DeleteFiles(
                        xmlPath,
                        fullImagePath,
                        plateImagePath);

                    return;
                }


                // ----------------------------------------------------
                // DUPLICATE VEHICLE CHECK
                // Same plate within previous 10 seconds
                // ----------------------------------------------------

                if (!string.IsNullOrWhiteSpace(finalPlateNumber))
                {
                    DateTime duplicateWindowStart =
                        DateTime.Now.AddSeconds(-10);

                    bool recentDetection =
                        await db.VehicleDetections.AnyAsync(x =>
                            x.PlateNumber == finalPlateNumber &&
                            x.DetectedAt >= duplicateWindowStart);

                    bool recentHistory =
                        await db.VehicleHistory.AnyAsync(x =>
                            x.PlateNumber == finalPlateNumber &&
                            x.DetectedAt >= duplicateWindowStart);

                    if (recentDetection || recentHistory)
                    {
                        _logger.LogInformation(
                            "⏭️ Duplicate vehicle ignored: {PlateNumber}",
                            finalPlateNumber);

                        DeleteFiles(
                            xmlPath,
                            fullImagePath,
                            plateImagePath);

                        return;
                    }
                }


              

                

                // ----------------------------------------------------
                // NORMAL VEHICLE DETECTION
                // ----------------------------------------------------

                var detection = new VehicleDetection
                {
                    PlateNumber =
                        finalPlateNumber,

                    VehicleType =
                        finalVehicleType,

                    CameraId =
                        camera.Id,

                    CameraName =
                        camera.Name,

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

                    XmlData =
                        xmlData,

                    DetectedAt =
                        detectedAt,

                    ImageFileName =
                        Path.GetFileName(fullImagePath)
                };

                db.VehicleDetections.Add(detection);

                await db.SaveChangesAsync();

                _logger.LogInformation(
                    "✅ Vehicle detection saved: {PlateNumber}",
                    finalPlateNumber);
                // ----------------------------------------------------
                // REGISTERED VEHICLE MATCHING
                // ----------------------------------------------------

                bool matched =
                    await vehicleMatchingService
                        .CheckAndStoreMatchAsync(detection);

                if (matched)
                {
                    _logger.LogInformation(
                        "REGISTERED VEHICLE MATCHED: {Plate}",
                        detection.PlateNumber);
                }
                else
                {
                    _logger.LogInformation(
                        "VEHICLE NOT REGISTERED: {Plate} - BARRIER WILL NOT OPEN",
                        detection.PlateNumber);
                }

                DeleteFiles(
                    xmlPath,
                    fullImagePath,
                    plateImagePath);

             
            }
            catch (Exception ex)
            {
                /*
                 * IMPORTANT:
                 *
                 * If anything fails, files remain
                 * in wwwroot/Images.
                 */

                _logger.LogError(
                    ex,
                    "FAILED: Event {EventId}. Files were NOT deleted.",
                    eventId
                );
            }
        }

        private static string GetEventId(
            string fileName)
        {
            /*
             * Example:
             *
             * input:
             * 192.168.10.64_20260828123053776_000
             *
             * output:
             * 192.168.10.64_20260828123053776_000
             *
             *
             * input:
             * 192.168.10.64_20260828123053776_000_plate
             *
             * output:
             * 192.168.10.64_20260828123053776_000
             */

            if (fileName.StartsWith(
        "Plate",
        StringComparison.OrdinalIgnoreCase))
            {
                return fileName.Substring(5);
            }

            return fileName;
        }

        private async Task WaitForFileReady(
            string filePath)
        {
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    using FileStream stream =
                        new FileStream(
                            filePath,
                            FileMode.Open,
                            FileAccess.Read,
                            FileShare.None
                        );

                    if (stream.Length > 0)
                    {
                        return;
                    }
                }
                catch (IOException)
                {
                    // FTP is still writing the file.
                }

                await Task.Delay(500);
            }

            throw new IOException(
                $"File is not ready: {filePath}"
            );
        }

        private void DeleteFiles(
    string xmlPath,
    string fullImagePath,
    string? plateImagePath)
        {
            try
            {
                if (File.Exists(xmlPath))
                {
                    File.Delete(xmlPath);

                    _logger.LogInformation(
                        "Deleted XML: {File}",
                        xmlPath
                    );
                }

                if (File.Exists(fullImagePath))
                {
                    File.Delete(fullImagePath);

                    _logger.LogInformation(
                        "Deleted full vehicle image: {File}",
                        fullImagePath
                    );
                }

                if (!string.IsNullOrWhiteSpace(plateImagePath) &&
                    File.Exists(plateImagePath))
                {
                    File.Delete(plateImagePath);

                    _logger.LogInformation(
                        "Deleted plate image: {File}",
                        plateImagePath
                    );
                }

                _logger.LogInformation(
                    "Event files deleted successfully."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Database saved, but file deletion failed."
                );
            }
        }

        public override void Dispose()
        {
            _watcher?.Dispose();

            base.Dispose();
        }
    }
}