using appBackend.Repositories;
using Microsoft.EntityFrameworkCore;
using MyAppBackend.Repositories;
using MyAppBackend.Services;
using Amazon.S3;
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
builder.Services.AddAWSService<IAmazonS3>(); // Register IAmazonS3

builder.Services.AddScoped<IPollService, PollService>();
builder.Services.AddScoped<IPollRepository, PollRepository>(); 

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

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("OnAuthenticationFailed: " + context.Exception.Message); // Log failure reason
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("OnTokenValidated: " + context.SecurityToken); // Log success
                // You could inspect context.Principal.Claims here
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine("OnChallenge: " + context.ErrorDescription); // Log challenge details
                return Task.CompletedTask;
            },
            OnMessageReceived = context => {
                Console.WriteLine("OnMessageReceived: Token received from header: " + (context.Request.Headers.ContainsKey("Authorization") ? "Yes" : "No"));
                return Task.CompletedTask;
            }
        };

    });

// Enable CORS for Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200",
                "http://frontend-lb-755996743.ap-southeast-1.elb.amazonaws.com"
            )
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
builder.Services.AddControllers()
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        x.JsonSerializerOptions.PropertyNamingPolicy = null; // optional: keeps PascalCase if you prefer
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
builder.Services.AddScoped<UserSettingsService>(); // Register User Settings Service

builder.Services.AddSwaggerGen(); // Swagger generation setup

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<SocialMediaDbContext>(options =>
    options.UseNpgsql(connectionString)
    .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
    .EnableSensitiveDataLogging()
);


builder.WebHost.ConfigureKestrel(serverOptions =>
{
    if (builder.Environment.IsDevelopment())
    {
        // Bind to localhost in development for local testing
        serverOptions.ListenLocalhost(5181);  // Use localhost:5000
    }
    else
    {
        // Bind to any IP on AWS or production environments
        serverOptions.ListenAnyIP(5000);  // Use 0.0.0.0:5000 for external access
    }
});

var app = builder.Build();

// Enable Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseRouting(); // Often needed before Auth if using endpoints
app.UseAuthentication(); // <-- Before Authorization
app.UseAuthorization();  // <-- After Authentication
app.MapControllers();

app.Run();
