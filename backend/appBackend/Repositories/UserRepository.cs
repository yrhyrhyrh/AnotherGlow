using Microsoft.EntityFrameworkCore;
using appBackend.Models;
using System.Security.Cryptography.Xml;

namespace appBackend.Repositories
{
    public interface IUserRepository
    {
        Task<User?> CreateUserAsync(User user); // Create a new user
        Task<User?> GetUserByCredentialsAsync(string username); // Fetch user by username
        Task<User?> UpdateUserAsync(User user);
        Task<User> GetUserByUserId(Guid userId);
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

        public async Task<User> GetUserByUserId(Guid userId)
        {
            Console.WriteLine("Getting user details for: " + userId);

            var user = await _context.Users.FirstAsync(x => x.UserId == userId);

            return user!;
        }

        public async Task<User?> UpdateUserAsync(User updatedUser)
        {
            try
            {
                // Fetch the existing entity from the database *to be tracked by EF Core*
                var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.UserId == updatedUser.UserId);
                if (existingUser != null)
                {
                    // Update only the allowed properties from the input 'updatedUser' object
                    existingUser.Email = updatedUser.Email;
                    existingUser.Username = updatedUser.Username;
                    existingUser.FullName = updatedUser.FullName;
                    existingUser.Bio = updatedUser.Bio;
                    existingUser.JobRole = updatedUser.JobRole;
                    // DO NOT update UserId, CreatedAt, or PasswordHash here!
                    existingUser.UpdatedAt = DateTime.UtcNow; // Use UtcNow

                    // _context.Users.Update(existingUser); // Usually not needed if fetched and modified
                    await _context.SaveChangesAsync();
                    return existingUser; // Return the updated, tracked entity
                }
                return null; // Or throw not found exception
            }
            catch (Exception ex) // Catch more specific exceptions if possible
            {
                // log ex
                Console.WriteLine($"Error updating user {updatedUser.UserId}: {ex.Message}"); // Basic logging
                return null;
            }
        }



    }
}
