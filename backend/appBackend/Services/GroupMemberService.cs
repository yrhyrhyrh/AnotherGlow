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

        public async Task<GroupMember?> AddGroupMemberAsync(Guid group_id, Guid user_id, bool is_admin)
        {
            Console.WriteLine("new group member");
            Console.WriteLine(group_id);
            // Check if group already exists
            if (!await _context.Groups.AnyAsync(g => g.GroupId == group_id)){
              return null; // Group doesn't exist
            }else if (await _context.GroupMembers.AnyAsync(g=> g.GroupId == group_id && g.UserId == user_id)){
              return null; // Group member already added
            }

            var newGroupMember = new GroupMember
            {
                GroupId = group_id,
                UserId = user_id,
                IsAdmin = is_admin,
                CreatedAt = DateTime.UtcNow,
            };

            _context.GroupMembers.Add(newGroupMember);
            await _context.SaveChangesAsync();
            return newGroupMember;
        }
    }
}
