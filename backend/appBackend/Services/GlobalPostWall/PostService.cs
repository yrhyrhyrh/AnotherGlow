using appBackend.Dtos.GlobalPostWall;
using appBackend.Interfaces.GlobalPostWall;
using appBackend.Models;
using appBackend.Repositories;

namespace appBackend.Services.GlobalPostWall
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly SocialMediaDbContext _dbContext; // Still need DbContext for User lookup and SaveChanges in attachment context
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PostService(IPostRepository postRepository, IAttachmentRepository attachmentRepository, SocialMediaDbContext dbContext, IWebHostEnvironment webHostEnvironment)
        {
            _postRepository = postRepository;
            _attachmentRepository = attachmentRepository;
            _dbContext = dbContext; // Keep DbContext for now for User lookup
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<List<PostDTO>> GetGlobalPostsAsync()
        {
            var posts = await _postRepository.GetAllPostsAsync();
            return posts.Select(p => MapPostToDTO(p)).ToList();
        }

        public async Task<PostDTO?> GetPostByIdAsync(Guid postId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            return post == null ? null : MapPostToDTO(post);
        }

        public async Task<PostDTO> CreatePostAsync(Guid userId, CreatePostRequestDTO createPostDto)
        {
            var user = await _dbContext.Users.FindAsync(userId); // Still using DbContext for User lookup
            if (user == null) throw new KeyNotFoundException("User not found.");

            var post = new Post
            {
                UserId = userId,
                Content = createPostDto.Content
            };

            var createdPost = await _postRepository.AddPostAsync(post); // Use repository to add post

            // Handle Attachments
            if (createPostDto.Attachments != null && createPostDto.Attachments.Any())
            {
                await SaveAttachmentsAsync(createdPost.PostId, createPostDto.Attachments);
            }

            return MapPostToDTO(createdPost);
        }

        public async Task<PostDTO?> UpdatePostAsync(Guid postId, Guid userId, UpdatePostRequestDTO updatePostDto)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null) return null;

            if (post.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to update this post.");
            }

            if (!string.IsNullOrEmpty(updatePostDto.Content))
            {
                post.Content = updatePostDto.Content;
                post.UpdatedAt = DateTime.UtcNow;
            }

            var updatedPost = await _postRepository.UpdatePostAsync(post); // Use repository to update
            return MapPostToDTO(updatedPost);
        }


        public async Task<bool> DeletePostAsync(Guid postId, Guid userId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null) return false;

            if (post.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this post.");
            }

            return await _postRepository.DeletePostAsync(postId); // Use repository to delete
        }


        private PostDTO MapPostToDTO(Post post)
        {
            return new PostDTO
            {
                PostId = post.PostId,
                UserId = post.UserId,
                AuthorUsername = post.Author?.Username ?? "Unknown",
                AuthorFullName = post.Author?.FullName ?? "Unknown",
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                LikeCount = post.Likes?.Count ?? 0, // Might be better to use repository for counts in real-world, but for now ok.
                CommentCount = post.Comments?.Count ?? 0, // Same here
                Attachments = post.Attachments?.Select(a => new AttachmentDTO
                {
                    AttachmentId = a.AttachmentId,
                    FileName = a.FileName,
                    FilePath = a.FilePath,
                    ContentType = a.ContentType,
                    FileSize = a.FileSize
                }).ToList() ?? new List<AttachmentDTO>()
            };
        }

        private async Task SaveAttachmentsAsync(Guid postId, List<IFormFile> attachments)
        {
            if (attachments == null || !attachments.Any()) return;

            string uploadFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "attachments");
            Directory.CreateDirectory(uploadFolderPath);

            foreach (var file in attachments)
            {
                if (file.Length > 0)
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    string filePath = Path.Combine(uploadFolderPath, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    var attachment = new Attachment
                    {
                        PostId = postId,
                        FileName = file.FileName,
                        FilePath = Path.Combine("uploads", "attachments", uniqueFileName),
                        ContentType = file.ContentType,
                        FileSize = file.Length
                    };
                    await _attachmentRepository.AddAttachmentAsync(attachment); // Use attachment repository
                }
            }
        }
    }
}
