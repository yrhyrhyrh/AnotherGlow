using System;

namespace appBackend.Models 
{
    public class Vote
    {
        public Guid VoteId { get; set; }
        public Guid UserId { get; set; }
        public Guid PollId { get; set; }
        public int OptionIndex { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
