using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Dapper;
using Npgsql;

namespace PSP.Tests;

public class TestDatabase : IAsyncLifetime
{
    private const string ConnectionPostgresDatabase = "Host=database;Username=postgres;Password=pspsecret;Database=postgres;Pooling=false;Include Error Detail=true;";
    private const string ConnectionTestDatabase = "Host=database;Username=postgres;Password=pspsecret;Database=test;Pooling=false";

    public async Task InitializeAsync()
    {
        Console.WriteLine("Roda 5");
        await using (var connection = new NpgsqlConnection(ConnectionPostgresDatabase))
        {
            await connection.ExecuteAsync("DROP DATABASE IF EXISTS test;");
            await connection.ExecuteAsync("CREATE DATABASE test;");
        }
        await using (var connection = new NpgsqlConnection(ConnectionTestDatabase))
        {
            var initSqlFileContent = await File.ReadAllTextAsync("/source/init.sql");
            await connection.ExecuteAsync(initSqlFileContent);
        }
        Environment.SetEnvironmentVariable("POSTGRES_DATABASE", "test");
    }

    public async Task DisposeAsync()
    {
        Console.WriteLine("Roda 6");
        await using var connection = new NpgsqlConnection(ConnectionPostgresDatabase);
        await connection.ExecuteAsync("DROP DATABASE test;");
    }
}
public class UnitTest1 : TestDatabase, IClassFixture<WebApplicationFactory<Program>>
{
    // private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    
    public UnitTest1(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }
    
    [Fact]
    public async Task ShouldSignUpUserCorrectly()
    {
        Console.WriteLine("Roda 1");
        using var client = _factory.CreateClient();
        var response = await client.PostAsync("/signup", JsonContent.Create(new { username = "Miguel", password = "any_password" }));
        await _factory.DisposeAsync();
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var expected = JsonSerializer.Serialize(new { id = 1, username = "Miguel" });
        var actual = await response.Content.ReadAsStringAsync();
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public async Task ShouldReturnThatUsernameIsRequired()
    {
        Console.WriteLine("Roda 2");
        await using var application = new WebApplicationFactory<Program>();
        using var client = application.CreateClient();
        
        var response = await client.PostAsync("/signup", JsonContent.Create(new { password = "any_password" }));
    
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var expected = JsonSerializer.Serialize(new { message = "The username field is required." });
        var actual = await response.Content.ReadAsStringAsync();
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public async Task ShouldReturnThatPasswordIsRequired()
    {
        Console.WriteLine("Roda 3");
        await using var application = new WebApplicationFactory<Program>();
        using var client = application.CreateClient();
        
        var response = await client.PostAsync("/signup", JsonContent.Create(new { username = "Miguel" }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var expected = JsonSerializer.Serialize(new { message = "The password field is required." });
        var actual = await response.Content.ReadAsStringAsync();
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public async Task ShouldReturnUserAlreadyInUse()
    {
        Console.WriteLine("Roda 4");
        await using var application = new WebApplicationFactory<Program>();
        using var client = application.CreateClient();
        
        await client.PostAsync("/signup", JsonContent.Create(new { username = "Miguel", password = "any_password" }));
        
        var response = await client.PostAsync("/signup", JsonContent.Create(new { username = "Miguel", password = "any_password" }));

        var expected = JsonSerializer.Serialize(new { message = "This user is already in use." });
        var actual = await response.Content.ReadAsStringAsync();
        Assert.Equal(expected, actual);
    } 
}