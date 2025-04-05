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

    public AuthController(IConfiguration configuration, AuthService authService)
    {
        _key = configuration["JwtSettings:Secret"] ?? throw new ArgumentNullException("JWT Secret is missing!");
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var userId = await _authService.ValidateUserAsync(request.Username, request.Password);
        if (userId==null)
            return Unauthorized();

        var token = _authService.GenerateJwtToken(userId.Value.ToString());
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

        var userId = await _authService.RegisterUserAsync(request);
        
        if (userId == null)
        {
            return Conflict(new { message = "Username or Email already exists." });
        }

        // Generate JWT token for the registered user
        var token = _authService.GenerateJwtToken(userId.Value.ToString());

        return Ok(new { token, message = "User registered successfully!" });
    }

}
