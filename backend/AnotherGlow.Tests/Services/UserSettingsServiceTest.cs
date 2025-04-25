using appBackend.Adapters;
using appBackend.Models;
using appBackend.Repositories;
using Microsoft.AspNetCore.Identity;
using Moq;
using Microsoft.Extensions.Configuration;
using appBackend.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace appBackend.Tests.Services
{
    public class UserSettingsServiceTest 
    {
        // Mocks for dependencies
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IUserAdapter> _mockUserAdapter; // Included even if not directly used by tested methods yet
        private readonly Mock<IPasswordHasher<User>> _mockPasswordHasher;
        private readonly Mock<IConfiguration> _mockConfiguration;

        // Service under test
        private readonly UserSettingsService _userSettingsService;

        // Test configuration values (optional if not used by methods, but good practice for constructor)
        private readonly string _testJwtSecret = "TEST_JWT_SECRET_FOR_USER_SETTINGS";

        public UserSettingsServiceTest()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockUserAdapter = new Mock<IUserAdapter>(); // Instantiate mock
            _mockPasswordHasher = new Mock<IPasswordHasher<User>>();
            _mockConfiguration = new Mock<IConfiguration>();

            _mockConfiguration.Setup(c => c["JwtSettings:Secret"]).Returns(_testJwtSecret);

            _userSettingsService = new UserSettingsService(
                _mockUserRepository.Object,
                _mockUserAdapter.Object,
                _mockPasswordHasher.Object,
                _mockConfiguration.Object
            );
        }

        [Fact]
        public async Task GetUserAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedUser = new User { UserId = userId, Username = "testuser", Email = "test@example.com" };

            _mockUserRepository.Setup(r => r.GetUserByUserId(userId))
                               .ReturnsAsync(expectedUser);

            // Act
            var result = await _userSettingsService.GetUserAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedUser.UserId, result.UserId);
            Assert.Equal(expectedUser.Username, result.Username);
            _mockUserRepository.Verify(r => r.GetUserByUserId(userId), Times.Once);
        }

        [Fact]
        public async Task GetUserAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockUserRepository.Setup(r => r.GetUserByUserId(userId))
                               .ReturnsAsync((User?)null); // Simulate user not found

            // Act
            var result = await _userSettingsService.GetUserAsync(userId);

            // Assert
            Assert.Null(result);
            _mockUserRepository.Verify(r => r.GetUserByUserId(userId), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldReturnUpdatedUser_WhenUpdateSucceeds()
        {
            // Arrange
            var userToUpdate = new User { UserId = Guid.NewGuid(), Username = "original", Email = "original@test.com", FullName = "Original Name" };
            var updatedUserData = new User { UserId = userToUpdate.UserId, Username = "updated", Email = "updated@test.com", FullName = "Updated Name" }; // Data passed to service
            var repositoryReturnedUser = new User { UserId = userToUpdate.UserId, Username = "updated", Email = "updated@test.com", FullName = "Updated Name", UpdatedAt = DateTimeOffset.UtcNow }; // What repo returns

            _mockUserRepository.Setup(r => r.UpdateUserAsync(updatedUserData))
                               .ReturnsAsync(repositoryReturnedUser);

            // Act
            var result = await _userSettingsService.UpdateUserAsync(updatedUserData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(repositoryReturnedUser.UserId, result.UserId);
            Assert.Equal(repositoryReturnedUser.Username, result.Username);
            Assert.Equal(repositoryReturnedUser.FullName, result.FullName);
            Assert.NotEqual(default(DateTimeOffset), result.UpdatedAt); // Check if UpdatedAt was set (by repo mock)
            _mockUserRepository.Verify(r => r.UpdateUserAsync(updatedUserData), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldReturnInputUser_WhenRepositoryReturnsNull()
        {
            // Arrange
            var userToUpdate = new User { UserId = Guid.NewGuid(), Username = "updatefail", Email = "fail@test.com" };

            _mockUserRepository.Setup(r => r.UpdateUserAsync(userToUpdate))
                               .ReturnsAsync((User?)null); // Simulate repository update failure

            // Act
            var result = await _userSettingsService.UpdateUserAsync(userToUpdate);

            // Assert
            Assert.NotNull(result);
            // Service returns the input user if the repository fails
            Assert.Equal(userToUpdate.UserId, result.UserId);
            Assert.Equal(userToUpdate.Username, result.Username);
            _mockUserRepository.Verify(r => r.UpdateUserAsync(userToUpdate), Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_ShouldReturnTrue_WhenChangeIsSuccessful()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var oldPassword = "oldPassword123";
            var newPassword = "newPassword456";
            var existingHashedPassword = "hashed_old_password";
            var newHashedPassword = "hashed_new_password";
            var user = new User { UserId = userId, PasswordHash = existingHashedPassword, Username = "pwdchangeuser" };
            var updatedUserFromRepo = new User { UserId = userId, PasswordHash = newHashedPassword, Username = "pwdchangeuser", UpdatedAt = DateTimeOffset.UtcNow };

            _mockUserRepository.Setup(r => r.GetUserByUserId(userId)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(h => h.VerifyHashedPassword(user, existingHashedPassword, oldPassword))
                               .Returns(PasswordVerificationResult.Success);
            _mockPasswordHasher.Setup(h => h.HashPassword(user, newPassword))
                               .Returns(newHashedPassword);
            // Verify the UpdateUserAsync call receives the user with the NEW hash
            _mockUserRepository.Setup(r => r.UpdateUserAsync(It.Is<User>(u => u.UserId == userId && u.PasswordHash == newHashedPassword)))
                               .ReturnsAsync(updatedUserFromRepo); // Simulate successful update in repo

            // Act
            var result = await _userSettingsService.ChangePasswordAsync(userId, oldPassword, newPassword);

            // Assert
            Assert.True(result);
            _mockUserRepository.Verify(r => r.GetUserByUserId(userId), Times.Once);
            _mockPasswordHasher.Verify(h => h.VerifyHashedPassword(user, existingHashedPassword, oldPassword), Times.Once);
            _mockPasswordHasher.Verify(h => h.HashPassword(user, newPassword), Times.Once);
            _mockUserRepository.Verify(r => r.UpdateUserAsync(It.Is<User>(u => u.UserId == userId && u.PasswordHash == newHashedPassword)), Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_ShouldReturnFalse_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var oldPassword = "oldPassword123";
            var newPassword = "newPassword456";

            _mockUserRepository.Setup(r => r.GetUserByUserId(userId)).ReturnsAsync((User?)null); // User not found

            // Act
            var result = await _userSettingsService.ChangePasswordAsync(userId, oldPassword, newPassword);

            // Assert
            Assert.False(result);
            _mockUserRepository.Verify(r => r.GetUserByUserId(userId), Times.Once);
            // Verify other methods were NOT called
            _mockPasswordHasher.Verify(h => h.VerifyHashedPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockPasswordHasher.Verify(h => h.HashPassword(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
            _mockUserRepository.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_ShouldReturnFalse_WhenOldPasswordIsIncorrect()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var oldPassword = "wrongOldPassword";
            var newPassword = "newPassword456";
            var existingHashedPassword = "hashed_password";
            var user = new User { UserId = userId, PasswordHash = existingHashedPassword };

            _mockUserRepository.Setup(r => r.GetUserByUserId(userId)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(h => h.VerifyHashedPassword(user, existingHashedPassword, oldPassword))
                               .Returns(PasswordVerificationResult.Failed); // Simulate incorrect password

            // Act
            var result = await _userSettingsService.ChangePasswordAsync(userId, oldPassword, newPassword);

            // Assert
            Assert.False(result);
            _mockUserRepository.Verify(r => r.GetUserByUserId(userId), Times.Once);
            _mockPasswordHasher.Verify(h => h.VerifyHashedPassword(user, existingHashedPassword, oldPassword), Times.Once);
            // Verify other methods were NOT called
            _mockPasswordHasher.Verify(h => h.HashPassword(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
            _mockUserRepository.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_ShouldReturnFalse_WhenUpdateFailsInRepository()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var oldPassword = "oldPassword123";
            var newPassword = "newPassword456";
            var existingHashedPassword = "hashed_old_password";
            var newHashedPassword = "hashed_new_password";
            var user = new User { UserId = userId, PasswordHash = existingHashedPassword };

            _mockUserRepository.Setup(r => r.GetUserByUserId(userId)).ReturnsAsync(user);
            _mockPasswordHasher.Setup(h => h.VerifyHashedPassword(user, existingHashedPassword, oldPassword))
                               .Returns(PasswordVerificationResult.Success);
            _mockPasswordHasher.Setup(h => h.HashPassword(user, newPassword))
                               .Returns(newHashedPassword);
            // Simulate repository update failure
            _mockUserRepository.Setup(r => r.UpdateUserAsync(It.Is<User>(u => u.UserId == userId && u.PasswordHash == newHashedPassword)))
                               .ReturnsAsync((User?)null);

            // Act
            var result = await _userSettingsService.ChangePasswordAsync(userId, oldPassword, newPassword);

            // Assert
            Assert.False(result);
            _mockUserRepository.Verify(r => r.GetUserByUserId(userId), Times.Once);
            _mockPasswordHasher.Verify(h => h.VerifyHashedPassword(user, existingHashedPassword, oldPassword), Times.Once);
            _mockPasswordHasher.Verify(h => h.HashPassword(user, newPassword), Times.Once);
            _mockUserRepository.Verify(r => r.UpdateUserAsync(It.Is<User>(u => u.UserId == userId && u.PasswordHash == newHashedPassword)), Times.Once); // Verify update was attempted
        }
    }
}