using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace appBackend.Models
{
    public class Comment
    {
        public Guid CommentId { get; set; }
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public string Content { get; set; } = string.Empty; // Comment text
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // --- Navigation Properties ---

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!; // Required relationship

        [ForeignKey("PostId")]
        public virtual Post Post { get; set; } = null!; // Required relationship

    }
}
