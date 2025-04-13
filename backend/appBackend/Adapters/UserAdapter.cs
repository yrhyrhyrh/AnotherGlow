using Microsoft.AspNetCore.Identity;
using appBackend.Models;
using appBackend.Dtos.Auth;

namespace appBackend.Adapters
{
    public interface IUserAdapter
    {
        User ToUser(RegisterRequest dto);
    }

    public class UserAdapter : IUserAdapter
    {
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserAdapter(IPasswordHasher<User> passwordHasher)
        {
            _passwordHasher = passwordHasher;
        }

        public User ToUser(RegisterRequest dto)
        {
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Hash the password and assign it
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            return user;
        }
    }
}
