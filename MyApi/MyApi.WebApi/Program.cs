using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MyApi.WebApi;
using MyApi.WebApi.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.MapType<DateOnly>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "date"
    });
    c.SchemaFilter<SwaggerEnumSchemaFilter>();
});
builder.Logging.AddConsole();

var connectionString = builder.Configuration.GetSection("ConnectionStrings")["WeatherContext"];

builder.Services.AddDbContext<WeatherContext>(options =>
    options.UseSqlServer(connectionString, providerOptions =>
    {
        providerOptions.EnableRetryOnFailure();
    }));

builder.Services.AddTransient<IGeoCodeService, GeoCodeService>();
builder.Services.AddHttpClient<GeoCodeAPI>(client =>
{
    client.BaseAddress = new Uri("https://test.geocoding.openapi.it/");
});

builder.Services.AddTransient<IOpenMeteoService, OpenMeteoService>();
builder.Services.AddHttpClient<OpenMeteoApi>(client =>
{
    client.BaseAddress = new Uri("https://api.open-meteo.com");
});




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}
