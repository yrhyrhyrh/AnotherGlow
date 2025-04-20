using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using appBackend.Adapters;
using appBackend.Dtos.Auth;
using appBackend.Repositories;
using appBackend.Models;
using Azure.Core;

namespace appBackend.Services
{
    public class UserSettingsService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserAdapter _userAdapter;
        private readonly string _jwtSecret;
        private readonly IPasswordHasher<User> _passwordHasher; // Inject Password Hasher

        public UserSettingsService(
            IUserRepository userRepository, 
            IUserAdapter userAdapter, 
            IPasswordHasher<User> passwordHasher,  // Accept as dependency
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _userAdapter = userAdapter;
            _passwordHasher = passwordHasher;
            _jwtSecret = configuration["JwtSettings:Secret"] ?? throw new ArgumentNullException("JWT Secret is missing!");
        }

        public async Task<User> GetUserAsync(Guid userId)
        {
            var user = await _userRepository.GetUserByUserId(userId);

            return user;
        }

        public async Task<User> UpdateUserAsync(User updatedUser)
        {
            var user = await _userRepository.UpdateUserAsync(updatedUser);

            return user ?? updatedUser;
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
        {
            var user = await _userRepository.GetUserByUserId(userId);
            if (user == null) return false; // User not found

            // Verify the old password
            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, oldPassword);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return false; // Incorrect old password
            }

            // Hash the new password
            var newPasswordHash = _passwordHasher.HashPassword(user, newPassword);

            // Update the user model (ideally via a dedicated repository method)
            user.PasswordHash = newPasswordHash;
            user.UpdatedAt = DateTime.UtcNow;

            var updatedUser = await _userRepository.UpdateUserAsync(user); // Or a specific UpdatePasswordHashAsync method

            return updatedUser != null;
        }
    }
}
