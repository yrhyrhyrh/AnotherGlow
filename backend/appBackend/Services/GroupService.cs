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

        public async Task<Group?> CreateGroupAsync(string groupname)
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
            return newGroup;
        }
    }
}
