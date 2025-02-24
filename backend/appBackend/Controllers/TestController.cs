using Microsoft.AspNetCore.Mvc;
using MyAppBackend.Models;
using MyAppBackend.Services;

namespace MyAppBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IWeatherForecastService _weatherForecastService;

        public TestController(IWeatherForecastService weatherForecastService)
        {
            _weatherForecastService = weatherForecastService;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> GetWeatherForecast()
        {
            return _weatherForecastService.GetWeatherForecast();
        }
    }
}
