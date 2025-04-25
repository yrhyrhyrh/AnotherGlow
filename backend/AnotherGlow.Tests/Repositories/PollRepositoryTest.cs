using appBackend.Models;
using appBackend.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;

namespace appBackend.Tests.Repositories
{
    public class PollRepositoryTest
    {
        private readonly Mock<SocialMediaDbContext> _mockContext;
        private readonly Mock<DbSet<Poll>> _mockPollSet;
        private readonly PollRepository _repository;
        private readonly List<Poll> _pollList;

        public PollRepositoryTest()
        {
            _mockContext = new Mock<SocialMediaDbContext>(new DbContextOptions<SocialMediaDbContext>());
            _pollList = new List<Poll>();
            _mockPollSet = MockDbSetHelper.CreateMockDbSet(_pollList);

            _mockContext.Setup(c => c.Polls).Returns(_mockPollSet.Object);
            _mockContext.Setup(c => c.SaveChanges()).Returns(1);

            _repository = new PollRepository(_mockContext.Object);
        }


        [Fact]
        public void GetAll_ShouldReturnAllPolls()
        {
            // Arrange
            _pollList.Add(new Poll { PollId = Guid.NewGuid(), Question = "Q1" });
            _pollList.Add(new Poll { PollId = Guid.NewGuid(), Question = "Q2" });

            // Act
            var result = _repository.GetAll();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void GetById_ShouldReturnCorrectPoll_WhenExists()
        {
            // Arrange
            var pollId = Guid.NewGuid();
            var expectedPoll = new Poll { PollId = pollId, Question = "Find Me" };
            _pollList.Add(expectedPoll);
            _pollList.Add(new Poll { PollId = Guid.NewGuid(), Question = "Ignore Me" });


            // Act
            var result = _repository.GetById(pollId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(pollId, result.PollId);
            Assert.Equal("Find Me", result.Question);
        }

        [Fact]
        public void GetById_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            _pollList.Add(new Poll { PollId = Guid.NewGuid(), Question = "Some Poll" });

            // Act
            var result = _repository.GetById(nonExistentId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Add_ShouldAddPollAndSaveChanges()
        {
            // Arrange
            var newPoll = new Poll { PollId = Guid.NewGuid(), Question = "New Poll" };

            // Act
            _repository.Add(newPoll);

            // Assert
            _mockPollSet.Verify(m => m.Add(It.Is<Poll>(p => p == newPoll)), Times.Once());
            _mockContext.Verify(m => m.SaveChanges(), Times.Once());
            Assert.Contains(newPoll, _pollList); // Check if added to the underlying list
        }
    }
}