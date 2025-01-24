namespace MyApi.WebApi.Services
{
    public class GeoCodeService: IGeoCodeService
    {

        protected readonly GeoCodeAPI _geoCodeAPI;

        public GeoCodeService(GeoCodeAPI geoCodeAPI)
        {
            _geoCodeAPI = geoCodeAPI;
        }


        public async Task<GeoCodeObject> GetLatitudeLongitude(string adresse)
        {
            var GeoCodeAdresse = new Address
            {
                Address1 = adresse
            };
            var response = await _geoCodeAPI.GeocodeAsync(GeoCodeAdresse);

            return new GeoCodeObject
            {
                Latitude = response.Elements.Element.Latitude,
                Longitude = response.Elements.Element.Longitude
            };
        }
    }
}
