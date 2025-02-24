using MyAppBackend.Repositories;
using MyAppBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Register services and repositories
builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();
builder.Services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();

// Add services to the container
builder.Services.AddControllers(); // Make sure controllers are registered here
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Swagger generation setup

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
