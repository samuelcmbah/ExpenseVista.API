using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseVista.API.Controllers
{
    [Route("api/test")]
    [ApiController]
    [AllowAnonymous]
    public class TestController : ControllerBase
    {

        [HttpGet]
        public IActionResult GetValues()
        {
            return Ok(new { message = "The api is working well" });
        }
    }
}
