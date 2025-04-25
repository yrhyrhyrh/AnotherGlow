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
    public class PostRepositoryTest
    {
        private readonly Mock<SocialMediaDbContext> _mockContext;
        private readonly Mock<DbSet<Post>> _mockPostSet;
        private readonly Mock<DbSet<User>> _mockUserSet;
        private readonly Mock<DbSet<Like>> _mockLikeSet;
        private readonly Mock<DbSet<Comment>> _mockCommentSet;
        private readonly Mock<DbSet<Attachment>> _mockAttachmentSet;
        private readonly PostRepository _repository;
        private readonly List<Post> _postList;
        private readonly List<User> _userList;
        private readonly List<Like> _likeList;
        private readonly List<Comment> _commentList;
        private readonly List<Attachment> _attachmentList;

        public PostRepositoryTest()
        {
            _mockContext = new Mock<SocialMediaDbContext>(new DbContextOptions<SocialMediaDbContext>());

            _postList = new List<Post>();
            _userList = new List<User>();
            _likeList = new List<Like>();
            _commentList = new List<Comment>();
            _attachmentList = new List<Attachment>();

            _mockPostSet = MockDbSetHelper.CreateMockDbSet(_postList);
            _mockUserSet = MockDbSetHelper.CreateMockDbSet(_userList);
            _mockLikeSet = MockDbSetHelper.CreateMockDbSet(_likeList);
            _mockCommentSet = MockDbSetHelper.CreateMockDbSet(_commentList);
            _mockAttachmentSet = MockDbSetHelper.CreateMockDbSet(_attachmentList);

            _mockContext.Setup(c => c.Posts).Returns(_mockPostSet.Object);
            _mockContext.Setup(c => c.Users).Returns(_mockUserSet.Object);
            _mockContext.Setup(c => c.Likes).Returns(_mockLikeSet.Object);
            _mockContext.Setup(c => c.Comments).Returns(_mockCommentSet.Object);
            _mockContext.Setup(c => c.Attachments).Returns(_mockAttachmentSet.Object);

            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

            _repository = new PostRepository(_mockContext.Object);
        }

        // Helper to create a Post with linked entities for testing Includes
        private Post CreateTestPost(Guid postId, Guid userId, string username, string content, DateTime createdAt, Guid? groupId = null, int likeCount = 0, int commentCount = 0, int attachmentCount = 0)
        {
            var user = _userList.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                user = new User { UserId = userId, Username = username };
                _userList.Add(user);
            }

            var post = new Post
            {
                PostId = postId,
                UserId = userId,
                Author = user,
                Content = content,
                CreatedAt = createdAt,
                GroupId = groupId ?? Guid.NewGuid(), // Assign a default if null
                Likes = new List<Like>(),
                Comments = new List<Comment>(),
                Attachments = new List<Attachment>()
            };
            _postList.Add(post);

            for (int i = 0; i < likeCount; i++)
            {
                var like = new Like { LikeId = Guid.NewGuid(), PostId = postId, UserId = Guid.NewGuid() }; // Dummy user for like
                _likeList.Add(like);
                post.Likes.Add(like);
            }
            for (int i = 0; i < commentCount; i++)
            {
                var comment = new Comment { CommentId = Guid.NewGuid(), PostId = postId, UserId = Guid.NewGuid(), Content = $"Comment {i}" }; // Dummy user
                _commentList.Add(comment);
                post.Comments.Add(comment);
            }
            for (int i = 0; i < attachmentCount; i++)
            {
                var attachment = new Attachment { AttachmentId = Guid.NewGuid(), PostId = postId, FileName = $"file{i}.txt", FilePath = $"/path/file{i}.txt" };
                _attachmentList.Add(attachment);
                post.Attachments.Add(attachment);
            }
            return post;
        }


        [Fact]
        public async Task GetAllPostsAsync_ShouldReturnAllPostsOrderedByDateDesc()
        {
            // Arrange
            var post1 = CreateTestPost(Guid.NewGuid(), Guid.NewGuid(), "UserA", "Post 1", DateTime.UtcNow.AddHours(-2), likeCount: 1);
            var post2 = CreateTestPost(Guid.NewGuid(), Guid.NewGuid(), "UserB", "Post 2", DateTime.UtcNow, commentCount: 1); // Newest
            var post3 = CreateTestPost(Guid.NewGuid(), Guid.NewGuid(), "UserC", "Post 3", DateTime.UtcNow.AddHours(-1), attachmentCount: 1);

            // Act
            var result = await _repository.GetAllPostsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal(post2.PostId, result[0].PostId); // post2 is newest
            Assert.Equal(post3.PostId, result[1].PostId);
            Assert.Equal(post1.PostId, result[2].PostId);

            // Basic check if Includes might have worked (collections should be non-null)
            Assert.NotNull(result[0].Author);
            Assert.NotNull(result[0].Likes);
            Assert.NotNull(result[0].Comments);
            Assert.NotNull(result[0].Attachments);
            Assert.Single(result[2].Likes); // Post 1 had 1 like
            Assert.Single(result[0].Comments); // Post 2 had 1 comment
            Assert.Single(result[1].Attachments); // Post 3 had 1 attachment
        }

        [Fact]
        public async Task GetPostsPagedAsync_ShouldReturnCorrectPageAndTotalCount_NoGroupId()
        {
            // Arrange
            var user = new User { UserId = Guid.NewGuid(), Username = "Pager" };
            _userList.Add(user);
            for (int i = 0; i < 15; i++) // Create 15 posts
            {
                CreateTestPost(Guid.NewGuid(), user.UserId, user.Username, $"Post {i}", DateTime.UtcNow.AddMinutes(-i));
            }
            int pageNumber = 2;
            int pageSize = 5;
            Guid currentUserId = Guid.NewGuid(); // Not directly used in this repo method query logic

            // Act
            var (posts, totalCount) = await _repository.GetPostsPagedAsync(pageNumber, pageSize, currentUserId);

            // Assert
            Assert.Equal(15, totalCount);
            Assert.NotNull(posts);
            Assert.Equal(pageSize, posts.Count);
            // Check if it skipped the first page (posts 0-4) and got the second page (posts 5-9)
            // Since ordered descending, newest is index 0. Page 1 = 0-4, Page 2 = 5-9.
            Assert.Equal("Post 5", posts[0].Content);
            Assert.Equal("Post 9", posts[4].Content);
        }

        [Fact]
        public async Task GetPostsPagedAsync_ShouldReturnCorrectPageAndTotalCount_WithGroupId()
        {
            // Arrange
            var user = new User { UserId = Guid.NewGuid(), Username = "PagerGroup" };
            _userList.Add(user);
            var groupId1 = Guid.NewGuid();
            var groupId2 = Guid.NewGuid();

            for (int i = 0; i < 8; i++) // 8 posts in group 1
            {
                CreateTestPost(Guid.NewGuid(), user.UserId, user.Username, $"G1 Post {i}", DateTime.UtcNow.AddMinutes(-i), groupId: groupId1);
            }
            for (int i = 0; i < 5; i++) // 5 posts in group 2
            {
                CreateTestPost(Guid.NewGuid(), user.UserId, user.Username, $"G2 Post {i}", DateTime.UtcNow.AddMinutes(-i), groupId: groupId2);
            }

            int pageNumber = 1;
            int pageSize = 5;
            Guid currentUserId = Guid.NewGuid();

            // Act
            var (posts, totalCount) = await _repository.GetPostsPagedAsync(pageNumber, pageSize, currentUserId, groupId1);

            // Assert
            Assert.Equal(8, totalCount); // Total in group 1
            Assert.NotNull(posts);
            Assert.Equal(pageSize, posts.Count);
            // Check if it got the first page (newest 5) for group 1
            Assert.Equal("G1 Post 0", posts[0].Content);
            Assert.Equal("G1 Post 4", posts[4].Content);
        }

        [Fact]
        public async Task GetPostByIdAsync_ShouldReturnPostWithIncludes_WhenExists()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var post = CreateTestPost(postId, Guid.NewGuid(), "UserById", "Content", DateTime.UtcNow, likeCount: 2, commentCount: 1);

            // Act
            var result = await _repository.GetPostByIdAsync(postId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(postId, result.PostId);
            Assert.NotNull(result.Author);
            Assert.Equal("UserById", result.Author.Username);
            Assert.NotNull(result.Likes);
            Assert.Equal(2, result.Likes.Count);
            Assert.NotNull(result.Comments);
            Assert.Single(result.Comments);
            Assert.NotNull(result.Attachments);
            Assert.Empty(result.Attachments); // 0 were created
        }

        [Fact]
        public async Task GetPostByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            var postId = Guid.NewGuid();

            // Act
            var result = await _repository.GetPostByIdAsync(postId);

            // Assert
            Assert.Null(result);
        }


        [Fact]
        public async Task AddPostAsync_ShouldAddPostAndReturnIt()
        {
            // Arrange
            var newPost = new Post { PostId = Guid.NewGuid(), UserId = Guid.NewGuid(), Content = "New post content", GroupId = Guid.NewGuid() };

            // Act
            var result = await _repository.AddPostAsync(newPost);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newPost.Content, result.Content);
            Assert.NotEqual(Guid.Empty, result.PostId);

            _mockPostSet.Verify(m => m.Add(It.Is<Post>(p => p == newPost)), Times.Once());
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.Contains(newPost, _postList);
        }


        [Fact]
        public async Task UpdatePostAsync_ShouldUpdatePostAndReturnIt()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var originalPost = CreateTestPost(postId, Guid.NewGuid(), "User", "Original", DateTime.UtcNow);
            var updatedPostData = new Post { PostId = postId, Content = "Updated", UserId = originalPost.UserId /* Keep required fields */, GroupId = originalPost.GroupId }; // Create a detached object with updates

            // Mock the Update method on DbSet
            _mockPostSet.Setup(m => m.Update(It.Is<Post>(p => p.PostId == postId)))
                      .Callback<Post>(p => {
                          // Simulate update behavior on the in-memory list
                          var existing = _postList.First(x => x.PostId == p.PostId);
                          existing.Content = p.Content; // Update relevant property
                          existing.UpdatedAt = DateTime.UtcNow;
                      });

            // Act
            var result = await _repository.UpdatePostAsync(updatedPostData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(postId, result.PostId);
            Assert.Equal("Updated", result.Content); // Check if content was updated in returned obj

            _mockPostSet.Verify(m => m.Update(It.Is<Post>(p => p.PostId == postId && p.Content == "Updated")), Times.Once());
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());

            // Verify the change in the backing list
            var postInList = _postList.FirstOrDefault(p => p.PostId == postId);
            Assert.NotNull(postInList);
            Assert.Equal("Updated", postInList.Content);
        }

        [Fact]
        public async Task DeletePostAsync_ShouldRemovePostAndReturnTrue_WhenExists()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var postToDelete = CreateTestPost(postId, Guid.NewGuid(), "User", "Delete Me", DateTime.UtcNow);


            // Act
            var result = await _repository.DeletePostAsync(postId);

            // Assert
            Assert.True(result);
            _mockPostSet.Verify(m => m.FindAsync(It.Is<object[]>(ids => (Guid)ids[0] == postId)), Times.Once());
            _mockPostSet.Verify(m => m.Remove(It.Is<Post>(p => p.PostId == postId)), Times.Once());
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.DoesNotContain(_postList, p => p.PostId == postId);
        }

        [Fact]
        public async Task DeletePostAsync_ShouldReturnFalse_WhenNotExists()
        {
            // Arrange
            var postId = Guid.NewGuid();
            _mockPostSet.Setup(m => m.FindAsync(It.Is<object[]>(ids => (Guid)ids[0] == postId))).ReturnsAsync((Post?)null);


            // Act
            var result = await _repository.DeletePostAsync(postId);

            // Assert
            Assert.False(result);
            _mockPostSet.Verify(m => m.FindAsync(It.Is<object[]>(ids => (Guid)ids[0] == postId)), Times.Once());
            _mockPostSet.Verify(m => m.Remove(It.IsAny<Post>()), Times.Never());
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
        }

        [Fact]
        public async Task GetPostLikeCountAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            var postId1 = Guid.NewGuid();
            var postId2 = Guid.NewGuid();
            CreateTestPost(postId1, Guid.NewGuid(), "User", "Post 1", DateTime.UtcNow, likeCount: 3);
            CreateTestPost(postId2, Guid.NewGuid(), "User", "Post 2", DateTime.UtcNow, likeCount: 1);

            // Act
            var count1 = await _repository.GetPostLikeCountAsync(postId1);
            var count2 = await _repository.GetPostLikeCountAsync(postId2);
            var count3 = await _repository.GetPostLikeCountAsync(Guid.NewGuid()); // Non-existent post

            // Assert
            Assert.Equal(3, count1);
            Assert.Equal(1, count2);
            Assert.Equal(0, count3);
        }

        [Fact]
        public async Task GetPostCommentCountAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            var postId1 = Guid.NewGuid();
            var postId2 = Guid.NewGuid();
            CreateTestPost(postId1, Guid.NewGuid(), "User", "Post 1", DateTime.UtcNow, commentCount: 2);
            CreateTestPost(postId2, Guid.NewGuid(), "User", "Post 2", DateTime.UtcNow, commentCount: 0);

            // Act
            var count1 = await _repository.GetPostCommentCountAsync(postId1);
            var count2 = await _repository.GetPostCommentCountAsync(postId2);
            var count3 = await _repository.GetPostCommentCountAsync(Guid.NewGuid()); // Non-existent post

            // Assert
            Assert.Equal(2, count1);
            Assert.Equal(0, count2);
            Assert.Equal(0, count3);
        }

        [Fact]
        public async Task GetPostAttachmentsAsync_ShouldReturnAttachmentsForPost()
        {
            // Arrange
            var postId1 = Guid.NewGuid();
            var postId2 = Guid.NewGuid();
            CreateTestPost(postId1, Guid.NewGuid(), "User", "Post 1", DateTime.UtcNow, attachmentCount: 2);
            CreateTestPost(postId2, Guid.NewGuid(), "User", "Post 2", DateTime.UtcNow, attachmentCount: 1);

            // Act
            var attachments1 = await _repository.GetPostAttachmentsAsync(postId1);
            var attachments2 = await _repository.GetPostAttachmentsAsync(postId2);
            var attachments3 = await _repository.GetPostAttachmentsAsync(Guid.NewGuid()); // Non-existent post


            // Assert
            Assert.NotNull(attachments1);
            Assert.Equal(2, attachments1.Count);
            Assert.StartsWith("file", attachments1[0].FileName); // Basic check

            Assert.NotNull(attachments2);
            Assert.Single(attachments2);

            Assert.NotNull(attachments3);
            Assert.Empty(attachments3);
        }
    }
}