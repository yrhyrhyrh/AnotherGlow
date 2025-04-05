using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace appBackend.Models
{
    public class Post
    {
        public Guid PostId { get; set; } // Corresponds to post_id (UUID)

        public Guid UserId { get; set; } // Foreign key to User

        [Required]
        public string Content { get; set; } = string.Empty; // Corresponds to content (TEXT)

        public DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow; // Corresponds to created_at

        public DateTimeOffset UpdatedAt { get; set; } = DateTime.UtcNow; // Corresponds to updated_at

        // --- Navigation Properties ---

        // The user who authored this post
        [ForeignKey("UserId")]
        public virtual User Author { get; set; } = null!; // Required relationship

        // Likes received by this post (Join entity)
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}
