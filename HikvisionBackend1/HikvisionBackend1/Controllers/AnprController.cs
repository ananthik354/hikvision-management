using HikvisionBackend1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HikvisionBackend1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnprController : ControllerBase
    {
        [HttpGet("latest")]
        public IActionResult GetLatest()
        {
            var anprEvent = new AnprEvent
            {
                Id = 1,
                CameraId = 1,
                CameraName = "Entrance Camera",
                PlateNumber = "TN38AB1234",
                VehicleType = "Motorcycle",
                ImageUrl = "/images/bike.jpg",
                DetectedAt = DateTime.Now
            };

            return Ok(anprEvent);
        }
    }
}
