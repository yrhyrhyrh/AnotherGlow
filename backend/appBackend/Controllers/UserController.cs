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
using appBackend.DTOs.UserSettings;
using Microsoft.AspNetCore.Authorization;
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

    [HttpPut("update/{userId}")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserDto updateUserDto)
    {
        // 1. Authorization Check: Ensure the logged-in user is the one being updated
        if (!User.Identity!.IsAuthenticated)
        {
            // Optional: Add role check if Admins can update any user
            // if (!User.IsInRole("Admin")) // Example admin check
            Console.WriteLine($"UpdateUser Forbidden: Route UserId {userId} is not authenticated");
            return Forbid(); // 403 Forbidden - Authenticated but not authorized for this resource
            // }
        }

        // 2. Model Validation (ASP.NET Core does this automatically based on DTO attributes)
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState); // 400 Bad Request with validation errors
        }

        // 3. Prepare User object for the service layer
        // Map data from the DTO and the route parameter (userId) to a User object
        // Important: Only map fields that should be updated.
        var userToUpdate = new User
        {
            UserId = userId, // Get UserId from the route parameter
            Email = updateUserDto.Email,
            Username = updateUserDto.Username,
            FullName = updateUserDto.FullName,
            Bio = updateUserDto.Bio,
            JobRole = updateUserDto.JobRole
            // DO NOT set PasswordHash, CreatedAt, etc. here. The repository handles UpdatedAt.
        };

        // 4. Call the Service Layer
        var updatedUser = await _userSettingsService.UpdateUserAsync(userToUpdate);

        // 5. Handle the Result
        if (updatedUser == null)
        {
            // This could mean the user wasn't found by the repository during the update attempt,
            // or another error occurred (check repository logs).
            return NotFound(new { message = $"User with ID {userId} not found or update failed." });
        }

        // 6. Return Success Response (optionally return the updated data, excluding sensitive info)
        // Map the *result* back to a safe DTO to return to the client
        var resultDto = new UserSettingsDto
        {
            UserId = updatedUser.UserId,
            Email = updatedUser.Email,
            Username = updatedUser.Username,
            FullName = updatedUser.FullName,
            Bio = updatedUser.Bio,
            JobRole = updatedUser.JobRole,
            ProfilePictureUrl = updatedUser.ProfilePictureUrl // Include if you have this field
        };

        return Ok(resultDto); // 200 OK with the updated user data (without hash)
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
