namespace PSP;

using Npgsql;
class SignUpRequest
{
    public string? Username { get; }
    public string? Password { get; }

    public SignUpRequest(string? username, string? password)
    {
        Username = username;
        Password = password;
    }
}
class HttpResponse
{
    public int StatusCode { get; }
    public object Body { get; }

    public HttpResponse(object body, int statusCode)
    {
        Body = body;
        StatusCode = statusCode;
    }
}

class Recipient
{
    public int Id { get; }
    public string Username { get; }
    public string? Password { get; }

    public Recipient(int id, string username, string password)
    {
        Id = id;
        Username = username;
        Password = password;
    }

}

public class Program
{
    private static void Main()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.MapPost("/signup", (SignUpRequest request) =>
        {
            var controller = new SignUpController();
            HttpResponse response = controller.Handle(request);
            return Results.Json(response.Body, statusCode: response.StatusCode);
        });

        app.Lifetime.ApplicationStopping.Register(() =>
        {
            Console.WriteLine("Roda no clear pool");
            NpgsqlConnection.ClearAllPools();
        });
        app.Run();
    }
}