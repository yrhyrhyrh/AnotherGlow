using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace appBackend.Models
{
    // Represents the 'likes' table (Many-to-Many join table for Users and Posts)
    public class Like
    {
        public Guid LikeId { get; set; } // Corresponds to like_id (UUID)
        public Guid UserId { get; set; } // Foreign key to the user who liked
        public Guid PostId { get; set; } // Foreign key to the post that was liked

        public DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow; // Corresponds to created_at

        // --- Navigation Properties ---

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!; // Required relationship

        [ForeignKey("PostId")]
        public virtual Post Post { get; set; } = null!; // Required relationship
    }
}
