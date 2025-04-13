using System;
using System.Collections.Generic; // Required for List

namespace appBackend.DTOs
{
    public class VoteRequest
    {
        public Guid UserId { get; set; }          // Ensure this type matches your User ID type
        public int? OptionIndex { get; set; }      // Nullable for multi-select case
        public List<int>? OptionIndices { get; set; } // List for multi-select
        public bool Retract { get; set; } = false;  // Default to false
    }
}