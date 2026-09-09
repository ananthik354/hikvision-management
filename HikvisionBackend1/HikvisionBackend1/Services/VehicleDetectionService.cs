using HikvisionBackend1.Data;
using HikvisionBackend1.Models;
using Microsoft.EntityFrameworkCore;

namespace HikvisionBackend1.Services
{
    public class VehicleDetectionService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<VehicleDetectionService> _logger;

        public VehicleDetectionService(
            AppDbContext context,
            IWebHostEnvironment environment,
            ILogger<VehicleDetectionService> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        // ============================================================
        // GET ALL VEHICLE DETECTIONS
        // ============================================================

        public async Task<List<VehicleDetection>> GetAllAsync()
        {
            return await _context.VehicleDetections
                .AsNoTracking()
                .OrderByDescending(x => x.DetectedAt)
                .Select(x => new VehicleDetection
                {
                    Id = x.Id,
                    CameraId = x.CameraId,
                    CameraName = x.CameraName,
                    PlateNumber = x.PlateNumber,
                    VehicleType = x.VehicleType,
                    ImageFileName = x.ImageFileName,
                    ImageUrl = x.ImageUrl,
                    DetectedAt = x.DetectedAt,
                    ConfidenceLevel = x.ConfidenceLevel,
                    XmlFileName = x.XmlFileName,
                    UUID = x.UUID,
                    Direction = x.Direction,
                    Country = x.Country
                })
                .ToListAsync();
        }


        // ============================================================
        // GET VEHICLE DETECTIONS BY CAMERA ID
        // ============================================================

        public async Task<List<VehicleDetection>> GetByCameraIdAsync(
            int cameraId)
        {
            return await _context.VehicleDetections
                .AsNoTracking()
                .Where(x => x.CameraId == cameraId)
                .OrderByDescending(x => x.DetectedAt)
                .Select(x => new VehicleDetection
                {
                    Id = x.Id,
                    CameraId = x.CameraId,
                    CameraName = x.CameraName,
                    PlateNumber = x.PlateNumber,
                    VehicleType = x.VehicleType,
                    ImageFileName = x.ImageFileName,
                    ImageUrl = x.ImageUrl,
                    DetectedAt = x.DetectedAt,
                    ConfidenceLevel = x.ConfidenceLevel,
                    XmlFileName = x.XmlFileName,
                    UUID = x.UUID,
                    Direction = x.Direction,
                    Country = x.Country
                })
                .ToListAsync();
        }


        // ============================================================
        // GET VEHICLE DETECTION BY ID
        // ============================================================

        public async Task<VehicleDetection?> GetByIdAsync(int id)
        {
            return await _context.VehicleDetections
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }


        // ============================================================
        // SEARCH BY PLATE NUMBER - ALL CAMERAS
        // ============================================================

        public async Task<List<VehicleDetection>> SearchByPlateAsync(
            string plateNumber)
        {
            plateNumber = plateNumber.Trim().ToUpper();

            return await _context.VehicleDetections
                .AsNoTracking()
                .Where(x =>
                    x.PlateNumber != null &&
                    x.PlateNumber.ToUpper().Contains(plateNumber))
                .OrderByDescending(x => x.DetectedAt)
                .Select(x => new VehicleDetection
                {
                    Id = x.Id,
                    CameraId = x.CameraId,
                    CameraName = x.CameraName,
                    PlateNumber = x.PlateNumber,
                    VehicleType = x.VehicleType,
                    ImageFileName = x.ImageFileName,
                    ImageUrl = x.ImageUrl,
                    DetectedAt = x.DetectedAt,
                    ConfidenceLevel = x.ConfidenceLevel,
                    XmlFileName = x.XmlFileName,
                    UUID = x.UUID,
                    Direction = x.Direction,
                    Country = x.Country
                })
                .ToListAsync();
        }


        // ============================================================
        // SEARCH BY PLATE NUMBER + CAMERA ID
        // ============================================================

        public async Task<List<VehicleDetection>>
            SearchByCameraAndPlateAsync(
                int cameraId,
                string plateNumber)
        {
            plateNumber = plateNumber.Trim().ToUpper();

            return await _context.VehicleDetections
                .AsNoTracking()
                .Where(x =>
                    x.CameraId == cameraId &&
                    x.PlateNumber != null &&
                    x.PlateNumber.ToUpper().Contains(plateNumber))
                .OrderByDescending(x => x.DetectedAt)
                .Select(x => new VehicleDetection
                {
                    Id = x.Id,
                    CameraId = x.CameraId,
                    CameraName = x.CameraName,
                    PlateNumber = x.PlateNumber,
                    VehicleType = x.VehicleType,
                    ImageFileName = x.ImageFileName,
                    ImageUrl = x.ImageUrl,
                    DetectedAt = x.DetectedAt,
                    ConfidenceLevel = x.ConfidenceLevel,
                    XmlFileName = x.XmlFileName,
                    UUID = x.UUID,
                    Direction = x.Direction,
                    Country = x.Country
                })
                .ToListAsync();
        }


        // ============================================================
        // GET FULL VEHICLE IMAGE
        // ============================================================

        public async Task<(byte[]? Image, string? ContentType)>
            GetFullVehicleImageAsync(int id)
        {
            var vehicle = await _context.VehicleDetections
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.FullVehicleImageBase64,
                    x.ImageFileName
                })
                .FirstOrDefaultAsync();

            if (vehicle == null ||
                string.IsNullOrWhiteSpace(
                    vehicle.FullVehicleImageBase64))
            {
                return (null, null);
            }

            try
            {
                byte[] imageBytes =
                    Convert.FromBase64String(
                        vehicle.FullVehicleImageBase64);

                string extension =
                    Path.GetExtension(
                        vehicle.ImageFileName ?? ".jpg")
                        .ToLowerInvariant();

                string contentType = extension switch
                {
                    ".png" => "image/png",
                    ".jpeg" => "image/jpeg",
                    ".jpg" => "image/jpeg",
                    _ => "image/jpeg"
                };

                return (imageBytes, contentType);
            }
            catch (FormatException)
            {
                return (null, null);
            }
        }


        // ============================================================
        // GET PLATE IMAGE
        // ============================================================

        public async Task<(byte[]? Image, string? ContentType)>
            GetPlateImageAsync(int id)
        {
            var vehicle = await _context.VehicleDetections
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.PlateImageBase64
                })
                .FirstOrDefaultAsync();

            if (vehicle == null ||
                string.IsNullOrWhiteSpace(
                    vehicle.PlateImageBase64))
            {
                return (null, null);
            }

            try
            {
                byte[] imageBytes =
                    Convert.FromBase64String(
                        vehicle.PlateImageBase64);

                return (imageBytes, "image/jpeg");
            }
            catch (FormatException)
            {
                return (null, null);
            }
        }


        // ============================================================
        // DASHBOARD
        // ============================================================

        public async Task<object> GetDashboardAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var totalVehicles =
                await _context.VehicleDetections
                    .CountAsync();

            var todayVehicles =
                await _context.VehicleDetections
                    .CountAsync(x =>
                        x.DetectedAt >= today &&
                        x.DetectedAt < tomorrow);

            var uniquePlatesToday =
                await _context.VehicleDetections
                    .Where(x =>
                        x.DetectedAt >= today &&
                        x.DetectedAt < tomorrow &&
                        x.PlateNumber != null &&
                        x.PlateNumber != "")
                    .Select(x => x.PlateNumber)
                    .Distinct()
                    .CountAsync();

            var last10 =
                await _context.VehicleDetections
                    .AsNoTracking()
                    .OrderByDescending(x => x.DetectedAt)
                    .Take(10)
                    .Select(x => new
                    {
                        x.Id,
                        x.CameraId,
                        x.CameraName,
                        x.PlateNumber,
                        x.VehicleType,
                        x.ConfidenceLevel,
                        x.Direction,
                        x.Country,
                        x.DetectedAt,

                        FullVehicleImageUrl =
                            $"/api/VehicleDetection/{x.Id}/image",

                        PlateImageUrl =
                            string.IsNullOrEmpty(
                                x.PlateImageBase64)
                                ? null
                                : $"/api/VehicleDetection/{x.Id}/plate-image"
                    })
                    .ToListAsync();

            return new
            {
                success = true,
                totalVehicles,
                todayVehicles,
                uniquePlatesToday,
                last10
            };
        }


        // ============================================================
        // SAVE IMAGE TO DATABASE
        // ============================================================

        public async Task<bool> SaveImageToDatabaseAsync(
            string fileName,
            int cameraId,
            string cameraName,
            string plateNumber,
            string vehicleType)
        {
            string imagePath =
                Path.Combine(
                    _environment.WebRootPath,
                    "Images",
                    fileName);

            try
            {
                if (!File.Exists(imagePath))
                {
                    _logger.LogWarning(
                        "Image not found: {Path}",
                        imagePath);

                    return false;
                }

                byte[] imageBytes =
                    await File.ReadAllBytesAsync(
                        imagePath);

                if (imageBytes.Length == 0)
                {
                    _logger.LogWarning(
                        "Image is empty: {FileName}",
                        fileName);

                    return false;
                }

                string imageBase64 =
                    Convert.ToBase64String(
                        imageBytes);

                var detection =
                    new VehicleDetection
                    {
                        CameraId = cameraId,

                        CameraName = cameraName,

                        PlateNumber =
                            plateNumber?
                                .Trim()
                                .ToUpper(),

                        VehicleType = vehicleType,

                        ImageFileName = fileName,

                        ImageUrl =
                            $"/Images/{fileName}",

                        FullVehicleImageBase64 =
                            imageBase64,

                        DetectedAt =
                            DateTime.Now
                    };

                _context.VehicleDetections.Add(
                    detection);

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Vehicle detection saved. " +
                    "DetectionId: {Id}, CameraId: {CameraId}, Plate: {Plate}",
                    detection.Id,
                    detection.CameraId,
                    detection.PlateNumber);

                File.Delete(imagePath);

                _logger.LogInformation(
                    "Original image deleted: {FileName}",
                    fileName);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to save image to database: {FileName}",
                    fileName);

                return false;
            }
        }
        // ============================================================
        // DASHBOARD BY CAMERA
        // ============================================================

        public async Task<object> GetDashboardByCameraIdAsync(int cameraId)
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // ------------------------------------------------------------
            // TOTAL VEHICLES FOR THIS CAMERA
            // ------------------------------------------------------------

            var totalVehicles =
                await _context.VehicleDetections
                    .CountAsync(x =>
                        x.CameraId == cameraId);


            // ------------------------------------------------------------
            // TODAY'S VEHICLES FOR THIS CAMERA
            // ------------------------------------------------------------

            var todayVehicles =
                await _context.VehicleDetections
                    .CountAsync(x =>
                        x.CameraId == cameraId &&
                        x.DetectedAt >= today &&
                        x.DetectedAt < tomorrow);


            // ------------------------------------------------------------
            // UNIQUE PLATES TODAY FOR THIS CAMERA
            // ------------------------------------------------------------

            var todayUniqueVehicles =
                await _context.VehicleDetections
                    .Where(x =>
                        x.CameraId == cameraId &&
                        x.DetectedAt >= today &&
                        x.DetectedAt < tomorrow &&
                        x.PlateNumber != null &&
                        x.PlateNumber != "")
                    .Select(x => x.PlateNumber)
                    .Distinct()
                    .CountAsync();


            // ------------------------------------------------------------
            // LAST 10 VEHICLES FOR THIS CAMERA
            // ------------------------------------------------------------

            var last10 =
                await _context.VehicleDetections
                    .AsNoTracking()
                    .Where(x =>
                        x.CameraId == cameraId)
                    .OrderByDescending(x => x.DetectedAt)
                    .Take(10)
                    .Select(x => new
                    {
                        x.Id,
                        x.CameraId,
                        x.CameraName,
                        x.PlateNumber,
                        x.VehicleType,
                        x.ConfidenceLevel,
                        x.Direction,
                        x.Country,
                        x.DetectedAt,

                        FullVehicleImageUrl =
                            $"/api/VehicleDetection/{x.Id}/image",

                        PlateImageUrl =
                            string.IsNullOrEmpty(
                                x.PlateImageBase64)
                                ? null
                                : $"/api/VehicleDetection/{x.Id}/plate-image"
                    })
                    .ToListAsync();


            // ------------------------------------------------------------
            // RETURN CAMERA-SPECIFIC DASHBOARD
            // ------------------------------------------------------------

            return new
            {
                success = true,

                cameraId,

                totalVehicles,

                todayVehicles,

                todayUniqueVehicles,

                last10
            };
        }
    }
}