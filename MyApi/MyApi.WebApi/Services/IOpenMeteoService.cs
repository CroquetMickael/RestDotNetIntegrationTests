namespace MyApi.WebApi.Services
{
    public interface IOpenMeteoService
    {
        Task<MeteoServiceObject> getMeteo(float latitude, float longitude);
    }
}
