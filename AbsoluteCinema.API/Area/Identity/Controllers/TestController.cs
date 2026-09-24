using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AbsoluteCinema.API.Area.Identity.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Working");
        }
    }
}
