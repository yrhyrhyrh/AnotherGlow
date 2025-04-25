using appBackend.Models;
using appBackend.Repositories; // Context
using appBackend.Interfaces.GlobalPostWall; // Interface
using appBackend.Repositories.GlobalPostWall; // Repository implementation
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace appBackend.Tests.Repositories
{
    public class CommentRepositoryTest
    {
        private readonly Mock<SocialMediaDbContext> _mockContext;
        private readonly Mock<DbSet<Comment>> _mockCommentSet;
        private readonly Mock<DbSet<User>> _mockUserSet; // For Include
        private readonly CommentRepository _repository;
        private readonly List<Comment> _commentList;
        private readonly List<User> _userList; // For Include

        public CommentRepositoryTest()
        {
            _mockContext = new Mock<SocialMediaDbContext>(new DbContextOptions<SocialMediaDbContext>());
            _commentList = new List<Comment>();
            _userList = new List<User>();

            _mockCommentSet = MockDbSetHelper.CreateMockDbSet(_commentList);
            _mockUserSet = MockDbSetHelper.CreateMockDbSet(_userList); // Mock user set

            _mockContext.Setup(c => c.Comments).Returns(_mockCommentSet.Object);
            _mockContext.Setup(c => c.Users).Returns(_mockUserSet.Object); // Setup context for users
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

            _repository = new CommentRepository(_mockContext.Object);
        }

        [Fact]
        public async Task GetCommentByIdAsync_ShouldReturnComment_WhenExists()
        {
            // Arrange
            var commentId = Guid.NewGuid();
            var expectedComment = new Comment { CommentId = commentId, Content = "Find Me" };
            _commentList.Add(expectedComment);

            // Act
            var result = await _repository.GetCommentByIdAsync(commentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(commentId, result.CommentId);
            _mockCommentSet.Verify(m => m.FindAsync(It.Is<object[]>(ids => (Guid)ids[0] == commentId)), Times.Once());
        }

        [Fact]
        public async Task GetCommentByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            var commentId = Guid.NewGuid();
            _mockCommentSet.Setup(m => m.FindAsync(It.Is<object[]>(ids => (Guid)ids[0] == commentId))).ReturnsAsync((Comment?)null);


            // Act
            var result = await _repository.GetCommentByIdAsync(commentId);

            // Assert
            Assert.Null(result);
            _mockCommentSet.Verify(m => m.FindAsync(It.Is<object[]>(ids => (Guid)ids[0] == commentId)), Times.Once());
        }

        [Fact]
        public async Task AddCommentAsync_ShouldAddCommentAndReturnIt()
        {
            // Arrange
            var newComment = new Comment { CommentId = Guid.NewGuid(), PostId = Guid.NewGuid(), UserId = Guid.NewGuid(), Content = "New Comment" };

            // Act
            var result = await _repository.AddCommentAsync(newComment);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Comment", result.Content);
            Assert.NotEqual(Guid.Empty, result.CommentId);

            _mockCommentSet.Verify(m => m.Add(It.Is<Comment>(c => c == newComment)), Times.Once());
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.Contains(newComment, _commentList);
        }

        [Fact]
        public async Task DeleteCommentAsync_ShouldRemoveCommentAndReturnTrue_WhenExists()
        {
            // Arrange
            var commentId = Guid.NewGuid();
            var commentToDelete = new Comment { CommentId = commentId, Content = "Delete Me" };
            _commentList.Add(commentToDelete);


            // Act
            var result = await _repository.DeleteCommentAsync(commentId);

            // Assert
            Assert.True(result);
            _mockCommentSet.Verify(m => m.FindAsync(It.Is<object[]>(ids => (Guid)ids[0] == commentId)), Times.Once());
            _mockCommentSet.Verify(m => m.Remove(It.Is<Comment>(c => c.CommentId == commentId)), Times.Once());
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.DoesNotContain(_commentList, c => c.CommentId == commentId);
        }

        [Fact]
        public async Task DeleteCommentAsync_ShouldReturnFalse_WhenNotExists()
        {
            // Arrange
            var commentId = Guid.NewGuid();
            _mockCommentSet.Setup(m => m.FindAsync(It.Is<object[]>(ids => (Guid)ids[0] == commentId))).ReturnsAsync((Comment?)null);


            // Act
            var result = await _repository.DeleteCommentAsync(commentId);

            // Assert
            Assert.False(result);
            _mockCommentSet.Verify(m => m.FindAsync(It.Is<object[]>(ids => (Guid)ids[0] == commentId)), Times.Once());
            _mockCommentSet.Verify(m => m.Remove(It.IsAny<Comment>()), Times.Never());
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetCommentsByPostIdAsync_ShouldReturnOrderedCommentsForPost()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var otherPostId = Guid.NewGuid();
            var user1 = new User { UserId = Guid.NewGuid(), Username = "User1" };
            var user2 = new User { UserId = Guid.NewGuid(), Username = "User2" };
            _userList.AddRange(new[] { user1, user2 });

            var comment1 = new Comment { CommentId = Guid.NewGuid(), PostId = postId, UserId = user1.UserId, Content = "First Comment", CreatedAt = DateTime.UtcNow.AddMinutes(-10), User = user1 };
            var comment2 = new Comment { CommentId = Guid.NewGuid(), PostId = postId, UserId = user2.UserId, Content = "Second Comment", CreatedAt = DateTime.UtcNow.AddMinutes(-5), User = user2 };
            var comment3 = new Comment { CommentId = Guid.NewGuid(), PostId = otherPostId, UserId = user1.UserId, Content = "Other Post Comment", CreatedAt = DateTime.UtcNow.AddMinutes(-1), User = user1 };
            var comment4 = new Comment { CommentId = Guid.NewGuid(), PostId = postId, UserId = user1.UserId, Content = "Third Comment", CreatedAt = DateTime.UtcNow, User = user1 }; // Should be last

            _commentList.AddRange(new[] { comment1, comment2, comment3, comment4 });

            // Act
            var result = await _repository.GetCommentsByPostIdAsync(postId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count); // Only comments for postId
            Assert.Equal(comment1.CommentId, result[0].CommentId); // Ordered by CreatedAt
            Assert.Equal(comment2.CommentId, result[1].CommentId);
            Assert.Equal(comment4.CommentId, result[2].CommentId);

            // Check Includes worked (basic check)
            Assert.NotNull(result[0].User);
            Assert.Equal(user1.Username, result[0].User.Username);
            Assert.NotNull(result[1].User);
            Assert.Equal(user2.Username, result[1].User.Username);
            Assert.NotNull(result[2].User);
            Assert.Equal(user1.Username, result[2].User.Username);
        }

        [Fact]
        public async Task GetCommentsByPostIdAsync_ShouldReturnEmptyList_WhenNoCommentsForPost()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var otherPostId = Guid.NewGuid();
            var user1 = new User { UserId = Guid.NewGuid(), Username = "User1" };
            _userList.Add(user1);
            var comment1 = new Comment { CommentId = Guid.NewGuid(), PostId = otherPostId, UserId = user1.UserId, Content = "Other Post Comment", CreatedAt = DateTime.UtcNow, User = user1 };
            _commentList.Add(comment1);

            // Act
            var result = await _repository.GetCommentsByPostIdAsync(postId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}