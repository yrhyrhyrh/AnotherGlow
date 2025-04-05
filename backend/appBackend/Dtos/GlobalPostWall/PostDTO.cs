namespace appBackend.Dtos.GlobalPostWall
{
    public class PostDTO
    {
        public Guid PostId { get; set; }
        public Guid UserId { get; set; } // Author ID
        public string AuthorUsername { get; set; } = string.Empty;
        public string AuthorFullName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public List<AttachmentDTO> Attachments { get; set; } = new List<AttachmentDTO>();
    }

    public class CreatePostRequestDTO
    {
        public string Content { get; set; } = string.Empty;
        public List<IFormFile> Attachments { get; set; } = new List<IFormFile>(); // For file uploads
    }

    public class UpdatePostRequestDTO
    {
        public string? Content { get; set; } // Allow updating content
    }
}
