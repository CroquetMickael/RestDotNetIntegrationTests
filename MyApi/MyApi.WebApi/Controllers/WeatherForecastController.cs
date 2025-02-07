using Microsoft.AspNetCore.Mvc;

namespace MyApi.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly WeatherContext _weatherContext;

    public WeatherForecastController(WeatherContext weatherContext)
    {
        _weatherContext = weatherContext;
    }

    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {
        var allWeathers = _weatherContext.WeatherForecasts.ToList();

        return allWeathers.Select(dbWeather => new WeatherForecast
        {
            Date = dbWeather.Date,
            TemperatureC = dbWeather.TemperatureC,
            Summary = dbWeather.Summary
        });
    }

    [HttpGet]
    [Route("{date}")]
    public WeatherForecast? Get(DateOnly date)
    {
        var dbWeather = _weatherContext.WeatherForecasts.FirstOrDefault(w => w.Date == date);

        if (dbWeather == null)
        {
            return null;
        }

        return new WeatherForecast
        {
            Date = dbWeather.Date,
            TemperatureC = dbWeather.TemperatureC,
            Summary = dbWeather.Summary
        };
    }

    [HttpPost]
    public IActionResult Post([FromBody] WeatherForecast weatherForecast)
    {
        _weatherContext.WeatherForecasts.Add(new DbWeatherForecast
        {
            Date = weatherForecast.Date,
            TemperatureC = weatherForecast.TemperatureC,
            Summary = weatherForecast.Summary
        });

        _weatherContext.SaveChanges();

        return Ok();
    }
}
