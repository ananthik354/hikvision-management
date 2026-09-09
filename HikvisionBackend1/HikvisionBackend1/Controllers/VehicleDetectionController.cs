using HikvisionBackend1.Data;
using HikvisionBackend1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HikvisionBackend1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleDetectionController : ControllerBase
    {
        private readonly VehicleDetectionService _service;
        private readonly AppDbContext _context;
        public VehicleDetectionController(
    VehicleDetectionService service,
    AppDbContext context)
        {
            _service = service;
            _context = context;
        }

        // ============================================================
        // GET ALL DETECTIONS
        // GET /api/VehicleDetection
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var vehicles =
                    await _service.GetAllAsync();

                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message =
                        "Unable to get vehicle detections",
                    error = ex.Message
                });
            }
        }


        // ============================================================
        // GET DETECTIONS BY CAMERA
        //
        // GET /api/VehicleDetection/camera/1
        //
        // Camera 1 only
        //
        // GET /api/VehicleDetection/camera/2
        //
        // Camera 2 only
        // ============================================================

        [HttpGet("camera/{cameraId:int}")]
        public async Task<IActionResult>
            GetByCameraId(int cameraId)
        {
            try
            {
                if (cameraId <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Invalid camera ID"
                    });
                }

                var vehicles =
                    await _service
                        .GetByCameraIdAsync(cameraId);

                return Ok(new
                {
                    success = true,
                    cameraId = cameraId,
                    count = vehicles.Count,
                    data = vehicles
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message =
                        "Unable to get vehicle detections for camera",
                    error = ex.Message
                });
            }
        }


        // ============================================================
        // GET VEHICLE BY ID
        //
        // GET /api/VehicleDetection/1
        // ============================================================

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(
            int id)
        {
            try
            {
                var vehicle =
                    await _service.GetByIdAsync(id);

                if (vehicle == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message =
                            "Vehicle detection not found"
                    });
                }

                return Ok(vehicle);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message =
                        "Unable to get vehicle detection",
                    error = ex.Message
                });
            }
        }


        // ============================================================
        // SEARCH PLATE - ALL CAMERAS
        //
        // GET /api/VehicleDetection/search/TN33CC0407
        // ============================================================

        [HttpGet("search/{plateNumber}")]
        public async Task<IActionResult> Search(
            string plateNumber)
        {
            try
            {
                var vehicles =
                    await _service
                        .SearchByPlateAsync(
                            plateNumber);

                return Ok(new
                {
                    success = true,
                    count = vehicles.Count,
                    data = vehicles
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message =
                        "Unable to search vehicle detections",
                    error = ex.Message
                });
            }
        }


        // ============================================================
        // SEARCH PLATE BY CAMERA
        //
        // GET
        // /api/VehicleDetection/camera/1/search/TN33CC0407
        // ============================================================

        [HttpGet(
            "camera/{cameraId:int}/search/{plateNumber}")]
        public async Task<IActionResult>
            SearchByCamera(
                int cameraId,
                string plateNumber)
        {
            try
            {
                if (cameraId <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Invalid camera ID"
                    });
                }

                var vehicles =
                    await _service
                        .SearchByCameraAndPlateAsync(
                            cameraId,
                            plateNumber);

                return Ok(new
                {
                    success = true,
                    cameraId = cameraId,
                    count = vehicles.Count,
                    data = vehicles
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message =
                        "Unable to search vehicle detections",
                    error = ex.Message
                });
            }
        }


        // ============================================================
        // FULL VEHICLE IMAGE
        //
        // GET /api/VehicleDetection/1/image
        // ============================================================

        [HttpGet("{id:int}/image")]
        public async Task<IActionResult>
            GetFullVehicleImage(int id)
        {
            try
            {
                var result =
                    await _service
                        .GetFullVehicleImageAsync(id);

                if (result.Image == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message =
                            "Vehicle image not found"
                    });
                }

                return File(
                    result.Image,
                    result.ContentType ??
                        "image/jpeg");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message =
                        "Unable to get vehicle image",
                    error = ex.Message
                });
            }
        }


        // ============================================================
        // PLATE IMAGE
        //
        // GET /api/VehicleDetection/1/plate-image
        // ============================================================

        [HttpGet("{id:int}/plate-image")]
        public async Task<IActionResult>
            GetPlateImage(int id)
        {
            try
            {
                var result =
                    await _service
                        .GetPlateImageAsync(id);

                if (result.Image == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message =
                            "Plate image not found"
                    });
                }

                return File(
                    result.Image,
                    result.ContentType ??
                        "image/jpeg");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message =
                        "Unable to get plate image",
                    error = ex.Message
                });
            }
        }


        // ============================================================
        // DASHBOARD
        //
        // GET /api/VehicleDetection/dashboard
        // ============================================================


        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var result =
                    await _service.GetDashboardAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message =
                        "Unable to get dashboard data",
                    error = ex.Message
                });
            }
        }
        // ============================================================
        // DASHBOARD BY CAMERA
        //
        // GET /api/VehicleDetection/dashboard/1
        // GET /api/VehicleDetection/dashboard/2
        // ============================================================

        [HttpGet("dashboard/{cameraId:int}")]
        public async Task<IActionResult> GetDashboard(int cameraId)
        {
            try
            {
                if (cameraId <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid camera ID"
                    });
                }

                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                // ------------------------------------------------------------
                // TOTAL VEHICLES FOR THIS CAMERA
                // ------------------------------------------------------------

                var totalVehicles = await _context.VehicleDetections
                    .AsNoTracking()
                    .CountAsync(x => x.CameraId == cameraId);


                // ------------------------------------------------------------
                // TODAY'S VEHICLES FOR THIS CAMERA
                // ------------------------------------------------------------

                var todayVehicles = await _context.VehicleDetections
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.CameraId == cameraId &&
                        x.DetectedAt >= today &&
                        x.DetectedAt < tomorrow);


                // ------------------------------------------------------------
                // UNIQUE PLATES TODAY
                // ------------------------------------------------------------

                var todayUniqueVehicles = await _context.VehicleDetections
                    .AsNoTracking()
                    .Where(x =>
                        x.CameraId == cameraId &&
                        x.DetectedAt >= today &&
                        x.DetectedAt < tomorrow &&
                        x.PlateNumber != null &&
                        x.PlateNumber != "")
                    .Select(x => x.PlateNumber!)
                    .Distinct()
                    .CountAsync();


                // ------------------------------------------------------------
                // LAST 10 DETECTIONS
                // ------------------------------------------------------------

                var last10 = await _context.VehicleDetections
                    .AsNoTracking()
                    .Where(x => x.CameraId == cameraId)
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

                        fullVehicleImageUrl =
                            $"/api/VehicleDetection/{x.Id}/image",

                        plateImageUrl =
                            x.PlateImageBase64 != null
                                ? $"/api/VehicleDetection/{x.Id}/plate-image"
                                : null
                    })
                    .ToListAsync();


                // ------------------------------------------------------------
                // RETURN DASHBOARD DATA
                // ------------------------------------------------------------

                return Ok(new
                {
                    success = true,
                    cameraId = cameraId,
                    totalVehicles = totalVehicles,
                    todayVehicles = todayVehicles,
                    todayUniqueVehicles = todayUniqueVehicles,
                    last10 = last10
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Unable to load camera dashboard",
                    error = ex.Message
                });
            }
        }
    }
}