using Amazon.S3;
using Amazon.S3.Model;
using appBackend.Dtos.GlobalPostWall;
using appBackend.Interfaces.GlobalPostWall;
using appBackend.Models;
using appBackend.Models.PostComponents;
using appBackend.Repositories;
using System.Text.Json;

namespace appBackend.Services.GlobalPostWall
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly IPollService _pollService;
        private readonly SocialMediaDbContext _dbContext;
        private readonly IAmazonS3 _s3Client; // Inject IAmazonS3

        // Get S3 Bucket Name from Configuration
        private readonly string _s3BucketName;
        private readonly string _cloudformS3domain;

        public PostService(IPostRepository postRepository, IAttachmentRepository attachmentRepository, IPollService pollService, SocialMediaDbContext dbContext, IAmazonS3 s3Client, IConfiguration configuration) // Inject IConfiguration
        {
            _postRepository = postRepository;
            _attachmentRepository = attachmentRepository;
            _pollService = pollService;
            _dbContext = dbContext;
            _s3Client = s3Client;
            _s3BucketName = configuration["Aws:S3BucketName"] ?? throw new InvalidOperationException("S3BucketName is not configured."); // Read from config
            _cloudformS3domain = configuration["Aws:CfDistributionDomainName"] ?? throw new InvalidOperationException("CfDistributionDomainName is not configured."); // Read from config
        }

        public async Task<List<PostDTO>> GetGlobalPostsAsync(Guid currentUserId)
        {
            var posts = await _postRepository.GetAllPostsAsync();
            return posts.Select(p => MapPostToDTO(currentUserId, p)).ToList();
        }

        public async Task<(List<PostDTO> Posts, int TotalCount)> GetPostsPagedAsync(int pageNumber, int pageSize, Guid currentUserId, Guid? groupId = null)
        {
            var pagedResult = await _postRepository.GetPostsPagedAsync(pageNumber, pageSize, currentUserId, groupId);
            var compositePosts = pagedResult.Posts.Select(post => new CompositePost(post)).ToList();
            var postDTOs = compositePosts
                .Select(c => c.ToDTO(currentUserId))
                .OfType<PostDTO>()
                .ToList();

            return (Posts: postDTOs, TotalCount: pagedResult.TotalCount);
        }

        public async Task<PostDTO?> GetPostByIdAsync(Guid currentUserId, Guid postId)
        {

            var post = await _postRepository.GetPostByIdAsync(postId);
            return post == null ? null : MapPostToDTO(currentUserId, post);
        }

        public async Task<PostDTO> CreatePostAsync(Guid curentUserId, CreatePostRequestDTO createPostDto)
        {
            var user = await _dbContext.Users.FindAsync(curentUserId); // Still using DbContext for User lookup
            if (user == null) throw new KeyNotFoundException("User not found.");

            var post = new Post
            {
                PostId = Guid.NewGuid(),
                UserId = curentUserId,
                Content = createPostDto.Content,
                GroupId = createPostDto.GroupId
            };

            var createdPost = await _postRepository.AddPostAsync(post); // Use repository to add post

            // Handle Attachments
            if (createPostDto.Attachments != null && createPostDto.Attachments.Any())
            {
                await SaveAttachmentsAsync(createdPost.PostId, createPostDto.Attachments);
            }

            if (createPostDto.Poll != null)
            {
                CreatePollDTO? poll = JsonSerializer.Deserialize<CreatePollDTO>(createPostDto.Poll);
                poll.PostId = createdPost.PostId; // Set PostId for the poll
                await _pollService.CreatePollAsync(poll);
            }

            return MapPostToDTO(curentUserId, createdPost);
        }

        public async Task<PostDTO?> UpdatePostAsync(Guid postId, Guid currentUserId, UpdatePostRequestDTO updatePostDto)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null) return null;

            if (post.UserId != currentUserId)
            {
                throw new UnauthorizedAccessException("You are not authorized to update this post.");
            }

            if (!string.IsNullOrEmpty(updatePostDto.Content))
            {
                post.Content = updatePostDto.Content;
                post.UpdatedAt = DateTime.UtcNow;
            }

            var updatedPost = await _postRepository.UpdatePostAsync(post); // Use repository to update
            return MapPostToDTO(currentUserId, updatedPost);
        }


        public async Task<bool> DeletePostAsync(Guid postId, Guid currentUserId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null) return false;

            if (post.UserId != currentUserId)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this post.");
            }

            return await _postRepository.DeletePostAsync(postId); // Use repository to delete
        }


        private PostDTO MapPostToDTO(Guid currentUserId, Post post)
        {
            return new PostDTO
            {
                PostId = post.PostId,
                UserId = post.UserId,
                AuthorUsername = post.Author?.Username ?? "Unknown",
                AuthorFullName = post.Author?.FullName ?? "Unknown",
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                LikeCount = post.Likes?.Count ?? 0,
                CommentCount = post.Comments?.Count ?? 0,
                IsLikedByCurrentUser = post.Likes?.Any(like => like.UserId == currentUserId) ?? false,
                Attachments = post.Attachments?.Select(a => new AttachmentDTO
                {
                    AttachmentId = a.AttachmentId,
                    FileName = a.FileName,
                    FilePath = a.FilePath,
                    ContentType = a.ContentType,
                    FileSize = a.FileSize
                }).ToList() ?? new List<AttachmentDTO>(),
                Poll = post.Poll
            };
        }

        private async Task SaveAttachmentsAsync(Guid postId, List<IFormFile> attachments)
        {
            if (attachments == null || !attachments.Any()) return;

            foreach (var file in attachments)
            {
                if (file.Length > 0)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName; // Unique S3 key
                    string s3ObjectKey = $"attachments/{postId}/{uniqueFileName}"; // Organize files in S3

                    try
                    {
                        var putObjectRequest = new PutObjectRequest
                        {
                            BucketName = _s3BucketName,
                            Key = s3ObjectKey,
                            InputStream = file.OpenReadStream(), // Stream file content
                            ContentType = file.ContentType,     // Set Content-Type
                            AutoCloseStream = true // Ensure stream is closed
                        };

                        PutObjectResponse response = await _s3Client.PutObjectAsync(putObjectRequest);

                        if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                        {
                            string s3Url = $"https://{_cloudformS3domain}/{s3ObjectKey}";

                            var attachment = new Attachment
                            {
                                AttachmentId = Guid.NewGuid(),
                                PostId = postId,
                                FileName = file.FileName,
                                FilePath = s3Url, // Store S3 URL in FilePath (or create a new property FileUrl)
                                ContentType = file.ContentType,
                                FileSize = file.Length
                            };
                            await _attachmentRepository.AddAttachmentAsync(attachment);
                        }
                        else
                        {
                            Console.WriteLine($"Error uploading {file.FileName} to S3. Status Code: {response.HttpStatusCode}");
                        }
                    }
                    catch (AmazonS3Exception ex)
                    {
                        Console.WriteLine($"S3 Error uploading {file.FileName}: {ex.Message}");
                        // Handle S3 exception (e.g., retry, throw exception, return error result)
                        throw new Exception("Error uploading file to S3.", ex); // Re-throw or handle as needed
                    }
                }
            }
        }

    }
}
