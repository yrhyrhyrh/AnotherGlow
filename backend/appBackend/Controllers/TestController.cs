using Microsoft.AspNetCore.Mvc;
using MyAppBackend.Models;
using MyAppBackend.Services;

namespace MyAppBackend.Controllers
{
    //do not remove! using for aws healthcheck
    [ApiController]
    [Route("/")]
    public class RootController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok("Backend is running");
        }
    }

}
