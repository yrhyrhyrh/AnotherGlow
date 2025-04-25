using appBackend.Adapters;
using appBackend.Models;
using appBackend.Repositories;
using Moq;
using Microsoft.Extensions.Configuration;
using appBackend.Services;
using appBackend.Dtos.Group;
using appBackend.Dtos;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using System.Net; 

namespace appBackend.Tests.Services
{
    public class GroupServiceTest
    {
        // Mocks for dependencies
        private readonly Mock<IGroupRepository> _mockGroupRepository;
        private readonly Mock<IGroupAdapter> _mockGroupAdapter;
        private readonly Mock<IAmazonS3> _mockS3Client;
        private readonly Mock<IConfiguration> _mockConfiguration;

        // Service under test
        private readonly GroupService _groupService;

        // Test configuration values
        private readonly string _testBucketName = "test-bucket";
        private readonly string _testCloudFrontDomain = "d123testcfdomain.cloudfront.net";

        public GroupServiceTest()
        {
            _mockGroupRepository = new Mock<IGroupRepository>();
            _mockGroupAdapter = new Mock<IGroupAdapter>();
            _mockS3Client = new Mock<IAmazonS3>();
            _mockConfiguration = new Mock<IConfiguration>();

            var mockS3BucketSection = new Mock<IConfigurationSection>();
            mockS3BucketSection.Setup(s => s.Value).Returns(_testBucketName);
            _mockConfiguration.Setup(c => c["Aws:S3BucketName"]).Returns(_testBucketName);

            var mockCfDomainSection = new Mock<IConfigurationSection>();
            mockCfDomainSection.Setup(s => s.Value).Returns(_testCloudFrontDomain);
            _mockConfiguration.Setup(c => c["Aws:CfDistributionDomainName"]).Returns(_testCloudFrontDomain);

            // Instantiate the service with mocked dependencies
            _groupService = new GroupService(
                _mockGroupRepository.Object,
                _mockGroupAdapter.Object,
                _mockS3Client.Object,
                _mockConfiguration.Object // Pass the mocked configuration
            );
        }


        [Fact]
        public async Task GetGroupsByUserIdAsync_ShouldReturnGroups_WhenUserIsAdminAndGroupsExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new GetGroupsByUseridRequest { UserId = userId, IsAdmin = true };
            var expectedGroups = new List<Group>
            {
                new Group { GroupId = Guid.NewGuid(), Name = "Admin Group 1" },
                new Group { GroupId = Guid.NewGuid(), Name = "Admin Group 2" }
            };

            _mockGroupRepository.Setup(r => r.GetGroupsByUserIdAsync(userId, true))
                               .ReturnsAsync(expectedGroups);

            // Act
            var result = await _groupService.GetGroupsByUserIdAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedGroups.Count, result.Count);
            Assert.Equal(expectedGroups, result); 
            _mockGroupRepository.Verify(r => r.GetGroupsByUserIdAsync(userId, true), Times.Once);
        }

        [Fact]
        public async Task GetGroupsByUserIdAsync_ShouldReturnGroups_WhenUserIsNotAdminAndGroupsExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new GetGroupsByUseridRequest { UserId = userId, IsAdmin = false };
            var expectedGroups = new List<Group>
            {
                new Group { GroupId = Guid.NewGuid(), Name = "Member Group 1" }
            };

            _mockGroupRepository.Setup(r => r.GetGroupsByUserIdAsync(userId, false))
                               .ReturnsAsync(expectedGroups);

            // Act
            var result = await _groupService.GetGroupsByUserIdAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result); // Expecting one group in this setup
            Assert.Equal(expectedGroups, result);
            _mockGroupRepository.Verify(r => r.GetGroupsByUserIdAsync(userId, false), Times.Once);
        }

        [Fact]
        public async Task GetGroupsByUserIdAsync_ShouldReturnEmptyList_WhenNoGroupsFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new GetGroupsByUseridRequest { UserId = userId, IsAdmin = false };
            var expectedGroups = new List<Group>(); // Empty list

            _mockGroupRepository.Setup(r => r.GetGroupsByUserIdAsync(userId, false))
                               .ReturnsAsync(expectedGroups);

            // Act
            var result = await _groupService.GetGroupsByUserIdAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockGroupRepository.Verify(r => r.GetGroupsByUserIdAsync(userId, false), Times.Once);
        }

        [Fact]
        public async Task GetGroupAsync_ShouldReturnGroupDto_WhenGroupExists()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();
            var expectedGroupDto = new GroupDto { GroupId = groupId, Name = "Test Group Detail" /* Add other props */ };

            _mockGroupRepository.Setup(r => r.GetGroupAsync(groupId, currentUserId))
                               .ReturnsAsync(expectedGroupDto);

            // Act
            var result = await _groupService.GetGroupAsync(groupId, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedGroupDto.GroupId, result.GroupId);
            Assert.Equal(expectedGroupDto.Name, result.Name);
            _mockGroupRepository.Verify(r => r.GetGroupAsync(groupId, currentUserId), Times.Once);
        }

        [Fact]
        public async Task GetGroupAsync_ShouldReturnNull_WhenGroupDoesNotExist()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();

            _mockGroupRepository.Setup(r => r.GetGroupAsync(groupId, currentUserId))
                               .ReturnsAsync((GroupDto?)null); // Simulate group not found

            // Act
            var result = await _groupService.GetGroupAsync(groupId, currentUserId);

            // Assert
            Assert.Null(result);
            _mockGroupRepository.Verify(r => r.GetGroupAsync(groupId, currentUserId), Times.Once);
        }

        [Fact]
        public async Task CreateGroupAsync_ShouldReturnGroupId_WhenCreatedSuccessfully_WithoutPicture()
        {
            // Arrange
            var request = new CreateNewGroupRequest { Name = "New Group No Pic", Description = "Desc", UserId = Guid.NewGuid(), GroupPicture = null };
            var groupEntity = new Group { Name = request.Name, Description = request.Description /* other props */ };
            var expectedGroupId = Guid.NewGuid();

            _mockGroupAdapter.Setup(a => a.ToGroup(request)).Returns(groupEntity);
            _mockGroupRepository.Setup(r => r.CreateGroupAsync(It.Is<Group>(g => g == groupEntity && g.GroupPictureUrl == null)))
                               .ReturnsAsync(expectedGroupId);

            // Act
            var result = await _groupService.CreateGroupAsync(request);

            // Assert
            Assert.Equal(expectedGroupId, result);
            _mockGroupAdapter.Verify(a => a.ToGroup(request), Times.Once);
            _mockGroupRepository.Verify(r => r.CreateGroupAsync(It.Is<Group>(g => g == groupEntity && g.GroupPictureUrl == null)), Times.Once);
            // Ensure S3 client was NOT called
            _mockS3Client.Verify(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateGroupAsync_ShouldReturnGroupIdAndUploadPicture_WhenCreatedSuccessfully_WithPicture()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var mockFile = CreateMockFormFile("test.jpg", "image/jpeg", "dummy image content");
            var request = new CreateNewGroupRequest { Name = "New Group With Pic", Description = "Desc", UserId = adminId, GroupPicture = mockFile.Object };

            var groupEntity = new Group { Name = request.Name, Description = request.Description }; // GroupPictureUrl will be set by service
            var expectedGroupId = Guid.NewGuid();
            string expectedS3KeyPattern = $"group-pictures/{Guid.NewGuid()}_{mockFile.Object.FileName}"; // Pattern, exact Guid unknown
            string expectedCloudFrontUrlPattern = $"https://{_testCloudFrontDomain}/group-pictures/";

            _mockGroupAdapter.Setup(a => a.ToGroup(request)).Returns(groupEntity);

            // Mock S3 upload response
            var putResponse = new PutObjectResponse { HttpStatusCode = HttpStatusCode.OK };
            _mockS3Client.Setup(s3 => s3.PutObjectAsync(It.Is<PutObjectRequest>(p =>
                               p.BucketName == _testBucketName &&
                               p.Key.StartsWith("group-pictures/") && // Check prefix
                               p.Key.EndsWith("_" + mockFile.Object.FileName) && // Check suffix
                               p.ContentType == mockFile.Object.ContentType
                            ), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(putResponse)
                         .Callback<PutObjectRequest, CancellationToken>((req, ct) => {
                             // Capture the actual S3 key used if needed, or just use it in the repository mock setup below
                             // Store req.Key somewhere if needed for very precise assertion
                         });


            // Mock Repository call - *Crucially*, it should be called with the entity having the CloudFront URL set
            _mockGroupRepository.Setup(r => r.CreateGroupAsync(It.Is<Group>(g =>
                                    g == groupEntity &&
                                    g.GroupPictureUrl != null &&
                                    g.GroupPictureUrl.StartsWith(expectedCloudFrontUrlPattern) && // Check the domain and path prefix
                                    g.GroupPictureUrl.EndsWith("_" + mockFile.Object.FileName) // Check the filename part
                                )))
                               .ReturnsAsync(expectedGroupId);


            // Act
            var result = await _groupService.CreateGroupAsync(request);

            // Assert
            Assert.Equal(expectedGroupId, result);
            _mockGroupAdapter.Verify(a => a.ToGroup(request), Times.Once);
            // Verify S3 Upload was called correctly
            _mockS3Client.Verify(s3 => s3.PutObjectAsync(It.Is<PutObjectRequest>(p =>
                                p.BucketName == _testBucketName &&
                                p.Key.StartsWith("group-pictures/") &&
                                p.Key.EndsWith("_" + mockFile.Object.FileName) &&
                                p.ContentType == mockFile.Object.ContentType
                             ), It.IsAny<CancellationToken>()), Times.Once);
            // Verify Repository was called with the updated entity
            _mockGroupRepository.Verify(r => r.CreateGroupAsync(It.Is<Group>(g =>
                                    g == groupEntity &&
                                    g.GroupPictureUrl != null &&
                                    g.GroupPictureUrl.StartsWith(expectedCloudFrontUrlPattern) &&
                                    g.GroupPictureUrl.EndsWith("_" + mockFile.Object.FileName)
                                )), Times.Once);
        }

        [Fact]
        public async Task CreateGroupAsync_ShouldReturnGroupIdAndSetNullUrl_WhenS3UploadFails()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var mockFile = CreateMockFormFile("fail.jpg", "image/jpeg", "dummy image content");
            var request = new CreateNewGroupRequest { Name = "New Group S3 Fail", Description = "Desc", UserId = adminId, GroupPicture = mockFile.Object };

            var groupEntity = new Group { Name = request.Name, Description = request.Description };
            var expectedGroupId = Guid.NewGuid();

            _mockGroupAdapter.Setup(a => a.ToGroup(request)).Returns(groupEntity);

            // Mock S3 upload response - FAILED
            var putResponse = new PutObjectResponse { HttpStatusCode = HttpStatusCode.InternalServerError }; // Or BadRequest etc.
            _mockS3Client.Setup(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(putResponse); // Simulate failure

            // Mock Repository call - Should be called with GroupPictureUrl as null because upload failed
            _mockGroupRepository.Setup(r => r.CreateGroupAsync(It.Is<Group>(g => g == groupEntity && g.GroupPictureUrl == null)))
                               .ReturnsAsync(expectedGroupId);

            // Act
            var result = await _groupService.CreateGroupAsync(request);

            // Assert
            Assert.Equal(expectedGroupId, result);
            _mockGroupAdapter.Verify(a => a.ToGroup(request), Times.Once);
            _mockS3Client.Verify(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()), Times.Once); // S3 was attempted
            _mockGroupRepository.Verify(r => r.CreateGroupAsync(It.Is<Group>(g => g == groupEntity && g.GroupPictureUrl == null)), Times.Once); // Repository called with null URL
        }

        [Fact]
        public async Task SearchUsersNotInGroupAsync_ShouldReturnUsers_WhenUsersFound()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var keyword = "search";
            var expectedUsers = new List<UserDto>
            {
                new UserDto { UserId = Guid.NewGuid(), Username = "user1_found" },
                new UserDto { UserId = Guid.NewGuid(), Username = "user2_found" }
            };

            _mockGroupRepository.Setup(r => r.SearchUsersNotInGroupAsync(groupId, keyword))
                               .ReturnsAsync(expectedUsers);

            // Act
            var result = await _groupService.SearchUsersNotInGroupAsync(groupId, keyword);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedUsers.Count, result.Count);
            Assert.Equal(expectedUsers, result);
            _mockGroupRepository.Verify(r => r.SearchUsersNotInGroupAsync(groupId, keyword), Times.Once);
        }

        [Fact]
        public async Task SearchUsersNotInGroupAsync_ShouldReturnEmptyList_WhenNoUsersFound()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var keyword = "nosearchresults";
            var expectedUsers = new List<UserDto>(); // Empty list

            _mockGroupRepository.Setup(r => r.SearchUsersNotInGroupAsync(groupId, keyword))
                               .ReturnsAsync(expectedUsers);

            // Act
            var result = await _groupService.SearchUsersNotInGroupAsync(groupId, keyword);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockGroupRepository.Verify(r => r.SearchUsersNotInGroupAsync(groupId, keyword), Times.Once);
        }

        // --- Helper Method to Mock IFormFile ---
        private Mock<IFormFile> CreateMockFormFile(string fileName, string contentType, string content)
        {
            var fileMock = new Mock<IFormFile>();
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(ms.Length);
            fileMock.Setup(_ => _.ContentType).Returns(contentType);
            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            fileMock.Setup(_ => _.ContentDisposition).Returns($"form-data; name=\"file\"; filename=\"{fileName}\"");

            return fileMock;
        }
    }
}