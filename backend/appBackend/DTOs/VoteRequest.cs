namespace appBackend.DTOs
{
    public class VoteRequest
    {
        public Guid UserId { get; set; }
        public int OptionIndex { get; set; }
        public bool Retract { get; set; } = false; // Optional flag to support vote retraction
    }
}
