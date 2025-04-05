using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace appBackend.Models
{
    public class User
    {
        public Guid UserId { get; set; } // Corresponds to user_id (UUID)

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty; // Corresponds to username

        [Required]
        [MaxLength(255)]
        [EmailAddress] // Basic validation
        public string Email { get; set; } = string.Empty; // Corresponds to email

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty; // Corresponds to password_hash

        [MaxLength(100)]
        public string? FullName { get; set; } // Corresponds to full_name (nullable)

        public string? Bio { get; set; } // Corresponds to bio (nullable, TEXT maps well to string)

        [MaxLength(255)]
        public string? ProfilePictureUrl { get; set; } // Corresponds to profile_picture_url (nullable)

        public DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow; // Corresponds to created_at (TIMESTAMP WITH TIME ZONE)

        public DateTimeOffset UpdatedAt { get; set; } = DateTime.UtcNow; // Corresponds to updated_at (TIMESTAMP WITH TIME ZONE)

        // --- Navigation Properties ---

        // Posts created by this user
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

        // Users that this user is following (Join entity)
        public virtual ICollection<Follow> Following { get; set; } = new List<Follow>();

        // Users that are following this user (Join entity)
        public virtual ICollection<Follow> Followers { get; set; } = new List<Follow>();

        // Likes given by this user (Join entity)
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

        // Likes given by this user (Join entity)
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        // Group memberships for this user (Join entity)
        public virtual ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();
    }
}
