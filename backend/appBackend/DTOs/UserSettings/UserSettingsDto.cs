using System.ComponentModel.DataAnnotations;

namespace appBackend.Dtos.UserSettings;

public class UserSettingsDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty; 
    public string PasswordHash { get; set; } = string.Empty; 
    public string? FullName { get; set; } 
    public string? Bio { get; set; } 
    public string? ProfilePictureUrl { get; set; }
    public string? JobRole { get; set; } 
}
