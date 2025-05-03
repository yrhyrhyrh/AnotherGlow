using appBackend.Models;
using appBackend.Repositories;
using appBackend.Dtos.Group;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace appBackend.Services
{

    public interface IGroupRepository
    {
        Task<Group> GetGroupByIdAsync(Guid groupId);
        Task<List<Group>> GetGroupsByUserIdAsync(Guid userId, bool isAdmin);
        Task<GroupDto?> GetGroupAsync(Guid group_id, Guid currentUserId); // Updated to include currentUserId
        Task<Guid> CreateGroupAsync(Group group); // Get group by creds
        Task<List<UserDto>> SearchUsersNotInGroupAsync(Guid group_id, string keyword);
        Task<bool> UpdateGroupAsync(Group group);
        Task<bool> DeleteGroupAsync(Guid groupId);
    }

    public class GroupRepository : IGroupRepository
    {
        private readonly SocialMediaDbContext _context;

        public GroupRepository(SocialMediaDbContext context)
        {
            _context = context;
        }

        public async Task<Group> GetGroupByIdAsync(Guid groupId)
        {
            return await _context.Groups.FindAsync(groupId);
        }

        public async Task<List<Group>> GetGroupsByUserIdAsync(Guid userId, bool isAdmin)
        {
            Console.WriteLine("Getting groups for user: " + userId + " | isAdmin: " + isAdmin);

            var groups = await _context.GroupMembers
                .Where(gm => gm.UserId == userId && gm.IsAdmin == isAdmin)
                .Select(gm => gm.Group)
                .ToListAsync();

            return groups;
        }


        public async Task<GroupDto?> GetGroupAsync(Guid group_id, Guid currentUserId)
        {
            Console.WriteLine("Getting group details");
            Console.WriteLine(group_id);

            var group = await _context.Groups
                .Where(g => g.GroupId == group_id)
                .Select(g => new GroupDto
                {
                    GroupId = g.GroupId,
                    Name = g.Name,
                    Description = g.Description,
                    GroupPictureUrl = g.GroupPictureUrl,
                    IsAdmin = g.Members.Any(m => m.UserId == currentUserId && m.IsAdmin),
                    Members = g.Members.Select(m => new GroupMemberDto
                    {
                        GroupMemberId = m.GroupMemberId,
                        IsAdmin = m.IsAdmin,
                        User = new UserDto
                        {
                            UserId = m.User.UserId,
                            Username = m.User.Username,
                            ProfilePictureUrl = m.User.ProfilePictureUrl
                        }
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return group;
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
                GroupId = Guid.NewGuid(),
                Name = group.Name,
                Description = group.Description,
                GroupPictureUrl = group.GroupPictureUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _context.Groups.Add(newGroup);
            await _context.SaveChangesAsync();
            return newGroup.GroupId;
        }

        public async Task<bool> UpdateGroupAsync(Group group)
        {
            _context.Groups.Update(group);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<UserDto>> SearchUsersNotInGroupAsync(Guid group_id, string keyword)
        {
            Console.WriteLine($"Getting users to add in: {group_id} with keyword: {keyword}");

            var query = _context.Users
                .Where(u => !_context.GroupMembers
                    .Where(gm => gm.GroupId == group_id)
                    .Select(gm => gm.UserId)
                    .Contains(u.UserId));

            // Only filter by keyword if it's not null or whitespace
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(u => u.Username.Contains(keyword));
            }

            var usersNotInGroup = await query
                .OrderBy(u => u.Username)
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    ProfilePictureUrl = u.ProfilePictureUrl
                })
                .Take(20)
                .ToListAsync();

            return usersNotInGroup;
        }

        public async Task<bool> DeleteGroupAsync(Guid groupId)
        {
            var group = await _context.Groups
                .Include(g => g.Members)
                .Include(g => g.Posts)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);

            if (group == null)
            {
                return false;
            }

            // Remove all related entities
            _context.GroupMembers.RemoveRange(group.Members);
            _context.Posts.RemoveRange(group.Posts);
            _context.Groups.Remove(group);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
