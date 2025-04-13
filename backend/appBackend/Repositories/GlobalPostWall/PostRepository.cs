using appBackend.Interfaces.GlobalPostWall;
using appBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace appBackend.Repositories.GlobalPostWall
{
    public class PostRepository : IPostRepository
    {
        private readonly SocialMediaDbContext _dbContext;

        public PostRepository(SocialMediaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Post>> GetAllPostsAsync()
        {
            return await _dbContext.Posts
                .Include(p => p.Author)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Include(p => p.Attachments)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
        public async Task<(List<Post> Posts, int TotalCount)> GetPostsPagedAsync(int pageNumber, int pageSize, Guid currentUserId, Guid? groupId = null)
        {
            IQueryable<Post> query = _dbContext.Posts
                 .Include(p => p.Author)
                 .Include(p => p.Likes)
                 .Include(p => p.Comments)
                 .Include(p => p.Attachments);

            if (groupId.HasValue) // Filter by groupId if provided
            {
                query = query.Where(p => p.GroupId == groupId.Value);
            }

            var totalCount = await query.CountAsync(); // Count based on filtered query

            var posts = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (Posts: posts, TotalCount: totalCount);
        }
        public async Task<Post?> GetPostByIdAsync(Guid postId)
        {
            return await _dbContext.Posts
                .Include(p => p.Author)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Include(p => p.Attachments)
                .FirstOrDefaultAsync(p => p.PostId == postId);
        }

        public async Task<Post> AddPostAsync(Post post)
        {
            _dbContext.Posts.Add(post);
            await _dbContext.SaveChangesAsync();
            return post;
        }

        public async Task<Post> UpdatePostAsync(Post post)
        {
            _dbContext.Posts.Update(post);
            await _dbContext.SaveChangesAsync();
            return post;
        }

        public async Task<bool> DeletePostAsync(Guid postId)
        {
            var post = await _dbContext.Posts.FindAsync(postId);
            if (post == null) return false;

            _dbContext.Posts.Remove(post);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetPostLikeCountAsync(Guid postId)
        {
            return await _dbContext.Likes
                .CountAsync(l => l.PostId == postId);
        }

        public async Task<int> GetPostCommentCountAsync(Guid postId)
        {
            return await _dbContext.Comments
                .CountAsync(c => c.PostId == postId);
        }

        public async Task<List<Attachment>> GetPostAttachmentsAsync(Guid postId)
        {
            return await _dbContext.Attachments
                .Where(a => a.PostId == postId)
                .ToListAsync();
        }
    }
}
