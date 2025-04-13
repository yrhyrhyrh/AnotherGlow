using appBackend.Interfaces.GlobalPostWall;
using appBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace appBackend.Repositories.GlobalPostWall
{
    public class CommentRepository : ICommentRepository
    {
        private readonly SocialMediaDbContext _dbContext;

        public CommentRepository(SocialMediaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Comment?> GetCommentByIdAsync(Guid commentId)
        {
            return await _dbContext.Comments.FindAsync(commentId);
        }

        public async Task<Comment> AddCommentAsync(Comment comment)
        {
            _dbContext.Comments.Add(comment);
            await _dbContext.SaveChangesAsync();
            return comment;
        }

        public async Task<bool> DeleteCommentAsync(Guid commentId)
        {
            var comment = await _dbContext.Comments.FindAsync(commentId);
            if (comment == null) return false;

            _dbContext.Comments.Remove(comment);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        public async Task<List<Comment>> GetCommentsByPostIdAsync(Guid postId)
        {
            return await _dbContext.Comments
                .Include(c => c.User) // Eager load User for Author details
                .Where(c => c.PostId == postId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}
