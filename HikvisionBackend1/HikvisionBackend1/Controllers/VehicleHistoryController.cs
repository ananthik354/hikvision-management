using HikvisionBackend1.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HikvisionBackend1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleHistoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VehicleHistoryController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET ALL
        // /api/VehicleHistory
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var history = await _context.VehicleHistory
                    .AsNoTracking()
                    .OrderByDescending(x => x.DetectedAt)
                    .Select(x => new
                    {
                        x.Id,
                        x.RegisteredVehicleId,
                        x.VehicleDetectionId,
                        x.PlateNumber,
                        x.VehicleType,
                        x.FullVehicleImageBase64,
                        x.PlateImageBase64,
                        x.DetectedAt,
                        x.MatchedAt,
                        x.MatchStatus,
                        x.Triggered
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    count = history.Count,
                    data = history
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Unable to get vehicle history",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // GET BY CAMERA
        // /api/VehicleHistory/camera/1
        // /api/VehicleHistory/camera/2
        // ============================================================

        [HttpGet("camera/{cameraId:int}")]
        public async Task<IActionResult> GetByCamera(int cameraId)
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

                var history = await _context.VehicleHistory
                    .AsNoTracking()
                    .Where(x =>
                        x.VehicleDetection != null &&
                        x.VehicleDetection.CameraId == cameraId)
                    .OrderByDescending(x => x.DetectedAt)
                    .Select(x => new
                    {
                        x.Id,
                        x.RegisteredVehicleId,
                        x.VehicleDetectionId,

                        x.PlateNumber,
                        x.VehicleType,

                        x.FullVehicleImageBase64,
                        x.PlateImageBase64,

                        x.DetectedAt,
                        x.MatchedAt,

                        x.MatchStatus,
                        x.Triggered,

                        cameraId = x.VehicleDetection!.CameraId,
                        cameraName = x.VehicleDetection.CameraName,

                        confidenceLevel =
                            x.VehicleDetection.ConfidenceLevel,

                        direction =
                            x.VehicleDetection.Direction,

                        country =
                            x.VehicleDetection.Country,

                        vehicleName =
                            x.RegisteredVehicle != null
                                ? x.RegisteredVehicle.PlateNumber
                                : null
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    cameraId = cameraId,
                    count = history.Count,
                    data = history
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Unable to get vehicle history for camera",
                    error = ex.Message
                });
            }
        }
        // ============================================================
        // GET BY PLATE
        // /api/VehicleHistory/plate/TN33AA1085
        // ============================================================

        [HttpGet("plate/{plateNumber}")]
        public async Task<IActionResult> GetByPlate(string plateNumber)
        {
            try
            {
                plateNumber = plateNumber.Trim().ToUpper();

                var history = await _context.VehicleHistory
                    .AsNoTracking()
                    .Where(x =>
                        x.PlateNumber == plateNumber)
                    .OrderByDescending(x => x.DetectedAt)
                    .Select(x => new
                    {
                        x.Id,
                        x.RegisteredVehicleId,
                        x.VehicleDetectionId,
                        x.PlateNumber,
                        x.VehicleType,
                        x.FullVehicleImageBase64,
                        x.PlateImageBase64,
                        x.DetectedAt,
                        x.MatchedAt,
                        x.MatchStatus,
                        x.Triggered
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    count = history.Count,
                    data = history
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Unable to get vehicle history",
                    error = ex.Message
                });
            }
        }
    }
}