namespace MyApi.WebApi.Services
{
    public class MeteoServiceObject
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public IEnumerable<Temperature> Temperature_By_Times  {get; set;}
    }

    public class Temperature
    {
        public string Date { get; set; }

        public string Min { get; set; }

        public string Max { get; set; }
    }
}