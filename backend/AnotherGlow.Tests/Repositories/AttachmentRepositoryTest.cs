using appBackend.Models;
using appBackend.Repositories; // Context
using appBackend.Interfaces.GlobalPostWall; // Interface
using appBackend.Repositories.GlobalPostWall; // Repository implementation
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace appBackend.Tests.Repositories
{
    public class AttachmentRepositoryTest
    {
        private readonly Mock<SocialMediaDbContext> _mockContext;
        private readonly Mock<DbSet<Attachment>> _mockAttachmentSet;
        private readonly AttachmentRepository _repository;
        private readonly List<Attachment> _attachmentList;

        public AttachmentRepositoryTest()
        {
            _mockContext = new Mock<SocialMediaDbContext>(new DbContextOptions<SocialMediaDbContext>());
            _attachmentList = new List<Attachment>();
            _mockAttachmentSet = MockDbSetHelper.CreateMockDbSet(_attachmentList);

            _mockContext.Setup(c => c.Attachments).Returns(_mockAttachmentSet.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(1);

            _repository = new AttachmentRepository(_mockContext.Object);
        }

        [Fact]
        public async Task AddAttachmentAsync_ShouldAddAttachmentAndReturnIt()
        {
            // Arrange
            var newAttachment = new Attachment
            {
                AttachmentId = Guid.NewGuid(),
                PostId = Guid.NewGuid(),
                FileName = "test.png",
                FilePath = "/path/to/test.png",
                ContentType = "image/png",
                FileSize = 1024
            };

            // Act
            var result = await _repository.AddAttachmentAsync(newAttachment);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newAttachment.FileName, result.FileName);
            Assert.NotEqual(Guid.Empty, result.AttachmentId); // Assuming ID is generated on add

            _mockAttachmentSet.Verify(m => m.Add(It.Is<Attachment>(a => a == newAttachment)), Times.Once());
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.Contains(newAttachment, _attachmentList);
        }
    }
}