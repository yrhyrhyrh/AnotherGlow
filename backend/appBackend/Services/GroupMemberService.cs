using appBackend.Models;
using appBackend.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace appBackend.Services
{
    public class GroupMemberService
    {
        private readonly SocialMediaDbContext _context;

        public GroupMemberService(SocialMediaDbContext context)
        {
            _context = context;
        }

        public async Task<List<GroupMember>?> AddGroupMembersAsync(Guid group_id, List<Guid> user_ids, bool is_admin)
        {
            Console.WriteLine("Adding new group members");
            Console.WriteLine(group_id);

            // Check if group already exists
            var groupExists = await _context.Groups.AnyAsync(g => g.GroupId == group_id);
            if (!groupExists)
            {
                return null; // Group doesn't exist
            }

            // Create a list to hold new GroupMembers
            var newGroupMembers = new List<GroupMember>();

            foreach (var user_id in user_ids)
            {
                // Check if the user is already a member of the group
                var memberExists = await _context.GroupMembers
                    .AnyAsync(gm => gm.GroupId == group_id && gm.UserId == user_id);

                if (!memberExists)
                {
                    // Create new group member
                    var newGroupMember = new GroupMember
                    {
                        GroupId = group_id,
                        UserId = user_id,
                        IsAdmin = is_admin,
                        CreatedAt = DateTime.UtcNow,
                    };

                    newGroupMembers.Add(newGroupMember); // Add to the list
                }
            }

            // Add all new members to the context
            if (newGroupMembers.Any())
            {
                _context.GroupMembers.AddRange(newGroupMembers);
                await _context.SaveChangesAsync();
            }

            return newGroupMembers;
        }
    }
}
