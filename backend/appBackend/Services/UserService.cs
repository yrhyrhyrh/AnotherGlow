using appBackend.Models;
using appBackend.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace appBackend.Services
{
    public class UserService
    {
        private readonly SocialMediaDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public UserService(SocialMediaDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<User?> RegisterUserAsync(string username, string email, string password)
        {
            Console.WriteLine("registering!");
            Console.WriteLine(username+email+password);
            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Username == username || u.Email == email))
            {
                return null; // User already exists
            }

            var newUser = new User
            {
                Username = username,
                Email = email,
                PasswordHash = _passwordHasher.HashPassword(new User(), password), // Hash password
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return newUser;
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            Console.WriteLine("Validating user...");

            var user = await _context.Users
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync(); // Fetch the full user object

            if (user == null) return false; // User not found

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success;
        }
    }
}
