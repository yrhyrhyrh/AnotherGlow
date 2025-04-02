namespace appBackend.Dtos.GlobalPostWall
{
    public class LikeDTO
    {
        public Guid LikeId { get; set; }
        public Guid UserId { get; set; }
        public Guid PostId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
