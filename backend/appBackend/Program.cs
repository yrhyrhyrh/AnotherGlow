using appBackend.Repositories;
using Microsoft.EntityFrameworkCore;
using MyAppBackend.Repositories;
using MyAppBackend.Services;
using System.Data.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using appBackend.Services;
using appBackend.Adapters;
using appBackend.Models;
using Microsoft.AspNetCore.Identity;
using System.Text;
using appBackend.Interfaces.GlobalPostWall;
using appBackend.Services.GlobalPostWall;
using appBackend.Repositories.GlobalPostWall;

var builder = WebApplication.CreateBuilder(args);

// Register services and repositories
builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();
builder.Services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = jwtSettings["Secret"] ?? throw new ArgumentNullException("JWT Secret is missing!");

// Register JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        policy => policy
            .WithOrigins("http://localhost:4200") // Allow Angular frontend origin
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// Register Services
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IPostSocialActionsService, PostSocialActionsService>();

// Register Repositories
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IAttachmentRepository, AttachmentRepository>();
builder.Services.AddScoped<ILikeRepository, LikeRepository>();

builder.Services.AddAuthorization();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    options.JsonSerializerOptions.WriteIndented = true;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserAdapter, UserAdapter>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IGroupAdapter, GroupAdapter>();
builder.Services.AddScoped<IGroupMemberRepository, GroupMemberRepository>();
builder.Services.AddScoped<IGroupMemberAdapter, GroupMemberAdapter>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
// builder.Services.AddScoped<UserService>(); // Register UserService
builder.Services.AddScoped<GroupService>(); // Register Group service
builder.Services.AddScoped<GroupMemberService>(); // Register Group service

builder.Services.AddSwaggerGen(); // Swagger generation setup

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection"); // Get from appsettings.json

builder.Services.AddDbContext<SocialMediaDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptionsAction: sqlOptions =>
    {
        // Optional: Configure Npgsql options if needed
        // sqlOptions.EnableRetryOnFailure();
    })
    // Optional: Add logging in development
    .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
    .EnableSensitiveDataLogging() // Only in development!
);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowLocalhost");
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
