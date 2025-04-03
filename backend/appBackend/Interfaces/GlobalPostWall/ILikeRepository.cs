using appBackend.Models;

namespace appBackend.Interfaces.GlobalPostWall
{
    public interface ILikeRepository
    {
        Task<Like?> GetLikeAsync(Guid postId, Guid userId);
        Task<Like> AddLikeAsync(Like like);
        Task<bool> DeleteLikeAsync(Guid postId, Guid userId);
    }
}
