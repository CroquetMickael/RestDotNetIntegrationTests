namespace MyApi.WebApi.Services
{
    public interface IGeoCodeService
    {
        public Task<GeoCodeObject> GetLatitudeLongitude(string adresse);

    }
}
