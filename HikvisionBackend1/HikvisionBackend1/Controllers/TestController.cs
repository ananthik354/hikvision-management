using Microsoft.AspNetCore.Mvc;

namespace HikvisionBackend1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : Controller
    {
        [HttpGet]
        public IActionResult Test()
        {
            return Ok(new
            {
                success = true,
                message = "Hikvision backend is working"
            });
        }
    }
}
