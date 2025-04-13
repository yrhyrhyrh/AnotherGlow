using appBackend.Dtos.GlobalPostWall;

namespace appBackend.Interfaces.GlobalPostWall
{
    public interface IPostService
    {
        Task<List<PostDTO>> GetGlobalPostsAsync(Guid userId);
        Task<(List<PostDTO> Posts, int TotalCount)> GetPostsPagedAsync(int pageNumber, int pageSize, Guid currentUserId, Guid? groupId = null); // New paged service method
        Task<PostDTO?> GetPostByIdAsync(Guid userId, Guid postId);
        Task<PostDTO> CreatePostAsync(Guid userId, CreatePostRequestDTO createPostDto);
        Task<PostDTO?> UpdatePostAsync(Guid postId, Guid userId, UpdatePostRequestDTO updatePostDto);
        Task<bool> DeletePostAsync(Guid postId, Guid userId);
    }
}
