using appBackend.Dtos.GlobalPostWall;

namespace appBackend.Interfaces.GlobalPostWall
{
    public interface IPostSocialActionsService
    {
        Task<LikeDTO?> LikePostAsync(Guid postId, Guid userId);
        Task<bool> UnlikePostAsync(Guid postId, Guid userId);
        Task<CommentDTO> AddCommentToPostAsync(Guid postId, Guid userId, CreateCommentRequestDTO createCommentDto);
        Task<bool> DeleteCommentAsync(Guid commentId, Guid userId); // Author or Admin can delete
    }
}
