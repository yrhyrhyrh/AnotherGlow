using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using appBackend.Services;
using appBackend.Dtos;

namespace appBackend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly string _key;
    private readonly AuthService _authService;

    public AuthController(IConfiguration configuration, AuthService userService)
    {
        _key = configuration["JwtSettings:Secret"] ?? throw new ArgumentNullException("JWT Secret is missing!");
        _authService = userService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var validated = await _authService.ValidateUserAsync(request.Username, request.Password);
        if (!validated)
            return Unauthorized();

        var token = _authService.GenerateJwtToken(request.Username);
        return Ok(new { token });
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

        var user = await _authService.RegisterUserAsync(request);
        
        if (user == null)
        {
            return Conflict(new { message = "Username or Email already exists." });
        }

        // Generate JWT token for the registered user
        var token = _authService.GenerateJwtToken(user.Username);

        return Ok(new { token, message = "User registered successfully!" });
    }

}
