using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace appBackend.Models{
  public class GroupMember
    {
        public Guid GroupMemberId { get; set; } = Guid.NewGuid(); // Primary Key
        
        public Guid UserId { get; set; } // Foreign key to User
        public Guid GroupId { get; set; } // Foreign key to Group
        
        public bool IsAdmin { get; set; } = false; // Whether the user is an admin of the group

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("GroupId")]
        public virtual Group Group { get; set; } = null!;
    }
}
