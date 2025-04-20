using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using appBackend.Services;
using appBackend.Dtos.Group;
using appBackend.Dtos.UserSettings;
using System.ComponentModel.DataAnnotations;
namespace appBackend.Models;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly string _key;
    private readonly UserSettingsService _userSettingsService;

    public UserController(IConfiguration configuration, UserSettingsService userSettings)
    {
        _userSettingsService = userSettings;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<UserSettingsDto>> GetUser(Guid userId) // Return a DTO, not User model
    {
        // ... (Add authorization checks) ...
        var user = await _userSettingsService.GetUserAsync(userId);
        if (user == null) return NotFound();

        // Map to a DTO that excludes PasswordHash
        var userDto = new UserSettingsDto
        {
            UserId = user.UserId,
            Email = user.Email,
            Username = user.Username,
            FullName = user.FullName,
            Bio = user.Bio,
            JobRole = user.JobRole
            // Add other safe fields as needed
        };
        return Ok(userDto);
    }

    public class ChangePasswordRequest
    {
        public Guid UserId { get; set; } // Or get from route/claims
        [Required] public string OldPassword { get; set; } = "";
        [Required] public string NewPassword { get; set; } = "";
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var success = await _userSettingsService.ChangePasswordAsync(request.UserId, request.OldPassword, request.NewPassword);

        if (success)
        {
            return Ok(new { message = "Password changed successfully." });
        }
        else
        {
            // Provide a generic error or specific based on service logic (e.g., user not found vs incorrect password)
            return BadRequest(new { message = "Failed to change password. Check current password or user." });
        }
    }
}
