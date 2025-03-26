using Microsoft.AspNetCore.Mvc;

namespace Bonus_Implementation_Policy_WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        public UserController()
        {

        }

        [HttpGet("Hello")]
        public IActionResult Get()
        {
            return Ok(new { message = "Hello from this world " });
        }

    }
}
