using appBackend.Models;
using appBackend.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace appBackend.Services
{
    public class GroupService
    {
        private readonly SocialMediaDbContext _context;

        public GroupService(SocialMediaDbContext context)
        {
            _context = context;
        }

        public async Task<Group?> GetGroupAsync(Guid group_id)
        {
            Console.WriteLine("Getting group details");
            Console.WriteLine(group_id);

            // Check if the group exists by its group_id
            var group = await _context.Groups
                .FirstOrDefaultAsync(g => g.GroupId == group_id);

            // If the group is not found, return null
            if (group == null)
            {
                return null; // Group doesn't exist
            }

            return group; // Return the group details
        }

        public async Task<Group?> CreateGroupAsync(string groupname, Guid user_id)
        {
            Console.WriteLine("new group name");
            Console.WriteLine(groupname);
            // Check if group already exists
            if (await _context.Groups.AnyAsync(g => g.Name == groupname))
            {
                return null; // Group already exists
            }

            var newGroup = new Group
            {
                Name = groupname,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _context.Groups.Add(newGroup);
            await _context.SaveChangesAsync();

            // Add the user as the first member with admin rights
            var newGroupMember = new GroupMember
            {
                GroupId = newGroup.GroupId,
                UserId = user_id,
                IsAdmin = true,  // Set the user as admin
                CreatedAt = DateTime.UtcNow,
            };

            // Add the new group member to the context
            _context.GroupMembers.Add(newGroupMember);

            // Save the new member to the database
            await _context.SaveChangesAsync();
            return newGroup;
        }
    }
}
