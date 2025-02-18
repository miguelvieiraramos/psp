namespace PSP;

using Dapper;
using Npgsql;
using BCrypt.Net;
class SignUpController
{
    public HttpResponse Handle(SignUpRequest body)
    {
        if (body.Username == null)
        {
            return new HttpResponse(new {  message = "The username field is required." },400);
        }

        if (body.Password == null)
        {
            return new HttpResponse(new {  message = "The password field is required." },400);
        }
        var hashedPassword =  BCrypt.HashPassword(body.Password, 11);
        var connectionString = $"Host={Environment.GetEnvironmentVariable("POSTGRES_HOST")};Username={Environment.GetEnvironmentVariable("POSTGRES_USERNAME")};Password={Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")};Database={Environment.GetEnvironmentVariable("POSTGRES_DATABASE")}";
        using (var connection = new NpgsqlConnection(connectionString))
        {
            try
            {
                var sql = "INSERT INTO \"Recipients\" (username, password) VALUES (@Username, @Password) RETURNING id;";
                var id = connection.QuerySingle<int>(sql, new { body.Username, Password = hashedPassword });
                return new HttpResponse(new { id = id, username = body.Username }, 201);
            }
            catch (Exception e)
            {
                if (e.Data["ConstraintName"]!.Equals("Recipients_username_key"))
                {
                    return new HttpResponse(new {  message = "This user is already in use." }, 409);
                }
                throw;
            }
        }
    }
}
