using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace appBackend.Models
{
		public class Group
		{
				public Guid GroupId { get; set; } = Guid.NewGuid(); // Corresponds to group_id (UUID)
				
				[Required]
				[MaxLength(100)]
				public string Name { get; set; } = string.Empty; // Corresponds to name
				
				// Navigation property for members of the group
				public virtual ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
		}
}
