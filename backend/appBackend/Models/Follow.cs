using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace appBackend.Models
{
    // Represents the 'follows' table (Many-to-Many join table for Users)
    public class Follow
    {
        public Guid FollowId { get; set; } // Corresponds to follow_id (UUID)
        public Guid FollowerUserId { get; set; } // Foreign key to the user doing the following
        public Guid FollowingUserId { get; set; } // Foreign key to the user being followed

        public DateTimeOffset CreatedAt { get; set; } // Corresponds to created_at

        // --- Navigation Properties ---

        [ForeignKey("FollowerUserId")]
        public virtual User Follower { get; set; } = null!; // Required relationship

        [ForeignKey("FollowingUserId")]
        public virtual User Following { get; set; } = null!; // Required relationship
    }
}
