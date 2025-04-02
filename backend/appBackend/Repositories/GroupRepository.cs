using appBackend.Models;
using appBackend.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace appBackend.Services
{

    public interface IGroupRepository
    {
        Task<Group?> GetGroupAsync(Guid group_id); // Fetch group
        Task<Guid> CreateGroupAsync(Group group); // Get group by creds
    }

    public class GroupRepository : IGroupRepository
    {
        private readonly SocialMediaDbContext _context;

        public GroupRepository(SocialMediaDbContext context)
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

        public async Task<Guid> CreateGroupAsync(Group group)
        {
            Console.WriteLine("new group name");
            Console.WriteLine(group.Name);
            // Check if group already exists
            if (await _context.Groups.AnyAsync(g => g.Name == group.Name))
            {
                return Guid.Empty; // Group already exists
            }

            var newGroup = new Group
            {
                Name = group.Name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _context.Groups.Add(newGroup);
            await _context.SaveChangesAsync();
            return newGroup.GroupId;
        }
    }
}
