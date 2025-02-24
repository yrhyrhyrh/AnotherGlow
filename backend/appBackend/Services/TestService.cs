using MyAppBackend.Models;
using MyAppBackend.Repositories;

namespace MyAppBackend.Services
{
    public interface IWeatherForecastService
    {
        IEnumerable<WeatherForecast> GetWeatherForecast();
    }

    public class WeatherForecastService : IWeatherForecastService
    {
        private readonly IWeatherForecastRepository _weatherForecastRepository;

        public WeatherForecastService(IWeatherForecastRepository weatherForecastRepository)
        {
            _weatherForecastRepository = weatherForecastRepository;
        }

        public IEnumerable<WeatherForecast> GetWeatherForecast()
        {
            return _weatherForecastRepository.GetForecast();
        }
    }
}
