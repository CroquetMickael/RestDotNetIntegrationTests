using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Text.Json.Serialization;

namespace MyApi.WebApi.Services
{
    public class OpenMeteoService : IOpenMeteoService
    {
        protected readonly OpenMeteoApi _openMeteo;

        public OpenMeteoService(OpenMeteoApi openMeteo)
        {
            _openMeteo = openMeteo;
        }

        public async Task<MeteoServiceObject> getMeteo(float latitude, float longitude)
        {
            var response = await _openMeteo.ForecastAsync(
                [],
                [Anonymous2.Temperature_2m_max, Anonymous2.Temperature_2m_min],
                latitude,
                longitude,
                false,
                Temperature_unit.Celsius,
                null,
                null,
                "GMT",
                null);

            return mapOpenMeteoApiResponse(response);
        }

        private MeteoServiceObject mapOpenMeteoApiResponse(Response meteoApiResponse)
        {
            var meteoService = new MeteoServiceObject();
            var WeatherDataByDay = new List<Temperature>();
            meteoService.latitude = meteoApiResponse.Latitude;
            meteoService.longitude = meteoApiResponse.Longitude;
            WeatherData weatherData = JsonConvert.DeserializeObject<WeatherData>(meteoApiResponse.Daily.ToString());
            for (int i = 0; i < weatherData.Time.Count; i++)
            {
                Temperature temperature = new Temperature
                {
                    min = $"{weatherData.Temperature_2m_min[i]}",
                    max = $"{weatherData.Temperature_2m_max[i]}",
                    date = weatherData.Time[i]
                };
                WeatherDataByDay.Add(temperature);
            }
            meteoService.temperature_By_Times = WeatherDataByDay;

            return meteoService;
        }

    }

    public class WeatherData
    {
        public List<string> Time { get; set; }
        public List<double> Temperature_2m_max { get; set; }
        public List<double> Temperature_2m_min { get; set; }
    }

}