using appBackend.Models;

namespace appBackend.Interfaces.GlobalPostWall
{
    public interface IPostRepository
    {
        Task<List<Post>> GetAllPostsAsync();
        Task<(List<Post> Posts, int TotalCount)> GetPostsPagedAsync(int pageNumber, int pageSize, Guid currentUserId, Guid? groupId = null);
        Task<Post?> GetPostByIdAsync(Guid postId);
        Task<Post> AddPostAsync(Post post);
        Task<Post> UpdatePostAsync(Post post);
        Task<bool> DeletePostAsync(Guid postId);
        Task<int> GetPostLikeCountAsync(Guid postId);
        Task<int> GetPostCommentCountAsync(Guid postId);
        Task<List<Attachment>> GetPostAttachmentsAsync(Guid postId);
    }
}
