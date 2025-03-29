using Microsoft.EntityFrameworkCore;
using appBackend.Models;

namespace appBackend.Repositories
{
    public interface IUserRepository
    {
        Task<User?> CreateUserAsync(User user); // Create a new user
        Task<User?> GetUserByCredentialsAsync(string username); // Fetch user by username
    }

    public class UserRepository : IUserRepository
    {
        private readonly SocialMediaDbContext _context;

        public UserRepository(SocialMediaDbContext context)
        {
            _context = context;
        }

        public async Task<User?> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> GetUserByCredentialsAsync(string username)
        {
            // Fetch user by username and password
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}
