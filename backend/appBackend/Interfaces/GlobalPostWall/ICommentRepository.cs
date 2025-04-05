using appBackend.Models;

namespace appBackend.Interfaces.GlobalPostWall
{
    public interface ICommentRepository
    {
        Task<Comment?> GetCommentByIdAsync(Guid commentId);
        Task<Comment> AddCommentAsync(Comment comment);
        Task<bool> DeleteCommentAsync(Guid commentId);
    }
}
