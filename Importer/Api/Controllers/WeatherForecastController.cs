using Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IBlobStorage _blob;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IBlobStorage blob)
        {
            _logger = logger;
            _blob = blob;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            var response = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

            var json = System.Text.Json.JsonSerializer.Serialize(response);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            _blob.SaveAsync("weatherforecast", "data.json", stream).GetAwaiter().GetResult();

            return response;
        }
    }
}
