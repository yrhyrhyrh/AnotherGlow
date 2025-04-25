using appBackend.Models;
using appBackend.Repositories; // Assuming GroupRepository is here now
using appBackend.Services; // Contains IGroupRepository if not moved
using appBackend.Dtos.Group;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace appBackend.Tests.Repositories
{
    public class GroupRepositoryTest
    {
        private readonly Mock<SocialMediaDbContext> _mockContext;
        private readonly Mock<DbSet<Group>> _mockGroupSet;
        private readonly Mock<DbSet<GroupMember>> _mockGroupMemberSet;
        private readonly Mock<DbSet<User>> _mockUserSet;
        private readonly GroupRepository _repository;
        private readonly List<Group> _groupList;
        private readonly List<GroupMember> _groupMemberList;
        private readonly List<User> _userList;

        public GroupRepositoryTest()
        {
            _mockContext = new Mock<SocialMediaDbContext>(new DbContextOptions<SocialMediaDbContext>());

            _groupList = new List<Group>();
            _groupMemberList = new List<GroupMember>();
            _userList = new List<User>();

            _mockGroupSet = MockDbSetHelper.CreateMockDbSet(_groupList);
            _mockGroupMemberSet = MockDbSetHelper.CreateMockDbSet(_groupMemberList);
            _mockUserSet = MockDbSetHelper.CreateMockDbSet(_userList);

            _mockContext.Setup(c => c.Groups).Returns(_mockGroupSet.Object);
            _mockContext.Setup(c => c.GroupMembers).Returns(_mockGroupMemberSet.Object);
            _mockContext.Setup(c => c.Users).Returns(_mockUserSet.Object);

            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(1); 

            _repository = new GroupRepository(_mockContext.Object);
        }

        [Fact]
        public async Task GetGroupsByUserIdAsync_ShouldReturnAdminGroups_WhenIsAdminTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var group1 = new Group { GroupId = Guid.NewGuid(), Name = "Admin Group 1" };
            var group2 = new Group { GroupId = Guid.NewGuid(), Name = "Member Group" };
            var group3 = new Group { GroupId = Guid.NewGuid(), Name = "Admin Group 2" };
            _groupList.AddRange(new[] { group1, group2, group3 });

            _groupMemberList.Add(new GroupMember { GroupMemberId = Guid.NewGuid(), UserId = userId, GroupId = group1.GroupId, IsAdmin = true, Group = group1 });
            _groupMemberList.Add(new GroupMember { GroupMemberId = Guid.NewGuid(), UserId = userId, GroupId = group2.GroupId, IsAdmin = false, Group = group2 }); // User is member, not admin
            _groupMemberList.Add(new GroupMember { GroupMemberId = Guid.NewGuid(), UserId = userId, GroupId = group3.GroupId, IsAdmin = true, Group = group3 });
            _groupMemberList.Add(new GroupMember { GroupMemberId = Guid.NewGuid(), UserId = Guid.NewGuid(), GroupId = group1.GroupId, IsAdmin = false, Group = group1 }); // Different user

            // Act
            var result = await _repository.GetGroupsByUserIdAsync(userId, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, g => g.GroupId == group1.GroupId);
            Assert.Contains(result, g => g.GroupId == group3.GroupId);
            Assert.DoesNotContain(result, g => g.GroupId == group2.GroupId);
        }

        [Fact]
        public async Task GetGroupsByUserIdAsync_ShouldReturnMemberGroups_WhenIsAdminFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var group1 = new Group { GroupId = Guid.NewGuid(), Name = "Admin Group 1" };
            var group2 = new Group { GroupId = Guid.NewGuid(), Name = "Member Group" };
            _groupList.AddRange(new[] { group1, group2 });

            _groupMemberList.Add(new GroupMember { GroupMemberId = Guid.NewGuid(), UserId = userId, GroupId = group1.GroupId, IsAdmin = true, Group = group1 });
            _groupMemberList.Add(new GroupMember { GroupMemberId = Guid.NewGuid(), UserId = userId, GroupId = group2.GroupId, IsAdmin = false, Group = group2 });

            // Act
            var result = await _repository.GetGroupsByUserIdAsync(userId, false);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(group2.GroupId, result[0].GroupId);
        }

        [Fact]
        public async Task GetGroupsByUserIdAsync_ShouldReturnEmptyList_WhenNoMatchingGroups()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var group1 = new Group { GroupId = Guid.NewGuid(), Name = "Other Group" };
            _groupList.Add(group1);
            _groupMemberList.Add(new GroupMember { GroupMemberId = Guid.NewGuid(), UserId = Guid.NewGuid(), GroupId = group1.GroupId, IsAdmin = true, Group = group1 });

            // Act
            var result = await _repository.GetGroupsByUserIdAsync(userId, true);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetGroupAsync_ShouldReturnGroupDtoWithMembers_WhenGroupExists()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid(); // This user is an admin
            var otherUserId = Guid.NewGuid();
            var adminUser = new User { UserId = currentUserId, Username = "admin", ProfilePictureUrl = "admin.jpg" };
            var memberUser = new User { UserId = otherUserId, Username = "member", ProfilePictureUrl = "member.jpg" };
            _userList.AddRange(new[] { adminUser, memberUser });

            var group = new Group
            {
                GroupId = groupId,
                Name = "Test Group",
                Description = "Desc",
                GroupPictureUrl = "group.png",
                Members = new List<GroupMember>() // Need to populate this for the query
            };
            _groupList.Add(group);

            var member1 = new GroupMember { GroupMemberId = Guid.NewGuid(), GroupId = groupId, UserId = currentUserId, IsAdmin = true, User = adminUser, Group = group };
            var member2 = new GroupMember { GroupMemberId = Guid.NewGuid(), GroupId = groupId, UserId = otherUserId, IsAdmin = false, User = memberUser, Group = group };
            _groupMemberList.AddRange(new[] { member1, member2 });
            // Manually link members back to group for the mock query to work if needed,
            // although the Select projection should handle it based on the GroupMembers DbSet.
            group.Members.Add(member1);
            group.Members.Add(member2);


            // Act
            var result = await _repository.GetGroupAsync(groupId, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(groupId, result.GroupId);
            Assert.Equal("Test Group", result.Name);
            Assert.Equal("Desc", result.Description);
            Assert.Equal("group.png", result.GroupPictureUrl);
            Assert.True(result.IsAdmin); // currentUserId is admin
            Assert.Equal(2, result.Members.Count);

            var adminMemberDto = result.Members.FirstOrDefault(m => m.User.UserId == currentUserId); // UserDto UserId is string
            Assert.NotNull(adminMemberDto);
            Assert.True(adminMemberDto.IsAdmin);
            Assert.Equal("admin", adminMemberDto.User.Username);
            Assert.Equal("admin.jpg", adminMemberDto.User.ProfilePictureUrl);

            var regularMemberDto = result.Members.FirstOrDefault(m => m.User.UserId == otherUserId);
            Assert.NotNull(regularMemberDto);
            Assert.False(regularMemberDto.IsAdmin);
            Assert.Equal("member", regularMemberDto.User.Username);
            Assert.Equal("member.jpg", regularMemberDto.User.ProfilePictureUrl);
        }

        [Fact]
        public async Task GetGroupAsync_ShouldReturnGroupDtoWithIsAdminFalse_WhenCurrentUserIsNotAdmin()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid(); // This user is NOT an admin
            var adminUserId = Guid.NewGuid();
            var nonAdminUser = new User { UserId = currentUserId, Username = "viewer", ProfilePictureUrl = "viewer.jpg" };
            var adminUser = new User { UserId = adminUserId, Username = "admin", ProfilePictureUrl = "admin.jpg" };
            _userList.AddRange(new[] { nonAdminUser, adminUser });

            var group = new Group { GroupId = groupId, Name = "Test Group" };
            _groupList.Add(group);

            var member1 = new GroupMember { GroupMemberId = Guid.NewGuid(), GroupId = groupId, UserId = currentUserId, IsAdmin = false, User = nonAdminUser, Group = group };
            var member2 = new GroupMember { GroupMemberId = Guid.NewGuid(), GroupId = groupId, UserId = adminUserId, IsAdmin = true, User = adminUser, Group = group };
            _groupMemberList.AddRange(new[] { member1, member2 });
            group.Members.Add(member1); // Link for query if needed
            group.Members.Add(member2);

            // Act
            var result = await _repository.GetGroupAsync(groupId, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsAdmin); // Current user is not admin
            Assert.Equal(2, result.Members.Count); // Still see all members
        }

        [Fact]
        public async Task GetGroupAsync_ShouldReturnNull_WhenGroupDoesNotExist()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();

            // Act
            var result = await _repository.GetGroupAsync(groupId, currentUserId);

            // Assert
            Assert.Null(result);
        }


        [Fact]
        public async Task CreateGroupAsync_ShouldAddGroupAndReturnId_WhenNameIsUnique()
        {
            // Arrange
            var newGroup = new Group { Name = "Unique Group", Description = "Desc", GroupPictureUrl = "pic.jpg" };

            // Act
            var result = await _repository.CreateGroupAsync(newGroup);

            // Assert
            Assert.NotEqual(Guid.Empty, result); // Should get a new Guid
            _mockGroupSet.Verify(m => m.Add(It.Is<Group>(g => g.Name == newGroup.Name && g.GroupId != Guid.Empty)), Times.Once());
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.Contains(_groupList, g => g.Name == newGroup.Name);
        }

        [Fact]
        public async Task CreateGroupAsync_ShouldReturnEmptyGuid_WhenNameExists()
        {
            // Arrange
            var existingGroup = new Group { GroupId = Guid.NewGuid(), Name = "Existing Group" };
            _groupList.Add(existingGroup);

            var newGroup = new Group { Name = "Existing Group" }; // Same name

            // Act
            var result = await _repository.CreateGroupAsync(newGroup);

            // Assert
            Assert.Equal(Guid.Empty, result);
            _mockGroupSet.Verify(m => m.Add(It.IsAny<Group>()), Times.Never());
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task SearchUsersNotInGroupAsync_ShouldReturnUsersNotInGroupMatchingKeyword()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var userInGroup = new User { UserId = Guid.NewGuid(), Username = "useringroup", ProfilePictureUrl = "in.jpg" };
            var userNotInGroupMatch = new User { UserId = Guid.NewGuid(), Username = "findme_user", ProfilePictureUrl = "match.jpg" };
            var userNotInGroupNoMatch = new User { UserId = Guid.NewGuid(), Username = "another", ProfilePictureUrl = "no.jpg" };
            var userAlsoInGroup = new User { UserId = Guid.NewGuid(), Username = "findme_too_but_in_group", ProfilePictureUrl = "in2.jpg" };
            _userList.AddRange(new[] { userInGroup, userNotInGroupMatch, userNotInGroupNoMatch, userAlsoInGroup });

            _groupMemberList.Add(new GroupMember { GroupMemberId = Guid.NewGuid(), GroupId = groupId, UserId = userInGroup.UserId });
            _groupMemberList.Add(new GroupMember { GroupMemberId = Guid.NewGuid(), GroupId = groupId, UserId = userAlsoInGroup.UserId });

            var keyword = "findme";

            // Act
            var result = await _repository.SearchUsersNotInGroupAsync(groupId, keyword);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(userNotInGroupMatch.UserId, result[0].UserId);
            Assert.Equal("findme_user", result[0].Username);
            Assert.Equal("match.jpg", result[0].ProfilePictureUrl);
        }

        [Fact]
        public async Task SearchUsersNotInGroupAsync_ShouldReturnAllUsersNotInGroup_WhenKeywordIsEmpty()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var userInGroup = new User { UserId = Guid.NewGuid(), Username = "useringroup" };
            var userNotIn1 = new User { UserId = Guid.NewGuid(), Username = "aaa_not_in" }; // Should come first alphabetically
            var userNotIn2 = new User { UserId = Guid.NewGuid(), Username = "zzz_not_in" };
            _userList.AddRange(new[] { userInGroup, userNotIn1, userNotIn2 });

            _groupMemberList.Add(new GroupMember { GroupMemberId = Guid.NewGuid(), GroupId = groupId, UserId = userInGroup.UserId });

            var keyword = ""; // Empty keyword

            // Act
            var result = await _repository.SearchUsersNotInGroupAsync(groupId, keyword);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(userNotIn1.UserId, result[0].UserId); // Check order
            Assert.Equal(userNotIn2.UserId, result[1].UserId);
        }

        [Fact]
        public async Task SearchUsersNotInGroupAsync_ShouldReturnEmptyList_WhenAllUsersAreInGroup()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var userInGroup1 = new User { UserId = Guid.NewGuid(), Username = "user1" };
            var userInGroup2 = new User { UserId = Guid.NewGuid(), Username = "user2" };
            _userList.AddRange(new[] { userInGroup1, userInGroup2 });

            _groupMemberList.Add(new GroupMember { GroupMemberId = Guid.NewGuid(), GroupId = groupId, UserId = userInGroup1.UserId });
            _groupMemberList.Add(new GroupMember { GroupMemberId = Guid.NewGuid(), GroupId = groupId, UserId = userInGroup2.UserId });

            var keyword = "user";

            // Act
            var result = await _repository.SearchUsersNotInGroupAsync(groupId, keyword);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchUsersNotInGroupAsync_ShouldReturnEmptyList_WhenNoUsersMatchKeyword()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var userInGroup = new User { UserId = Guid.NewGuid(), Username = "useringroup" };
            var userNotInGroup = new User { UserId = Guid.NewGuid(), Username = "available" };
            _userList.AddRange(new[] { userInGroup, userNotInGroup });

            _groupMemberList.Add(new GroupMember { GroupMemberId = Guid.NewGuid(), GroupId = groupId, UserId = userInGroup.UserId });

            var keyword = "nomatch";

            // Act
            var result = await _repository.SearchUsersNotInGroupAsync(groupId, keyword);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}