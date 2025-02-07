using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;
using TechTalk.SpecFlow;

namespace MyApi.WebApi.Tests.Hooks;

[Binding]
public class DatabaseHook
{
    private static MsSqlContainer _msSqlContainer = null!;

    public static MsSqlContainer MsSqlContainer => _msSqlContainer;

    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        Task.Run(async () =>
        {
            await InitializeMsSqlContainerAsync();
            await PopulateDatabaseAsync();
        }).GetAwaiter().GetResult();
    }


    [AfterTestRun]
    public static void AfterTestRun()
    {
        Task.Run(async () =>
        {
            await _msSqlContainer.StopAsync();
            await _msSqlContainer.DisposeAsync().AsTask();
        }).GetAwaiter().GetResult();
    }

    private static async Task InitializeMsSqlContainerAsync()
    {
        _msSqlContainer = new MsSqlBuilder().Build();

        await _msSqlContainer.StartAsync();
    }

    private static async Task PopulateDatabaseAsync()
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
}
