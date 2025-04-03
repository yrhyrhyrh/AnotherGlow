namespace appBackend.Dtos.GlobalPostWall
{
    public class AttachmentDTO
    {
        public Guid AttachmentId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty; // Or FileUrl
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }
}
