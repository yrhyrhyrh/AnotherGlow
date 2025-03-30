using System;
using System.Collections.Generic;

public class Poll
{
    public int Id { get; set; }
    public string Question { get; set; }
    public List<string> Options { get; set; } = new List<string>();
    public Dictionary<int, int> Votes { get; set; } = new Dictionary<int, int>(); // Option ID -> Vote Count
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsGlobal { get; set; } // Whether poll is visible globally
}