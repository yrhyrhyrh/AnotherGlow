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
            Console.WriteLine("user does not exist yet");

            var newUser = new User
            {
                Username = username,
                Email = email,
                PasswordHash = _passwordHasher.HashPassword(null, password), // Hash password
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            
            Console.WriteLine("user object created");

            _context.Users.Add(newUser);
            Console.WriteLine("user added");
            await _context.SaveChangesAsync();
            Console.WriteLine("user added frfr");
            return newUser;
        }
    }
}
