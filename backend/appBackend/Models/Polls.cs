using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace appBackend.Models
{
    public class Poll
    {
        public Guid PollId { get; set; }

        public string Question { get; set; } = string.Empty; // Initialize strings
        public List<string> Options { get; set; } = new List<string>();
        // Stores aggregate counts, not individual votes
        public Dictionary<int, int> Votes { get; set; } = new Dictionary<int, int>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool AllowMultipleSelections { get; set; } // <-- ADDED

        [ForeignKey("PostId")]
        public Guid PostId { get; set; }
        public Post? Post { get; set; } = null!; // Navigation property
    }
}