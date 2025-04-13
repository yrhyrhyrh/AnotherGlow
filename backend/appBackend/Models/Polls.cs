using System;
using System.Collections.Generic;

namespace appBackend.Models
{
    public class Poll
    {
        public Guid PollId { get; set; }
        public Guid UserId { get; set; } // Creator's User ID

        public string Question { get; set; } = string.Empty; // Initialize strings
        public List<string> Options { get; set; } = new List<string>();
        // Stores aggregate counts, not individual votes
        public Dictionary<int, int> Votes { get; set; } = new Dictionary<int, int>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsGlobal { get; set; }
        public bool AllowMultipleSelections { get; set; } // <-- ADDED
    }
}