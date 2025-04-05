using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace appBackend.Models
{
    public class Attachment
    {
        public Guid AttachmentId { get; set; }
        public Guid PostId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty; // Or FileUrl if storing in cloud
        public string ContentType { get; set; } = string.Empty; // Mime type
        public long FileSize { get; set; } // Size in bytes
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property

        [ForeignKey("PostId")]
        public virtual Post Post { get; set; } = null!; // Required relationship
    }
}
