namespace appBackend.Dtos.GlobalPostWall
{
    public class CommentDTO
    {
        public Guid CommentId { get; set; }
        public Guid UserId { get; set; } // Comment author ID
        public string AuthorUsername { get; set; } = string.Empty;
        public string AuthorFullName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCommentRequestDTO
    {
        public string Content { get; set; } = string.Empty;
    }
}
