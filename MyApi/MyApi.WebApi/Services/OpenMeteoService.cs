using MyApi.WebApi.Services;

public class OpenMeteoService : IOpenMeteoService
{

    protected readonly OpenMeteoApi _openMeteo;

    public OpenMeteoService(OpenMeteoApi openMeteo)
    {
        _openMeteo = openMeteo;
    }

    public async Task<MeteoServiceObject> GetMeteo(
        float latitude,
        float longitude)
    {
        var response = await _openMeteo.ForecastAsync([], [Anonymous2.Temperature_2m_max, Anonymous2.Temperature_2m_min], latitude, longitude, false, Temperature_unit.Celsius, null, null, "GMT", null);
        return MapOpenMeteoApiResponse(response);
    }

    private MeteoServiceObject MapOpenMeteoApiResponse(Response meteoApiResponse)
    {
        var meteoService = new MeteoServiceObject();
        var WeatherDataByDay = new List<Temperature>();
        meteoService.Latitude = meteoApiResponse.Latitude;
        meteoService.Longitude = meteoApiResponse.Longitude;
        DailyResponse dailyResponse = meteoApiResponse.Daily;
        for (int i = 0; i < dailyResponse.Time.Count; i++)
        {
            Temperature temperature = new Temperature
            {
                Min = $"{dailyResponse.Temperature_2m_min.ToArray()[i]}",
                Max = $"{dailyResponse.Temperature_2m_max.ToArray()[i]}",
                Date = dailyResponse.Time.ToArray()[i]
            };
            WeatherDataByDay.Add(temperature);
        }
        meteoService.Temperature_By_Times = WeatherDataByDay;

        return meteoService;
    }
}

public class WeatherData
{
    public List<string> Time { get; set; }
    public List<double> Temperature_2m_max { get; set; }
    public List<double> Temperature_2m_min { get; set; }
}