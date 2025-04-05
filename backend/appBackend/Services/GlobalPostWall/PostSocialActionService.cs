using appBackend.Dtos.GlobalPostWall;
using appBackend.Interfaces.GlobalPostWall;
using appBackend.Models;
using appBackend.Repositories;

namespace appBackend.Services.GlobalPostWall
{
    public class PostSocialActionsService : IPostSocialActionsService
    {
        private readonly ILikeRepository _likeRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly SocialMediaDbContext _dbContext; // Still need DbContext for User lookup

        public PostSocialActionsService(ILikeRepository likeRepository, ICommentRepository commentRepository, SocialMediaDbContext dbContext)
        {
            _likeRepository = likeRepository;
            _commentRepository = commentRepository;
            _dbContext = dbContext; // Keep DbContext for now for User lookup
        }

        public async Task<LikeDTO?> LikePostAsync(Guid postId, Guid userId)
        {
            var post = await _dbContext.Posts.FindAsync(postId); // Still using DbContext for Post lookup (or create PostRepository if needed more)
            var user = await _dbContext.Users.FindAsync(userId);

            if (post == null || user == null) return null;

            var existingLike = await _likeRepository.GetLikeAsync(postId, userId);

            if (existingLike != null) return null;

            var like = new Like { PostId = postId, UserId = userId };
            var createdLike = await _likeRepository.AddLikeAsync(like);

            return new LikeDTO
            {
                LikeId = createdLike.LikeId,
                PostId = createdLike.PostId,
                UserId = createdLike.UserId,
                CreatedAt = createdLike.CreatedAt
            };
        }

        public async Task<bool> UnlikePostAsync(Guid postId, Guid userId)
        {
            return await _likeRepository.DeleteLikeAsync(postId, userId);
        }

        public async Task<CommentDTO> AddCommentToPostAsync(Guid postId, Guid userId, CreateCommentRequestDTO createCommentDto)
        {
            var post = await _dbContext.Posts.FindAsync(postId); // Still using DbContext for Post lookup
            var user = await _dbContext.Users.FindAsync(userId);

            if (post == null || user == null) throw new KeyNotFoundException("Post or User not found.");

            var comment = new Comment
            {
                PostId = postId,
                UserId = userId,
                Content = createCommentDto.Content
            };

            var createdComment = await _commentRepository.AddCommentAsync(comment);

            return new CommentDTO
            {
                CommentId = createdComment.CommentId,
                UserId = createdComment.UserId,
                AuthorUsername = user.Username, // You might need to fetch user details if not readily available - consider UserRepository later
                AuthorFullName = user.FullName,
                Content = createdComment.Content,
                CreatedAt = createdComment.CreatedAt
            };
        }

        public async Task<bool> DeleteCommentAsync(Guid commentId, Guid userId)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(commentId);
            if (comment == null) return false;

            if (comment.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this comment.");
            }

            return await _commentRepository.DeleteCommentAsync(commentId);
        }
    }
    public class PostSocialActionService
    {
    }
}
