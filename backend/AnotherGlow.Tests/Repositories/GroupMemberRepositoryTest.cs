using appBackend.Models;
using appBackend.Repositories; // Contains DbContext
using appBackend.Services; // Contains Repository and Interface if not moved
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace appBackend.Tests.Repositories
{
    public class GroupMemberRepositoryTest
    {
        private readonly Mock<SocialMediaDbContext> _mockContext;
        private readonly Mock<DbSet<GroupMember>> _mockGroupMemberSet;
        private readonly GroupMemberRepository _repository;
        private readonly List<GroupMember> _groupMemberList;

        public GroupMemberRepositoryTest()
        {
            _mockContext = new Mock<SocialMediaDbContext>(new DbContextOptions<SocialMediaDbContext>());
            _groupMemberList = new List<GroupMember>();
            _mockGroupMemberSet = MockDbSetHelper.CreateMockDbSet(_groupMemberList);

            _mockContext.Setup(c => c.GroupMembers).Returns(_mockGroupMemberSet.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(1); // Simulate successful async save

            _repository = new GroupMemberRepository(_mockContext.Object);
        }

        [Fact]
        public async Task AddGroupMembersAsync_ShouldAddMembersAndReturnTrue_WhenListIsNotEmpty()
        {
            // Arrange
            var membersToAdd = new List<GroupMember>
            {
                new GroupMember { GroupId = Guid.NewGuid(), UserId = Guid.NewGuid() },
                new GroupMember { GroupId = Guid.NewGuid(), UserId = Guid.NewGuid() }
            };

            // Act
            var result = await _repository.AddGroupMembersAsync(membersToAdd);

            // Assert
            Assert.True(result);
            _mockGroupMemberSet.Verify(m => m.AddRange(It.Is<List<GroupMember>>(l => l.Count == 2)), Times.Once());
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.Equal(2, _groupMemberList.Count); // Check underlying list
        }

        [Fact]
        public async Task AddGroupMembersAsync_ShouldReturnFalse_WhenListIsEmpty()
        {
            // Arrange
            var membersToAdd = new List<GroupMember>(); // Empty list

            // Act
            var result = await _repository.AddGroupMembersAsync(membersToAdd);

            // Assert
            Assert.False(result);
            _mockGroupMemberSet.Verify(m => m.AddRangeAsync(It.IsAny<IEnumerable<GroupMember>>(), It.IsAny<CancellationToken>()), Times.Never());
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task AddGroupMembersAsync_ShouldReturnFalse_WhenSaveChangesFails()
        {
            // Arrange
            var membersToAdd = new List<GroupMember>
            {
                new GroupMember { GroupId = Guid.NewGuid(), UserId = Guid.NewGuid() }
            };

            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new DbUpdateException("Simulated DB error")); // Simulate failure

            // Act
            var result = await _repository.AddGroupMembersAsync(membersToAdd);

            // Assert
            Assert.False(result);
            _mockGroupMemberSet.Verify(m => m.AddRange(It.IsAny<List<GroupMember>>()), Times.Once()); // AddRange was called
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once()); // SaveChanges was called (and failed)
        }

        [Fact]
        public async Task RemoveMemberAsync_ShouldRemoveMemberAndReturnTrue_WhenMemberExists()
        {
            // Arrange
            var memberIdToRemove = Guid.NewGuid();
            var memberToRemove = new GroupMember { GroupMemberId = memberIdToRemove, GroupId = Guid.NewGuid(), UserId = Guid.NewGuid() };
            _groupMemberList.Add(memberToRemove);
            _groupMemberList.Add(new GroupMember { GroupMemberId = Guid.NewGuid(), GroupId = Guid.NewGuid(), UserId = Guid.NewGuid() }); // Another member


            // Act
            var result = await _repository.RemoveMemberAsync(memberIdToRemove);

            // Assert
            Assert.True(result);
            // Verify FindAsync was called (or equivalent logic in helper)
            _mockGroupMemberSet.Verify(m => m.FindAsync(It.Is<object[]>(ids => (Guid)ids[0] == memberIdToRemove)), Times.Once());
            _mockGroupMemberSet.Verify(m => m.Remove(It.Is<GroupMember>(gm => gm.GroupMemberId == memberIdToRemove)), Times.Once());
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.Single(_groupMemberList); // Check underlying list
            Assert.DoesNotContain(_groupMemberList, gm => gm.GroupMemberId == memberIdToRemove);
        }

        [Fact]
        public async Task RemoveMemberAsync_ShouldReturnFalse_WhenMemberDoesNotExist()
        {
            // Arrange
            var memberIdToRemove = Guid.NewGuid(); // Does not exist
            _groupMemberList.Add(new GroupMember { GroupMemberId = Guid.NewGuid(), GroupId = Guid.NewGuid(), UserId = Guid.NewGuid() });

            // Need to ensure FindAsync returns null for the non-existent ID
            _mockGroupMemberSet.Setup(m => m.FindAsync(It.Is<object[]>(ids => (Guid)ids[0] == memberIdToRemove)))
                               .ReturnsAsync((GroupMember?)null);


            // Act
            var result = await _repository.RemoveMemberAsync(memberIdToRemove);

            // Assert
            Assert.False(result);
            _mockGroupMemberSet.Verify(m => m.FindAsync(It.Is<object[]>(ids => (Guid)ids[0] == memberIdToRemove)), Times.Once());
            _mockGroupMemberSet.Verify(m => m.Remove(It.IsAny<GroupMember>()), Times.Never());
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }
    }
}