using appBackend.Models;
using appBackend.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace appBackend.Tests.Repositories
{
    public class UserRepositoryTest
    {
        private readonly Mock<SocialMediaDbContext> _mockContext;
        private readonly Mock<DbSet<User>> _mockUserSet;
        private readonly UserRepository _repository;
        private readonly List<User> _userList;

        public UserRepositoryTest()
        {
            _mockContext = new Mock<SocialMediaDbContext>(new DbContextOptions<SocialMediaDbContext>());
            _userList = new List<User>();
            _mockUserSet = MockDbSetHelper.CreateMockDbSet(_userList);

            _mockContext.Setup(c => c.Users).Returns(_mockUserSet.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(1); // Simulate successful async save

            _repository = new UserRepository(_mockContext.Object);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldAddUserAndSaveChangesAsync()
        {
            // Arrange
            var newUser = new User { UserId = Guid.NewGuid(), Username = "testuser", Email = "test@example.com", PasswordHash = "hash" };

            // Act
            var result = await _repository.CreateUserAsync(newUser);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newUser.UserId, result.UserId);
            Assert.Contains(newUser, _userList);
        }

        [Fact]
        public async Task GetUserByCredentialsAsync_ShouldReturnUser_WhenUsernameExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var username = "findme";
            var expectedUser = new User { UserId = userId, Username = username, Email = "find@me.com", PasswordHash = "hash" };
            _userList.Add(expectedUser);
            _userList.Add(new User { UserId = Guid.NewGuid(), Username = "otheruser", Email = "other@me.com", PasswordHash = "hash" });


            // Act
            var result = await _repository.GetUserByCredentialsAsync(username);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(username, result.Username);
        }

        [Fact]
        public async Task GetUserByCredentialsAsync_ShouldReturnNull_WhenUsernameDoesNotExist()
        {
            // Arrange
            var username = "nosuchuser";
            _userList.Add(new User { UserId = Guid.NewGuid(), Username = "otheruser", Email = "other@me.com", PasswordHash = "hash" });

            // Act
            var result = await _repository.GetUserByCredentialsAsync(username);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByUserId_ShouldReturnUser_WhenUserIdExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedUser = new User { UserId = userId, Username = "userbyid", Email = "id@me.com", PasswordHash = "hash" };
            _userList.Add(expectedUser);
            _userList.Add(new User { UserId = Guid.NewGuid(), Username = "otheruser", Email = "other@me.com", PasswordHash = "hash" });


            // Act
            var result = await _repository.GetUserByUserId(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal("userbyid", result.Username);
        }

        [Fact]
        public async Task GetUserByUserId_ShouldThrow_WhenUserIdDoesNotExist() // FirstAsync throws if not found
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userList.Add(new User { UserId = Guid.NewGuid(), Username = "otheruser", Email = "other@me.com", PasswordHash = "hash" });


            // Act & Assert
            await Assert.ThrowsAsync<TargetInvocationException>(() => _repository.GetUserByUserId(userId));
        }


        [Fact]
        public async Task UpdateUserAsync_ShouldUpdateUserAndSaveChangesAsync_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var originalUser = new User { UserId = userId, Username = "original", Email = "original@test.com", FullName = "Orig Name", Bio = "Bio", JobRole = "Dev", PasswordHash = "hash", CreatedAt = DateTimeOffset.UtcNow.AddDays(-1), UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1) };
            _userList.Add(originalUser);

            var updatedUserData = new User
            {
                UserId = userId, // Match the ID
                Username = "updated",
                Email = "updated@test.com",
                FullName = "Updated Name",
                Bio = "Updated Bio",
                JobRole = "Senior Dev"
                // Do not provide PasswordHash or CreatedAt
            };

            // Act
            var result = await _repository.UpdateUserAsync(updatedUserData);

            // Assert
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());

            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            // Check that properties were updated on the *original* object in the list
            Assert.Equal("updated", originalUser.Username);
            Assert.Equal("updated@test.com", originalUser.Email);
            Assert.Equal("Updated Name", originalUser.FullName);
            Assert.Equal("Updated Bio", originalUser.Bio);
            Assert.Equal("Senior Dev", originalUser.JobRole);
            // Check that sensitive/immutable fields were NOT updated
            Assert.Equal("hash", originalUser.PasswordHash);
            Assert.NotEqual(updatedUserData.CreatedAt, originalUser.CreatedAt); // CreatedAt should not change
            Assert.True(originalUser.UpdatedAt > originalUser.CreatedAt); // UpdatedAt should have changed
            Assert.True(result.UpdatedAt > originalUser.CreatedAt); // Check the returned object too
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var nonExistentUserId = Guid.NewGuid();
            var updatedUserData = new User { UserId = nonExistentUserId, Username = "updatefail" };

            // Act
            var result = await _repository.UpdateUserAsync(updatedUserData);

            // Assert
            Assert.Null(result);
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never()); // Save shouldn't be called
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldReturnNull_WhenSaveChangesFails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var originalUser = new User { UserId = userId, Username = "original" };
            _userList.Add(originalUser);
            var updatedUserData = new User { UserId = userId, Username = "updated" };

            // Setup SaveChangesAsync to throw an exception
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new DbUpdateException("Simulated DB error"));

            // Act
            var result = await _repository.UpdateUserAsync(updatedUserData);

            // Assert
            Assert.Null(result);
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once()); // Verify it was called
        }
    }
}