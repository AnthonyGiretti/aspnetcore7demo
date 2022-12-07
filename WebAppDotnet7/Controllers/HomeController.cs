using Microsoft.AspNetCore.Mvc;

namespace WebAppDotnet7.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public ActionResult MyHost(IHttpContextAccessor httpAccessor)
        {
            return Ok(new { httpAccessor.HttpContext.Request.Host });
        }
    }
}
