using appBackend.Adapters;
using appBackend.Models;
using appBackend.Repositories;
using Moq;
using appBackend.Services;
using appBackend.Dtos.Group; // Assuming GroupMemberRequest is here
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit; // Make sure Xunit is referenced

namespace appBackend.Tests.Services
{
    public class GroupMemberServiceTest
    {
        // Mocks for dependencies
        private readonly Mock<IGroupMemberRepository> _mockGroupMemberRepository;
        private readonly Mock<IGroupMemberAdapter> _mockGroupMemberAdapter;

        // Service under test
        private readonly GroupMemberService _groupMemberService;

        public GroupMemberServiceTest()
        {
            _mockGroupMemberRepository = new Mock<IGroupMemberRepository>();
            _mockGroupMemberAdapter = new Mock<IGroupMemberAdapter>();

            // Instantiate the service with mocked dependencies
            _groupMemberService = new GroupMemberService(
                _mockGroupMemberRepository.Object,
                _mockGroupMemberAdapter.Object
            );
        }

        // --- Test Methods for AddGroupMembersAsync ---

        [Fact]
        public async Task AddGroupMembersAsync_ShouldReturnTrue_WhenMembersAreAddedSuccessfully()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var memberRequests = new List<GroupMemberRequest>
            {
                new GroupMemberRequest { GroupId = groupId, UserId = Guid.NewGuid(), IsAdmin = false },
                new GroupMemberRequest { GroupId = groupId, UserId = Guid.NewGuid(), IsAdmin = false}
            };

            var correspondingMembers = new List<GroupMember>
            {
                new GroupMember { GroupId = groupId, UserId = memberRequests[0].UserId, IsAdmin = false, GroupMemberId = Guid.NewGuid() },
                new GroupMember { GroupId = groupId, UserId = memberRequests[1].UserId, IsAdmin = false, GroupMemberId = Guid.NewGuid() }
            };

            // Setup adapter to return the corresponding member for each request
            _mockGroupMemberAdapter.Setup(a => a.ToGroupMember(memberRequests[0])).Returns(correspondingMembers[0]);
            _mockGroupMemberAdapter.Setup(a => a.ToGroupMember(memberRequests[1])).Returns(correspondingMembers[1]);

            // Setup repository to simulate successful addition
            _mockGroupMemberRepository.Setup(r => r.AddGroupMembersAsync(It.Is<List<GroupMember>>(list =>
                list.Count == 2 &&
                list.Contains(correspondingMembers[0]) &&
                list.Contains(correspondingMembers[1])
            ))).ReturnsAsync(true);

            // Act
            var result = await _groupMemberService.AddGroupMembersAsync(memberRequests);

            // Assert
            Assert.True(result);

            // Verify adapter was called for each request
            _mockGroupMemberAdapter.Verify(a => a.ToGroupMember(memberRequests[0]), Times.Once);
            _mockGroupMemberAdapter.Verify(a => a.ToGroupMember(memberRequests[1]), Times.Once);

            // Verify repository was called once with the correct list of members
            _mockGroupMemberRepository.Verify(r => r.AddGroupMembersAsync(It.Is<List<GroupMember>>(list =>
                list.Count == 2 &&
                list.Contains(correspondingMembers[0]) &&
                list.Contains(correspondingMembers[1])
            )), Times.Once);
        }

        [Fact]
        public async Task AddGroupMembersAsync_ShouldReturnFalse_WhenRepositoryFailsToAddMembers()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var memberRequests = new List<GroupMemberRequest>
            {
                new GroupMemberRequest { GroupId = groupId, UserId = Guid.NewGuid(), IsAdmin = false }
            };

            var correspondingMember = new GroupMember { GroupId = groupId, UserId = memberRequests[0].UserId, IsAdmin = false, GroupMemberId = Guid.NewGuid() };

            _mockGroupMemberAdapter.Setup(a => a.ToGroupMember(memberRequests[0])).Returns(correspondingMember);

            // Setup repository to simulate failure
            _mockGroupMemberRepository.Setup(r => r.AddGroupMembersAsync(It.Is<List<GroupMember>>(list => list.Count == 1 && list.Contains(correspondingMember))))
                                       .ReturnsAsync(false);

            // Act
            var result = await _groupMemberService.AddGroupMembersAsync(memberRequests);

            // Assert
            Assert.False(result);

            // Verify interactions
            _mockGroupMemberAdapter.Verify(a => a.ToGroupMember(memberRequests[0]), Times.Once);
            _mockGroupMemberRepository.Verify(r => r.AddGroupMembersAsync(It.Is<List<GroupMember>>(list => list.Count == 1 && list.Contains(correspondingMember))), Times.Once);
        }

        [Fact]
        public async Task AddGroupMembersAsync_ShouldHandleEmptyRequestList_AndReturnTrue()
        {
            // Arrange
            var memberRequests = new List<GroupMemberRequest>(); // Empty list
            var expectedMemberList = new List<GroupMember>(); // Expect repo to be called with empty list

            // Setup repository - adding an empty list should likely succeed (return true)
            _mockGroupMemberRepository.Setup(r => r.AddGroupMembersAsync(It.Is<List<GroupMember>>(list => list.Count == 0)))
                                      .ReturnsAsync(true);

            // Act
            var result = await _groupMemberService.AddGroupMembersAsync(memberRequests);

            // Assert
            Assert.True(result); // Adding zero members succeeded

            // Verify adapter was never called
            _mockGroupMemberAdapter.Verify(a => a.ToGroupMember(It.IsAny<GroupMemberRequest>()), Times.Never);

            // Verify repository was called once with an empty list
            _mockGroupMemberRepository.Verify(r => r.AddGroupMembersAsync(It.Is<List<GroupMember>>(list => list.Count == 0)), Times.Once);
        }


        // --- Test Methods for RemoveMemberAsync ---

        [Fact]
        public async Task RemoveMemberAsync_ShouldReturnTrue_WhenMemberIsRemovedSuccessfully()
        {
            // Arrange
            var groupMemberIdToRemove = Guid.NewGuid();

            // Setup repository to simulate successful removal
            _mockGroupMemberRepository.Setup(r => r.RemoveMemberAsync(groupMemberIdToRemove))
                                       .ReturnsAsync(true);

            // Act
            var result = await _groupMemberService.RemoveMemberAsync(groupMemberIdToRemove);

            // Assert
            Assert.True(result);

            // Verify repository was called once with the correct ID
            _mockGroupMemberRepository.Verify(r => r.RemoveMemberAsync(groupMemberIdToRemove), Times.Once);
        }

        [Fact]
        public async Task RemoveMemberAsync_ShouldReturnFalse_WhenRepositoryFailsToRemoveMember()
        {
            // Arrange
            var groupMemberIdToRemove = Guid.NewGuid();

            // Setup repository to simulate failure (e.g., member not found)
            _mockGroupMemberRepository.Setup(r => r.RemoveMemberAsync(groupMemberIdToRemove))
                                       .ReturnsAsync(false);

            // Act
            var result = await _groupMemberService.RemoveMemberAsync(groupMemberIdToRemove);

            // Assert
            Assert.False(result);

            // Verify repository was called once with the correct ID
            _mockGroupMemberRepository.Verify(r => r.RemoveMemberAsync(groupMemberIdToRemove), Times.Once);
        }
    }
}