﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace appBackend.Models
{
    public class Post
    {
        public Guid PostId { get; set; } // Corresponds to post_id (UUID)

        public Guid UserId { get; set; } // Foreign key to User

        [Required]
        public string Content { get; set; } = string.Empty; // Corresponds to content (TEXT)

        public DateTime CreatedAt { get; set; } // Corresponds to created_at

        public DateTime UpdatedAt { get; set; } // Corresponds to updated_at

        [ForeignKey("GroupId")]
        public Guid GroupId { get; set; }
        public Group Group { get; set; } = null!; // Navigation property

        // --- Navigation Properties ---

        // The user who authored this post
        [ForeignKey("UserId")]
        public virtual User Author { get; set; } = null!; // Required relationship

        // Likes received by this post (Join entity)
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

        // Comments received by this post (Join entity)
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        // Attachments on the post (Join entity)
        public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
        public virtual Poll? Poll { get; set; }
    }
}
