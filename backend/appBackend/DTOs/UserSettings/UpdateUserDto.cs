using System.ComponentModel.DataAnnotations;

namespace appBackend.DTOs.UserSettings
{
    public class UpdateUserDto
    {
        // Include only the fields the user should be able to update via this form.
        // Validation attributes ensure basic data integrity.

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required.")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters long.")]
        public string Username { get; set; } = string.Empty;

        // Optional fields
        public string? FullName { get; set; }

        [MaxLength(500, ErrorMessage = "Bio cannot exceed 500 characters.")]
        public string? Bio { get; set; }

        public string? JobRole { get; set; }

        // Note: We DO NOT include UserId (it comes from the route)
        // Note: We DO NOT include Password/PasswordHash (handled separately)
        // Note: We DO NOT include ProfilePictureUrl (handle file uploads separately if needed)
    }

}
