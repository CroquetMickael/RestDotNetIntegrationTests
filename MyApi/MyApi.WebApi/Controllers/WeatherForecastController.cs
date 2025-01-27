using Microsoft.AspNetCore.Mvc;
using MyApi.WebApi.Model;
using MyApi.WebApi.Services;

namespace MyApi.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly WeatherContext _weatherContext;
    private readonly IOpenMeteoService _openMeteoApi;

    public WeatherForecastController(WeatherContext weatherContext, IOpenMeteoService openMeteoApi)
    {
        _weatherContext = weatherContext;
        _openMeteoApi = openMeteoApi;
    }

    [HttpGet(Name = "GetWeatherForecast")]
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

    [HttpGet]
    [Route("SevenDayMinMax")]
    public async Task<MeteoServiceObject?> Get([FromQuery] MeteoObject meteo)
    {
        return await _openMeteoApi.getMeteo(meteo.Latitude, meteo.Longitude);
    }
}