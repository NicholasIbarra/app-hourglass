using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class WeatherController : ControllerBase
{
    [HttpGet("weather/today")]
    public IActionResult GetTodayWeather()
    {
        var weatherInfo = new
        {
            Date = DateTime.Now,
            TemperatureC = 22,
            Summary = "Sunny"
        };
        return Ok(weatherInfo);
    }
}
