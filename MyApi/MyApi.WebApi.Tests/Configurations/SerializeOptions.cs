using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace MyApi.WebApi.Tests.Configurations;

public static class SerializerOptions
{
    public static readonly JsonSerializerOptions SerializeOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static readonly JsonSerializerOptions DeserializeOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };
}
