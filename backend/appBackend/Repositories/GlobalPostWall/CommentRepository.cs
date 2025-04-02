using appBackend.Interfaces.GlobalPostWall;
using appBackend.Models;

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
    }
}
