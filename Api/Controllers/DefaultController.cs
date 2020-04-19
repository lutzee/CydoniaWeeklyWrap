using Microsoft.AspNetCore.Mvc;

namespace Cww.Api.Controllers
{
    [Route("")]
    [ApiController]
    public class DefaultController : ControllerBase
    {
        [HttpGet]
        public ActionResult Get()
        {
            return CreatedAtAction(nameof(Get), null, new { System = "Cydonia Weekly Wrap V0.0.1" });
        }
    }
}