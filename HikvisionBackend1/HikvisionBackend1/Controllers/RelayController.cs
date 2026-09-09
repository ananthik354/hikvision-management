using HikvisionBackend1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HikvisionBackend1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RelayController : ControllerBase
    {
        [HttpPost("open")]
        public IActionResult OpenRelay()
        {
            var result = new RelayResult
            {
                Success = true,
                Action = "OPEN",
                Message = "Relay opened successfully",
                ExecutedAt = DateTime.Now
            };

            return Ok(result);
        }

        [HttpPost("close")]
        public IActionResult CloseRelay()
        {
            var result = new RelayResult
            {
                Success = true,
                Action = "CLOSE",
                Message = "Relay closed successfully",
                ExecutedAt = DateTime.Now
            };

            return Ok(result);
        }
    }
}
