namespace MyApi.WebApi.Tests.Features;

using BoDi;
using Hooks;
using Microcks.Testcontainers;
using MyApi.WebApi.Model;
using MyApi.WebApi.Services;
using MyApi.WebApi.Tests.Configurations;
using MyApi.WebApi.Tests.Utils;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;


[Binding]
internal class WeatherWebApiSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly IObjectContainer _objectContainer;

    internal const string ResponseKey = nameof(ResponseKey);

    public WeatherWebApiSteps(ScenarioContext scenarioContext, IObjectContainer objectContainer)
    {
        _scenarioContext = scenarioContext;
        _objectContainer = objectContainer;
    }

    [Given("the existing forecast are")]
    public void GivenTheExistingWeatherForecastAre(Table table)
    {
        var weatherContext = _objectContainer.Resolve<WeatherContext>();

        foreach (var row in table.Rows)
        {
            weatherContext.WeatherForecasts.Add(new DbWeatherForecast
            {
                Date = DateOnly.Parse(row["Date"]),
                TemperatureC = int.Parse(row["TemperatureC"]),
                Summary = row["Summary"]
            });
        }

        weatherContext.SaveChanges();
    }

    [Given("The days are:")]
    public void GivenTheDaysAre(Table table)
    {
        var days = table.Rows.Select(row => row["Time"]).ToArray();
        _scenarioContext.Add("days", days);

    }

    [Given("The minimal temperatures are:")]
    public void GivenTheMinimalTemperaturesAre(Table table)
    {
        var min = table.Rows.Select(row => row["Min"]).ToArray();
        _scenarioContext.Add("minimals", min);
    }

    [Given("The maximum temperatures are:")]
    public void GivenTheMaximumTemperaturesAre(Table table)
    {
        var max = table.Rows.Select(row => row["Max"]).ToArray();

        _scenarioContext.Add("maximums", max);

    }

    [Given("The external service forecast respond")]
    public void GivenTheExternalServiceForecastRespond()
    {
        _scenarioContext.TryGetValue("days", out String[] days);
        _scenarioContext.TryGetValue("minimals", out String[] minimals);
        _scenarioContext.TryGetValue("maximums", out String[] maximums);

        var mappedObject = new Dictionary<string, String[]>();
        mappedObject.Add("time", days);
        mappedObject.Add("temperature_2m_min", minimals);
        mappedObject.Add("temperature_2m_max", maximums);

        var httpResponse = new Response()
        {
            Daily = mappedObject,
        };

        var httpMessageResponse = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(httpResponse, SerializerOptions.SerializeOptions))
        };

        InitWebApplicationFactory.HttpMessageHandlerMeteoService.SetResponse(httpMessageResponse);

    }

    [When("I make a GET request to '(.*)'")]
    public async Task WhenIMakeAGetRequestTo(string endpoint)
    {
        var client = _scenarioContext.Get<HttpClient>(InitWebApplicationFactory.HttpClientKey);
        _scenarioContext.Add(ResponseKey, await client.GetAsync(endpoint));
    }

    [When("I make a GET request to '(.*)' with:")]
    public async Task WhenIMakeAGetRequestToWith(string endpoint, Table table)
    {
        var client = _scenarioContext.Get<HttpClient>(InitWebApplicationFactory.HttpClientKey);
        NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
        var meteoObject = table.CreateInstance<MeteoObject>();
        queryString.Add("latitude", meteoObject.latitude.ToString());
        queryString.Add("longitude", meteoObject.longitude.ToString());
        var url = $"{endpoint}?{queryString.ToString()}";
        _scenarioContext.Add(ResponseKey, await client.GetAsync(url));
    }

    [Then(@"the response status code is '(.*)'")]
    public void ThenTheResponseStatusCodeIs(int statusCode)
    {
        var expected = (HttpStatusCode)statusCode;
        Assert.Equal(expected, _scenarioContext.Get<HttpResponseMessage>(ResponseKey).StatusCode);
    }

    [Then(@"the response is")]
    public async Task ThenTheResponseIs(Table table)
    {
        var response = await _scenarioContext.Get<HttpResponseMessage>(ResponseKey).Content.ReadAsStringAsync();

        var expected = table.CreateInstance<WeatherForecast>();
        var actual = JsonSerializer.Deserialize<WeatherForecast>(response, new JsonSerializerOptions
        {
            IgnoreReadOnlyProperties = true,
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(actual);
        Assert.Equal(expected.Date, actual.Date);
        Assert.Equal(expected.TemperatureC, actual.TemperatureC);
        Assert.Equal(expected.TemperatureF, actual.TemperatureF);
        Assert.Equal(expected.Summary, actual.Summary);
    }

    [Then(@"the response with longitude '(.*)' and latitude '(.*)'")]
    public async Task ThenTheResponseWithLongitudeAndLatitude(double longitude, double latitude, Table table)
    {
        var response = await _scenarioContext.Get<HttpResponseMessage>(ResponseKey).Content.ReadAsStringAsync();

        var expected = table.CreateSet<Temperature>();
        var actual = JsonSerializer.Deserialize<MeteoServiceObject>(response, new JsonSerializerOptions
        {
            IgnoreReadOnlyProperties = true,
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(actual);
        Assert.Equal(expected.Count(), actual.temperature_By_Times.Count());
        Assert.Equal(JsonSerializer.Serialize(expected), JsonSerializer.Serialize(actual.temperature_By_Times));
    }
}