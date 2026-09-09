using HikvisionBackend1.Data;
using HikvisionBackend1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HikvisionBackend1.Controllers
{
    [ApiController]
    [Route("api/hikvision/barrier")]
    public class HikvisionBarrierController : ControllerBase
    {
        private readonly IHikvisionBarrierService _barrierService;
        private readonly AppDbContext _db;

        public HikvisionBarrierController(
            IHikvisionBarrierService barrierService,
            AppDbContext db)
        {
            _barrierService = barrierService;
            _db = db;
        }

        // POST /api/hikvision/barrier/open/1
        [HttpPost("open/{cameraId}")]
        public async Task<IActionResult> Open(int cameraId)
        {
            var camera =
                await _db.Cameras
                    .FirstOrDefaultAsync(x => x.Id == cameraId);

            if (camera == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Camera not found."
                });
            }

            if (string.IsNullOrWhiteSpace(camera.HikvisionUsername) ||
                string.IsNullOrWhiteSpace(camera.HikvisionPassword))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Hikvision username/password not configured."
                });
            }

            bool result =
                await _barrierService.OpenBarrierAsync(
                    camera.IpAddress,
                    camera.HttpPort,
                    camera.HikvisionUsername,
                    camera.HikvisionPassword,
                    camera.IsapiChannel);

            if (!result)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to open Hikvision barrier."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Hikvision barrier opened.",
                cameraId = camera.Id,
                camera = camera.Name,
                ipAddress = camera.IpAddress
            });
        }


        // POST /api/hikvision/barrier/close/1
        [HttpPost("close/{cameraId}")]
        public async Task<IActionResult> Close(int cameraId)
        {
            var camera =
                await _db.Cameras
                    .FirstOrDefaultAsync(x => x.Id == cameraId);

            if (camera == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Camera not found."
                });
            }

            if (string.IsNullOrWhiteSpace(camera.HikvisionUsername) ||
                string.IsNullOrWhiteSpace(camera.HikvisionPassword))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Hikvision username/password not configured."
                });
            }

            bool result =
                await _barrierService.CloseBarrierAsync(
                    camera.IpAddress,
                    camera.HttpPort,
                    camera.HikvisionUsername,
                    camera.HikvisionPassword,
                    camera.IsapiChannel);

            if (!result)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to close Hikvision barrier."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Hikvision barrier closed.",
                cameraId = camera.Id,
                camera = camera.Name,
                ipAddress = camera.IpAddress
            });
        }
    }
}