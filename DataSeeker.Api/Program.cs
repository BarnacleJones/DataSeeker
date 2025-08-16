using DataAccess;
using DataAccess.Service;
using Microsoft.EntityFrameworkCore;
using Worker;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DataSeekerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUploadService, UploadService>();
builder.Services.Configure<LogIngestOptions>(builder.Configuration.GetSection("LogIngest"));

builder.Services.AddHostedService<LogIngester>();

var app = builder.Build();


// Apply any pending EF Core migrations at startup
// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<DataSeekerDbContext>();
//     db.Database.Migrate();
// }
using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<DataSeekerDbContext>();

var retries = 10;
while (retries > 0)
{
    try
    {
        db.Database.Migrate();
        break; // Success
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database not ready yet: {ex.Message}");
        retries--;
        Thread.Sleep(3000); // wait 3 seconds
    }
}

if (retries == 0)
{
    Console.WriteLine("Could not connect to the database after multiple attempts.");
    throw new Exception("Database not available");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}