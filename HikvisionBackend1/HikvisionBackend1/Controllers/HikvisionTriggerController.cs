using HikvisionBackend1.Services;
using Microsoft.AspNetCore.Mvc;

namespace HikvisionBackend1.Controllers
{
    [ApiController]
    [Route("api/hikvision/trigger")]
    public class HikvisionTriggerController : ControllerBase
    {
        private readonly HikvisionTriggerService _triggerService;
        private readonly ILogger<HikvisionTriggerController> _logger;

        public HikvisionTriggerController(
            HikvisionTriggerService triggerService,
            ILogger<HikvisionTriggerController> logger)
        {
            _triggerService = triggerService;
            _logger = logger;
        }


        // ============================================================
        // POST /api/hikvision/trigger
        // ============================================================

        [HttpPost]
        public async Task<IActionResult> Trigger()
        {
            _logger.LogInformation(
                "==========================================");

            _logger.LogInformation(
                "SOFTWARE -> HIKVISION CAMERA TRIGGER");

            var result =
                await _triggerService.TriggerCameraAsync();


            if (!result.Success)
            {
                _logger.LogWarning(
                    "Hikvision trigger failed: {Message}",
                    result.Message);

                return BadRequest(new
                {
                    success = false,
                    message = result.Message,
                    response = result.Response
                });
            }


            _logger.LogInformation(
                "Hikvision trigger successful.");

            _logger.LogInformation(
                "==========================================");


            return Ok(new
            {
                success = true,
                message = result.Message,
                response = result.Response
            });
        }
    }
}