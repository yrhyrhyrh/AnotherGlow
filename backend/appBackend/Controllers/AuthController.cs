using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using appBackend.Services;
namespace appBackend.Models;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly string _key;
    private readonly UserService _userService;

    public AuthController(IConfiguration configuration, UserService userService)
    {
        _key = configuration["JwtSettings:Secret"] ?? throw new ArgumentNullException("JWT Secret is missing!");
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Validate the user credentials (e.g., via UserService)
        var user = await _userService.FindUserByUsernameAsync(request.Username);
        if (user != null && await _userService.ValidateUserAsync(request.Username, request.Password))
        {
            // Generate a JWT including a claim for the user’s unique ID
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_key);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.Name, request.Username),       // The username
                new Claim("user_id", user.UserId.ToString())        // Custom claim for UserId
            }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(new { token = tokenHandler.WriteToken(token), userId = user.UserId });
        }

        return Unauthorized("Invalid username or password.");
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "All fields are required." });
        }

        // Attempt to register the new user
        var user = await _userService.RegisterUserAsync(request.Username, request.Email, request.Password);

        if (user == null)
        {
            return Conflict(new { message = "Username or Email already exists." });
        }

        // Generate the token with the user ID claim
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_key);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.Name, request.Username),
            new Claim("user_id", user.UserId.ToString())  // Add the user ID as a claim
        }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return Ok(new { token = tokenHandler.WriteToken(token), message = "User registered successfully!" });
    }
}
