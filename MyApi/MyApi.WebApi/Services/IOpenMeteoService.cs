namespace MyApi.WebApi.Services
{
    public interface IOpenMeteoService
    {
        Task<MeteoServiceObject> GetMeteo(
            float latitude,
            float longitude);
    }
}
