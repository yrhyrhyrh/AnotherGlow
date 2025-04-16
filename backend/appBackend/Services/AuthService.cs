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

namespace appBackend.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserAdapter _userAdapter;
        private readonly string _jwtSecret;
        private readonly IPasswordHasher<User> _passwordHasher; // Inject Password Hasher

        public AuthService(
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

        public string GenerateJwtToken(string userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] 
                { 
                    new Claim("userId", userId) 
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<Guid?> RegisterUserAsync(RegisterRequest registerRequest)
        {
            Console.WriteLine("Registering user: " + registerRequest.Username);

            var userEntity = _userAdapter.ToUser(registerRequest);

            // Hash the password before storing it
            userEntity.PasswordHash = _passwordHasher.HashPassword(userEntity, registerRequest.Password);

            var createdUser = await _userRepository.CreateUserAsync(userEntity);
            
            if (createdUser == null) return null;

            return createdUser.UserId;
        }

        public async Task<Guid?> ValidateUserAsync(string username, string password)
        {
            Console.WriteLine("Validating user...");

            var user = await _userRepository.GetUserByCredentialsAsync(username);
            if (user == null) return null;

            // Verify password hash
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            Console.WriteLine("Password result: " + result);

            if (result == PasswordVerificationResult.Success)
                return user.UserId;
            else
                return null;
        }
    }
}
