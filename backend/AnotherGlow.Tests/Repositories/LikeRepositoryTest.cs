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
    public class LikeRepositoryTest
    {
        private readonly Mock<SocialMediaDbContext> _mockContext;
        private readonly Mock<DbSet<Like>> _mockLikeSet;
        private readonly LikeRepository _repository;
        private readonly List<Like> _likeList;

        public LikeRepositoryTest()
        {
            _mockContext = new Mock<SocialMediaDbContext>(new DbContextOptions<SocialMediaDbContext>());
            _likeList = new List<Like>();
            _mockLikeSet = MockDbSetHelper.CreateMockDbSet(_likeList);

            _mockContext.Setup(c => c.Likes).Returns(_mockLikeSet.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

            _repository = new LikeRepository(_mockContext.Object);
        }

        [Fact]
        public async Task GetLikeAsync_ShouldReturnLike_WhenExists()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var expectedLike = new Like { LikeId = Guid.NewGuid(), PostId = postId, UserId = userId };
            _likeList.Add(expectedLike);
            _likeList.Add(new Like { LikeId = Guid.NewGuid(), PostId = Guid.NewGuid(), UserId = userId }); // Different post
            _likeList.Add(new Like { LikeId = Guid.NewGuid(), PostId = postId, UserId = Guid.NewGuid() }); // Different user

            // Act
            var result = await _repository.GetLikeAsync(postId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedLike.LikeId, result.LikeId);
        }

        [Fact]
        public async Task GetLikeAsync_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _likeList.Add(new Like { LikeId = Guid.NewGuid(), PostId = Guid.NewGuid(), UserId = Guid.NewGuid() }); // Some other like

            // Act
            var result = await _repository.GetLikeAsync(postId, userId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddLikeAsync_ShouldAddLikeAndReturnIt()
        {
            // Arrange
            var newLike = new Like { LikeId = Guid.NewGuid(), PostId = Guid.NewGuid(), UserId = Guid.NewGuid() };

            // Act
            var result = await _repository.AddLikeAsync(newLike);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newLike.PostId, result.PostId);
            Assert.Equal(newLike.UserId, result.UserId);
            Assert.NotEqual(Guid.Empty, result.LikeId);

            _mockLikeSet.Verify(m => m.Add(It.Is<Like>(l => l == newLike)), Times.Once());
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.Contains(newLike, _likeList);
        }

        [Fact]
        public async Task DeleteLikeAsync_ShouldRemoveLikeAndReturnTrue_WhenExists()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var likeToDelete = new Like { LikeId = Guid.NewGuid(), PostId = postId, UserId = userId };
            _likeList.Add(likeToDelete);
            _likeList.Add(new Like { LikeId = Guid.NewGuid(), PostId = Guid.NewGuid(), UserId = userId }); // Different post


            // Act
            var result = await _repository.DeleteLikeAsync(postId, userId);

            // Assert
            Assert.True(result);
            _mockLikeSet.Verify(m => m.Remove(It.Is<Like>(l => l.LikeId == likeToDelete.LikeId)), Times.Once());
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.DoesNotContain(_likeList, l => l.LikeId == likeToDelete.LikeId);
        }

        [Fact]
        public async Task DeleteLikeAsync_ShouldReturnFalse_WhenNotExists()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _likeList.Add(new Like { LikeId = Guid.NewGuid(), PostId = Guid.NewGuid(), UserId = Guid.NewGuid() }); // Some other like

            // Act
            var result = await _repository.DeleteLikeAsync(postId, userId);

            // Assert
            Assert.False(result);
            _mockLikeSet.Verify(m => m.Remove(It.IsAny<Like>()), Times.Never());
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }
    }
}