using appBackend.Adapters;
using appBackend.Models;
using appBackend.Repositories;
using Microsoft.AspNetCore.Identity;
using Moq;
using Microsoft.Extensions.Configuration;
using appBackend.Services;
using appBackend.Dtos.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace appBackend.Tests.Services // Adjusted namespace for clarity
{
    public class AuthServiceTest // Changed to public for test runner visibility
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IUserAdapter> _mockUserAdapter;
        private readonly Mock<IPasswordHasher<User>> _mockPasswordHasher;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly AuthService _authService;
        private readonly string _testJwtSecret = "THIS_IS_A_TEST_SECRET_THAT_IS_LONG_ENOUGH_FOR_HS256";

        public AuthServiceTest()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockUserAdapter = new Mock<IUserAdapter>();
            _mockPasswordHasher = new Mock<IPasswordHasher<User>>();
            _mockConfiguration = new Mock<IConfiguration>();

            // Setup IConfiguration mock to return the test secret
            _mockConfiguration.Setup(c => c["JwtSettings:Secret"]).Returns(_testJwtSecret);
            _authService = new AuthService(
                _mockUserRepository.Object,
                _mockUserAdapter.Object,
                _mockPasswordHasher.Object,
                _mockConfiguration.Object);
        }

        [Fact]
        public void GenerateJwtToken_ShouldReturnValidToken_WhenUserIdIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();

            // Act
            var tokenString = _authService.GenerateJwtToken(userId);

            // Assert
            Assert.NotNull(tokenString);

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_testJwtSecret)),
                ValidateIssuer = false, // Not set in AuthService, so validate false
                ValidateAudience = false, // Not set in AuthService, so validate false
                ValidateLifetime = true, // Check expiration
                ClockSkew = TimeSpan.Zero // Consider adding a small skew if needed
            };

            // Validate the token structure and signature
            SecurityToken validatedToken;
            var principal = tokenHandler.ValidateToken(tokenString, validationParameters, out validatedToken);

            Assert.NotNull(principal);
            Assert.NotNull(validatedToken);
            Assert.IsType<JwtSecurityToken>(validatedToken);

            // Validate claims
            var jwtToken = (JwtSecurityToken)validatedToken;
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "userId");
            Assert.NotNull(userIdClaim);
            Assert.Equal(userId, userIdClaim?.Value);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldReturnUserId_WhenRegistrationIsSuccessful()
        {
            // Arrange
            var registerRequest = new RegisterRequest { Username = "testuser", Password = "password123", Email = "test@example.com" };
            var userEntity = new User { Username = registerRequest.Username, Email = registerRequest.Email /* Other props */ };
            var createdUser = new User { UserId = Guid.NewGuid(), Username = userEntity.Username, Email = userEntity.Email, PasswordHash = "hashedpassword" };
            var hashedPassword = "hashedpassword";

            _mockUserAdapter.Setup(a => a.ToUser(registerRequest)).Returns(userEntity);
            _mockPasswordHasher.Setup(h => h.HashPassword(userEntity, registerRequest.Password)).Returns(hashedPassword);
            _mockUserRepository.Setup(r => r.CreateUserAsync(It.Is<User>(u => u.PasswordHash == hashedPassword)))
                               .ReturnsAsync(createdUser); // Ensure the user passed has the hash set

            // Act
            var result = await _authService.RegisterUserAsync(registerRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createdUser.UserId, result);

            _mockUserAdapter.Verify(a => a.ToUser(registerRequest), Times.Once);
            _mockPasswordHasher.Verify(h => h.HashPassword(userEntity, registerRequest.Password), Times.Once);
            _mockUserRepository.Verify(r => r.CreateUserAsync(It.Is<User>(u => u == userEntity && u.PasswordHash == hashedPassword)), Times.Once);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldReturnNull_WhenRepositoryFailsToCreateUser()
        {
            // Arrange
            var registerRequest = new RegisterRequest { Username = "testuser", Password = "password123", Email = "test@example.com" };
            var userEntity = new User { Username = registerRequest.Username, Email = registerRequest.Email };
            var hashedPassword = "hashedpassword";

            _mockUserAdapter.Setup(a => a.ToUser(registerRequest)).Returns(userEntity);
            _mockPasswordHasher.Setup(h => h.HashPassword(userEntity, registerRequest.Password)).Returns(hashedPassword);
            _mockUserRepository.Setup(r => r.CreateUserAsync(It.Is<User>(u => u.PasswordHash == hashedPassword)))
                               .ReturnsAsync((User?)null); // Simulate creation failure

            // Act
            var result = await _authService.RegisterUserAsync(registerRequest);

            // Assert
            Assert.Null(result);

            _mockUserAdapter.Verify(a => a.ToUser(registerRequest), Times.Once);
            _mockPasswordHasher.Verify(h => h.HashPassword(userEntity, registerRequest.Password), Times.Once);
            _mockUserRepository.Verify(r => r.CreateUserAsync(It.Is<User>(u => u == userEntity && u.PasswordHash == hashedPassword)), Times.Once);
        }

        [Fact]
        public async Task ValidateUserAsync_ShouldReturnUserId_WhenCredentialsAreValid()
        {
            // Arrange
            var username = "validuser";
            var password = "correctpassword";
            var userId = Guid.NewGuid();
            var hashedPassword = "hashedpassword_for_validuser";
            var user = new User { UserId = userId, Username = username, PasswordHash = hashedPassword };

            _mockUserRepository.Setup(r => r.GetUserByCredentialsAsync(username)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(h => h.VerifyHashedPassword(user, hashedPassword, password))
                               .Returns(PasswordVerificationResult.Success);

            // Act
            var result = await _authService.ValidateUserAsync(username, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result);

            _mockUserRepository.Verify(r => r.GetUserByCredentialsAsync(username), Times.Once);
            _mockPasswordHasher.Verify(h => h.VerifyHashedPassword(user, hashedPassword, password), Times.Once);
        }

        [Fact]
        public async Task ValidateUserAsync_ShouldReturnNull_WhenUserNotFound()
        {
            // Arrange
            var username = "unknownuser";
            var password = "anypassword";

            _mockUserRepository.Setup(r => r.GetUserByCredentialsAsync(username)).ReturnsAsync((User?)null);

            // Act
            var result = await _authService.ValidateUserAsync(username, password);

            // Assert
            Assert.Null(result);

            _mockUserRepository.Verify(r => r.GetUserByCredentialsAsync(username), Times.Once);
            // Password hasher should not be called if user is not found
            _mockPasswordHasher.Verify(h => h.VerifyHashedPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ValidateUserAsync_ShouldReturnNull_WhenPasswordIsIncorrect()
        {
            // Arrange
            var username = "validuser";
            var password = "wrongpassword";
            var userId = Guid.NewGuid();
            var hashedPassword = "hashedpassword_for_validuser";
            var user = new User { UserId = userId, Username = username, PasswordHash = hashedPassword };

            _mockUserRepository.Setup(r => r.GetUserByCredentialsAsync(username)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(h => h.VerifyHashedPassword(user, hashedPassword, password))
                               .Returns(PasswordVerificationResult.Failed); // Simulate wrong password

            // Act
            var result = await _authService.ValidateUserAsync(username, password);

            // Assert
            Assert.Null(result);

            _mockUserRepository.Verify(r => r.GetUserByCredentialsAsync(username), Times.Once);
            _mockPasswordHasher.Verify(h => h.VerifyHashedPassword(user, hashedPassword, password), Times.Once);
        }

        [Fact]
        public async Task ValidateUserAsync_ShouldReturnNull_WhenPasswordVerificationNeedsRehash()
        {
            // Arrange
            var username = "validuser_needsrehash";
            var password = "correctpassword";
            var userId = Guid.NewGuid();
            var oldHashedPassword = "old_hashedpassword";
            var user = new User { UserId = userId, Username = username, PasswordHash = oldHashedPassword };

            _mockUserRepository.Setup(r => r.GetUserByCredentialsAsync(username)).ReturnsAsync(user);
            // Simulate scenario where hash algorithm changed, but password is correct
            _mockPasswordHasher.Setup(h => h.VerifyHashedPassword(user, oldHashedPassword, password))
                               .Returns(PasswordVerificationResult.SuccessRehashNeeded);

            // Act
            var result = await _authService.ValidateUserAsync(username, password);

            // Assert
            Assert.Null(result);

            _mockUserRepository.Verify(r => r.GetUserByCredentialsAsync(username), Times.Once);
            _mockPasswordHasher.Verify(h => h.VerifyHashedPassword(user, oldHashedPassword, password), Times.Once);
        }
    }
}