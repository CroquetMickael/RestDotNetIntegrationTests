using BoDi;
using Microcks.Testcontainers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MyApi.WebApi.Services;
using MyApi.WebApi.Tests.Utils;
using Respawn;
using TechTalk.SpecFlow;
using Testcontainers.MsSql;

namespace MyApi.WebApi.Tests.Hooks;

[Binding]
internal class InitWebApplicationFactory
{
    internal const string HttpClientKey = nameof(HttpClientKey);
    internal const string ApplicationKey = nameof(ApplicationKey);
    private MsSqlContainer _msSqlContainer = null!;
    private MicrocksContainer _microcksContainer = null!;
    internal MicrocksURI _microcksUrl = new MicrocksURI();
    public static HttpMessageHandlerMock HttpMessageHandlerMeteoService { get; set; } = new();

    private static void RemoveLogging(IServiceCollection services)
    {
        services.RemoveAll(typeof(ILogger<>));
        services.RemoveAll<ILogger>();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
    }

    private void ReplaceDatabase(IServiceCollection services, IObjectContainer objectContainer)
    {
        services.RemoveAll<DbContextOptions<WeatherContext>>();
        services.RemoveAll<WeatherContext>();

        services.AddDbContext<WeatherContext>(options =>
            options.UseSqlServer(_msSqlContainer.GetConnectionString(), providerOptions =>
            {
                providerOptions.EnableRetryOnFailure();
            }));

        var database = new WeatherContext(new DbContextOptionsBuilder<WeatherContext>()
            .UseSqlServer(_msSqlContainer.GetConnectionString())
            .Options);

        objectContainer.RegisterInstanceAs(database);
    }

    private async Task InitializeRespawnAsync()
    {
        var respawner = await Respawner.CreateAsync(
            _msSqlContainer.GetConnectionString(),
            new()
            {
                DbAdapter = DbAdapter.SqlServer,
            });

        await respawner.ResetAsync(_msSqlContainer.GetConnectionString());
    }

    private async Task PopulateDatabaseAsync()
    {
        await using SqlConnection sqlConnection = new SqlConnection(_msSqlContainer.GetConnectionString());

        await using var sqlCommand = new SqlCommand
        {
            Connection = sqlConnection,
            CommandText = @"
            CREATE TABLE [dbo].[WeatherForecast] (
                [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [Date] DATE NOT NULL,
                [TemperatureC] INT NOT NULL,
                [Summary] NVARCHAR(2000) NULL
            );
        "
        };

        sqlConnection.Open();

        await sqlCommand.ExecuteNonQueryAsync();
    }

    private async Task CreateApiTestcontainer()
    {
        _microcksContainer = new MicrocksBuilder()
           .WithImage("quay.io/microcks/microcks-uber:1.10.0")
            .WithMainArtifacts("openapi2.yml",
            "geocode.yaml")
            .WithSecondaryArtifacts("Mocks\\OpenMeteo\\openmeteomocks.yml",
            "Mocks\\GeoCode\\geoCodeMocks.yml")
           .Build();
        await _microcksContainer.StartAsync();
        _microcksUrl.OpenMeteoURI = _microcksContainer.GetRestMockEndpoint("Open-Meteo APIs", "1.0/v1");
        _microcksUrl.GeoCodeURI = _microcksContainer.GetRestMockEndpoint("Geocoding Api", "1.0.0/");
    }

    private static void ReplaceExternalServices(IServiceCollection services, ScenarioContext scenarioContext, MicrocksURI microcksUrls)
    {
        services.AddHttpClient<OpenMeteoApi>(client =>
        {
            client.BaseAddress = microcksUrls.OpenMeteoURI;
        });
        services.AddHttpClient<GeoCodeAPI>(client =>
        {
            client.BaseAddress = microcksUrls.GeoCodeURI;
        });
        //.ConfigurePrimaryHttpMessageHandler(() =>
        //{
        //    return HttpMessageHandlerMeteoService;
        //});
    }

    [BeforeScenario]
    public async Task BeforeScenario(ScenarioContext scenarioContext, IObjectContainer objectContainer)
    {
        _msSqlContainer = new MsSqlBuilder().Build();
        await _msSqlContainer.StartAsync();
        await PopulateDatabaseAsync();
        await InitializeRespawnAsync();
        await CreateApiTestcontainer();
        var application = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                RemoveLogging(services);
                ReplaceDatabase(services, objectContainer);
                ReplaceExternalServices(services, scenarioContext, _microcksUrl);
            });
        });

        var client = application.CreateClient();

        scenarioContext.TryAdd(HttpClientKey, client);
        scenarioContext.TryAdd(ApplicationKey, application);
    }

    [AfterScenario]
    public async Task AfterScenario(ScenarioContext scenarioContext)
    {
        if (scenarioContext.TryGetValue(HttpClientKey, out var client) && client is IDisposable disposable)
        {
            disposable.Dispose();
        }

        if (scenarioContext.TryGetValue(ApplicationKey, out var application) && application is IDisposable disposableApplication)
        {
            disposableApplication.Dispose();
        }

        await _msSqlContainer.StopAsync();
        await _msSqlContainer.DisposeAsync().AsTask();
    }

}