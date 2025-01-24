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
    private readonly IGeoCodeService _geoCodeApi;

    public WeatherForecastController(WeatherContext weatherContext, IOpenMeteoService openMeteoApi, IGeoCodeService geoCodeApi)
    {
        _weatherContext = weatherContext;
        _openMeteoApi = openMeteoApi;
        _geoCodeApi = geoCodeApi;
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
    public async Task<MeteoServiceObject?> Get([FromQuery] string adresse)
    {
        var geoCodes = await _geoCodeApi.GetLatitudeLongitude(adresse);
        return await _openMeteoApi.getMeteo(geoCodes.Latitude, geoCodes.Longitude);
    }
}