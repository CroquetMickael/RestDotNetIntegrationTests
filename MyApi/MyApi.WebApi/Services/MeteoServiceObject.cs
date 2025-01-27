namespace MyApi.WebApi.Services
{
    public class MeteoServiceObject
    {
        public double latitude { get; set; }

        public double longitude { get; set; }

        public IEnumerable<Temperature> temperature_By_Times  {get; set;}
    }

    public class Temperature
    {
        public string date { get; set; }

        public string min { get; set; }

        public string max { get; set; }
    }
}