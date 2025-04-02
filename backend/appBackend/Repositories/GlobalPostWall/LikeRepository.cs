using appBackend.Interfaces.GlobalPostWall;
using appBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace appBackend.Repositories.GlobalPostWall
{
    public class LikeRepository : ILikeRepository
    {
        private readonly SocialMediaDbContext _dbContext;

        public LikeRepository(SocialMediaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Like?> GetLikeAsync(Guid postId, Guid userId)
        {
            return await _dbContext.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
        }

        public async Task<Like> AddLikeAsync(Like like)
        {
            _dbContext.Likes.Add(like);
            await _dbContext.SaveChangesAsync();
            return like;
        }

        public async Task<bool> DeleteLikeAsync(Guid postId, Guid userId)
        {
            var likeToRemove = await _dbContext.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (likeToRemove == null) return false;

            _dbContext.Likes.Remove(likeToRemove);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
