using appBackend.DTOs; // For VoteRequest
using appBackend.Models;
using appBackend.Repositories; // For IPollRepository
using appBackend.Services; // The service under test
using Microsoft.EntityFrameworkCore;
using Moq;

namespace appBackend.Tests.Services
{
    public class PollServiceTest
    {
        private readonly Mock<IPollRepository> _mockPollRepository;
        private readonly Mock<SocialMediaDbContext> _mockContext;
        private readonly PollService _pollService;

        // Mock DbSets - these will hold our test data
        private readonly Mock<DbSet<Poll>> _mockPollsSet;
        private readonly Mock<DbSet<Vote>> _mockVotesSet;

        // Data stores for DbSets
        private List<Poll> _pollDataStore;
        private List<Vote> _voteDataStore;


        public PollServiceTest()
        {
            _mockPollRepository = new Mock<IPollRepository>();

            // --- Mocking DbContext and DbSets ---
            _mockContext = new Mock<SocialMediaDbContext>(new DbContextOptions<SocialMediaDbContext>()); // Need options for base constructor

            _pollDataStore = new List<Poll>(); // Initialize data stores
            _voteDataStore = new List<Vote>();

            // Create mock DbSet instances pointing to our in-memory lists
            _mockPollsSet = CreateMockDbSet(_pollDataStore);
            _mockVotesSet = CreateMockDbSet(_voteDataStore);

            // Setup the DbContext mock to return the mock DbSets
            _mockContext.Setup(c => c.Polls).Returns(_mockPollsSet.Object);
            _mockContext.Setup(c => c.Votes).Returns(_mockVotesSet.Object);

            _mockContext.Setup(c => c.SaveChanges()).Returns(() =>
            {
                return 1;
            });

            _mockContext.Setup(c => c.Entry(It.IsAny<object>()))
                        .Returns<object>(entity =>
                        {
                            var mockEntry = new Mock<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry>(entity);
                            mockEntry.SetupProperty(e => e.State); // Allows setting the State property
                            mockEntry.Setup(e => e.Entity).Returns(entity);
                            return mockEntry.Object;
                        });


            _pollService = new PollService(_mockPollRepository.Object, _mockContext.Object);
        }

        private Mock<DbSet<T>> CreateMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            // Mock Add operation to add to the list
            mockSet.Setup(m => m.Add(It.IsAny<T>())).Callback<T>((entity) => sourceList.Add(entity));

            // Mock Remove operation to remove from the list
            mockSet.Setup(m => m.Remove(It.IsAny<T>())).Callback<T>((entity) => sourceList.Remove(entity));

            // Mock AddRange
            mockSet.Setup(m => m.AddRange(It.IsAny<IEnumerable<T>>())).Callback<IEnumerable<T>>((entities) => sourceList.AddRange(entities));

            // Mock RemoveRange
            mockSet.Setup(m => m.RemoveRange(It.IsAny<IEnumerable<T>>())).Callback<IEnumerable<T>>((entities) =>
            {
                foreach (var entity in entities.ToList()) // ToList to avoid modification during iteration
                {
                    sourceList.Remove(entity);
                }
            });

            mockSet.Setup(m => m.Add(It.IsAny<T>()))
           .Callback<T>((entity) => sourceList.Add(entity));

    mockSet.Setup(m => m.AddRange(It.IsAny<IEnumerable<T>>()))
           .Callback<IEnumerable<T>>((entities) => sourceList.AddRange(entities));

    // Make Remove slightly more robust by finding the item in the list by reference or key
    mockSet.Setup(m => m.Remove(It.IsAny<T>()))
           .Callback<T>((entity) =>
           {
               // Find the entity in the list. This handles cases where EF might track
               // a different instance than the one originally added.
               // This example assumes an 'Equals' method or relies on reference equality
               // You might need to adjust to find by ID if reference equality fails.
               var entityToRemove = sourceList.FirstOrDefault(e => e.Equals(entity));
               if (entityToRemove != null)
               {
                   sourceList.Remove(entityToRemove);
               }
               else
               {
                   // Fallback or handle cases where the exact instance isn't found
                   // Maybe try removing the first item that matches by key?
                   // Or just try removing the passed entity directly if FirstOrDefault fails
                   sourceList.Remove(entity);
               }
           });


    mockSet.Setup(m => m.RemoveRange(It.IsAny<IEnumerable<T>>()))
           .Callback<IEnumerable<T>>((entities) =>
           {
               foreach (var entity in entities.ToList()) // Use ToList to avoid issues modifying collection during iteration
               {
                   // Use the same logic as single Remove
                   var entityToRemove = sourceList.FirstOrDefault(e => e.Equals(entity));
                   if (entityToRemove != null)
                   {
                       sourceList.Remove(entityToRemove);
                   }
                   else
                   {
                       sourceList.Remove(entity);
                   }
               }
           });

    return mockSet;

            return mockSet;
        }

        [Fact]
        public void CreatePoll_ShouldAddPollToRepository_WhenDataIsValid()
        {
            // Arrange
            var poll = new Poll
            {
                // PollId will be set by service if Guid.Empty
                UserId = Guid.NewGuid(),
                Question = "Test Question?",
                Options = new List<string> { "Option 1", "Option 2" },
                IsGlobal = true,
                AllowMultipleSelections = false
                // CreatedAt will be set by service
            };

            Poll? addedPoll = null;
            _mockPollRepository.Setup(r => r.Add(It.IsAny<Poll>()))
                               .Callback<Poll>(p => addedPoll = p); // Capture the poll passed to Add

            // Act
            _pollService.CreatePoll(poll);

            // Assert
            _mockPollRepository.Verify(r => r.Add(It.Is<Poll>(p =>
                p.Question == poll.Question &&
                p.UserId == poll.UserId &&
                p.Options.Count == 2 &&
                p.PollId != Guid.Empty && 
                p.CreatedAt != default(DateTime) 
            )), Times.Once);

            Assert.NotNull(addedPoll);
            Assert.NotEqual(Guid.Empty, addedPoll.PollId);
            Assert.True(addedPoll.CreatedAt > DateTime.UtcNow.AddMinutes(-1)); 
            Assert.NotNull(addedPoll.Votes); 
        }

        [Theory]
        [InlineData(null, "Q", new[] { "A", "B" }, true)] // Null UserId
        [InlineData("00000000-0000-0000-0000-000000000000", "Q", new[] { "A", "B" }, true)] // Empty Guid UserId
        [InlineData("11111111-1111-1111-1111-111111111111", null, new[] { "A", "B" }, true)] // Null Question
        [InlineData("11111111-1111-1111-1111-111111111111", " ", new[] { "A", "B" }, true)] // Whitespace Question
        [InlineData("11111111-1111-1111-1111-111111111111", "Q", null, true)] // Null Options
        [InlineData("11111111-1111-1111-1111-111111111111", "Q", new string[] { "A" }, true)] // Only one option
        [InlineData("11111111-1111-1111-1111-111111111111", "Q", new[] { "A", null }, true)] // Null option
        [InlineData("11111111-1111-1111-1111-111111111111", "Q", new[] { "A", " " }, true)] // Whitespace option
        public void CreatePoll_ShouldThrowArgumentException_WhenDataIsInvalid(string userIdStr, string question, string[] options, bool isGlobal)
        {
            // Arrange
            Guid.TryParse(userIdStr ?? string.Empty, out Guid userId); // Handle null userIdStr

            var poll = new Poll
            {
                UserId = userId,
                Question = question,
                Options = options?.ToList(), // Handle null options array
                IsGlobal = isGlobal
            };


            if (userIdStr == null) // Specific case for null UserId test where the object itself might be the issue
            {
                poll = new Poll { Question = question, Options = options?.ToList(), IsGlobal = isGlobal }; // UserId is default(Guid) here
            }


            // Act & Assert
            Assert.Throws<ArgumentException>(() => _pollService.CreatePoll(poll));
            _mockPollRepository.Verify(r => r.Add(It.IsAny<Poll>()), Times.Never);
        }

        [Fact]
        public void GetAllPolls_ShouldReturnAllPollsOrderedByDateDesc()
        {
            // Arrange
            var polls = new List<Poll>
            {
                new Poll { PollId = Guid.NewGuid(), Question = "Poll 2", CreatedAt = DateTime.UtcNow.AddHours(-1), Options = new List<string>{"a", "b"} },
                new Poll { PollId = Guid.NewGuid(), Question = "Poll 1", CreatedAt = DateTime.UtcNow, Options = new List<string>{"c", "d"} },
                new Poll { PollId = Guid.NewGuid(), Question = "Poll 3", CreatedAt = DateTime.UtcNow.AddHours(-2), Options = new List<string>{"e", "f"} }
            };
            _mockPollRepository.Setup(r => r.GetAll()).Returns(polls.AsQueryable()); // Return as IQueryable if repo does

            // Act
            var result = _pollService.GetAllPolls().ToList(); // ToList to execute

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("Poll 1", result[0].Question); // Should be the newest
            Assert.Equal("Poll 2", result[1].Question);
            Assert.Equal("Poll 3", result[2].Question); // Should be the oldest
            _mockPollRepository.Verify(r => r.GetAll(), Times.Once);
        }

        [Fact]
        public void GetPollById_ShouldReturnPoll_WhenFound()
        {
            // Arrange
            var pollId = Guid.NewGuid();
            var expectedPoll = new Poll { PollId = pollId, Question = "Find Me", Options = new List<string> { "X", "Y" } };
            _mockPollRepository.Setup(r => r.GetById(pollId)).Returns(expectedPoll);

            // Act
            var result = _pollService.GetPollById(pollId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(pollId, result.PollId);
            Assert.Equal(expectedPoll.Question, result.Question);
            _mockPollRepository.Verify(r => r.GetById(pollId), Times.Once);
        }

        [Fact]
        public void GetPollById_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            var pollId = Guid.NewGuid();
            _mockPollRepository.Setup(r => r.GetById(pollId)).Returns((Poll?)null);

            // Act
            var result = _pollService.GetPollById(pollId);

            // Assert
            Assert.Null(result);
            _mockPollRepository.Verify(r => r.GetById(pollId), Times.Once);
        }

        [Theory]
        [InlineData(true, null, null)] // Multi-select, null indices
        [InlineData(true, new int[] { }, null)] // Multi-select, empty indices
        [InlineData(true, new int[] { 99 }, null)] // Multi-select, invalid index
        [InlineData(false, null, null)] // Single-select, null index
        [InlineData(false, null, 99)] // Single-select, invalid index
        public void CastVote_ShouldThrowArgumentException_WhenInvalidInput(bool multiSelect, int[] optionIndices, int? optionIndex)
        {
            // Arrange
            var pollId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var poll = new Poll { PollId = pollId, Options = new List<string> { "A", "B" }, AllowMultipleSelections = multiSelect };
            _pollDataStore.Add(poll);

            var request = new VoteRequest
            {
                UserId = userId,
                OptionIndex = optionIndex,
                OptionIndices = optionIndices?.ToList()
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _pollService.CastVote(pollId, request));
            _mockContext.Verify(c => c.SaveChanges(), Times.Never);
            _mockVotesSet.Verify(v => v.Add(It.IsAny<Vote>()), Times.Never);
            _mockVotesSet.Verify(v => v.Remove(It.IsAny<Vote>()), Times.Never);
        }

        [Fact]
        public void CastVote_ShouldThrowArgumentException_WhenPollNotFound()
        {
            // Arrange
            var pollId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new VoteRequest { UserId = userId, OptionIndex = 0 };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _pollService.CastVote(pollId, request));
            _mockContext.Verify(c => c.SaveChanges(), Times.Never);
        }

        [Fact]
        public void RetractVote_ShouldDoNothing_WhenVoteDoesNotExist()
        {
            // Arrange
            var pollId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var poll = new Poll
            {
                PollId = pollId,
                UserId = Guid.NewGuid(),
                Question = "Q",
                Options = new List<string> { "Opt 0", "Opt 1" },
                Votes = new Dictionary<int, int> { { 0, 5 }, { 1, 3 } },
                AllowMultipleSelections = false
            };
            _pollDataStore.Add(poll);

            // Act
            _pollService.RetractVote(pollId, userId);

            // Assert
            _mockVotesSet.Verify(v => v.Remove(It.IsAny<Vote>()), Times.Never);
            _mockContext.Verify(c => c.SaveChanges(), Times.Never);
            Assert.Equal(5, poll.Votes[0]);
            Assert.Equal(3, poll.Votes[1]);
        }

        [Fact]
        public void RetractVote_ShouldThrowArgumentException_WhenPollNotFound()
        {
            // Arrange
            var pollId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _voteDataStore.Add(new Vote { PollId = pollId, UserId = userId, OptionIndex = 0 });

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _pollService.RetractVote(pollId, userId));
            _mockContext.Verify(c => c.SaveChanges(), Times.Never);
        }
    }
}