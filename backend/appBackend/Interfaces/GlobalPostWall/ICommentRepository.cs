using appBackend.Models;

namespace appBackend.Interfaces.GlobalPostWall
{
    public interface ICommentRepository
    {
        Task<Comment?> GetCommentByIdAsync(Guid commentId);
        Task<Comment> AddCommentAsync(Comment comment);
        Task<bool> DeleteCommentAsync(Guid commentId);
        Task<List<Comment>> GetCommentsByPostIdAsync(Guid postId); // New method to fetch comments by PostId
    }
}
