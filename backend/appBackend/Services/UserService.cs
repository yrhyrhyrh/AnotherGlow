using appBackend.Models;
using appBackend.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace appBackend.Services
{
    public class UserService
    {
        private readonly SocialMediaDbContext _context;

        public UserService(SocialMediaDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserAsync(Guid user_id)
        {
					// Check if the user exists by its user_id
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == user_id);

            // If the group is not found, return null
            if (user == null)
            {
                return null; // User doesn't exist
            }

            return user; // Return the user details
        }

        public async Task<List<User>> GetAllNonGroupUsersAsync(Guid group_id)
        {
            var usersNotInGroup = await _context.Users
                .Where(u => !_context.GroupMembers
                    .Any(gm => gm.GroupId == group_id && gm.UserId == u.UserId))
                .ToListAsync();

            return usersNotInGroup;
        }
    }
}
