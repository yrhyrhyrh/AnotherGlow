using System;
using System.Collections.Generic;

namespace appBackend.Models 
{ 
    public class Poll
    {
        public Guid PollId { get; set; }
        public Guid UserId { get; set; }

        public string Question { get; set; }
        public List<string> Options { get; set; } = new List<string>();
        public Dictionary<int, int> Votes { get; set; } = new Dictionary<int, int>(); // Option ID -> Vote Count
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsGlobal { get; set; } // Whether poll is visible globally
    }
}
