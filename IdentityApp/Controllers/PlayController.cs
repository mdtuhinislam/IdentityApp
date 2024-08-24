using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PlayController : ControllerBase
    {
        public PlayController()
        {
            
        }

        [HttpGet("get-players")]
        public IActionResult GetPlayers()
        {
            return new JsonResult(new { Message = "I am from player controller" });
        }
    }
}
