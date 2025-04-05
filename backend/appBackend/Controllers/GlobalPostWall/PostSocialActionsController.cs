using appBackend.Dtos.GlobalPostWall;
using appBackend.Interfaces.GlobalPostWall;
using Microsoft.AspNetCore.Mvc;

namespace appBackend.Controllers.GlobalPostWall
{
    [ApiController]
    [Route("api/social-actions")]
    public class SocialActionsController : ControllerBase
    {
        private readonly IPostSocialActionsService _socialActionsService;

        public SocialActionsController(IPostSocialActionsService socialActionsService)
        {
            _socialActionsService = socialActionsService;
        }

        [HttpPost("posts/{postId}/like")]
        public async Task<IActionResult> LikePost(Guid postId)
        {
            // **Important: Replace with actual user authentication to get current UserId**
            Guid currentUserId = Guid.Parse("YOUR_USER_ID_HERE"); // Replace placeholder

            var likeDto = await _socialActionsService.LikePostAsync(postId, currentUserId);
            if (likeDto == null) return BadRequest("Post not found or already liked."); // Or return 409 Conflict for already liked
            return CreatedAtAction(nameof(GetLike), new { likeId = likeDto.LikeId }, likeDto); // Or just Ok(likeDto)

        }

        [HttpGet("likes/{likeId}", Name = "GetLike")] // To use CreatedAtAction for LikePost
        public IActionResult GetLike(Guid likeId) // Dummy action just for CreatedAtAction
        {
            return Ok(); // You might not need to actually retrieve a single like, just return Ok
        }


        [HttpPost("posts/{postId}/unlike")]
        public async Task<IActionResult> UnlikePost(Guid postId)
        {
            // **Important: Replace with actual user authentication to get current UserId**
            Guid currentUserId = Guid.Parse("YOUR_USER_ID_HERE"); // Replace placeholder

            bool unliked = await _socialActionsService.UnlikePostAsync(postId, currentUserId);
            if (!unliked) return NotFound("Like not found or post not found.");
            return NoContent(); // 204 No Content - successful unlike
        }

        [HttpPost("posts/{postId}/comments")]
        public async Task<IActionResult> AddCommentToPost(Guid postId, [FromBody] CreateCommentRequestDTO createCommentDto)
        {
            // **Important: Replace with actual user authentication to get current UserId**
            Guid currentUserId = Guid.Parse("YOUR_USER_ID_HERE"); // Replace placeholder

            try
            {
                var commentDto = await _socialActionsService.AddCommentToPostAsync(postId, currentUserId, createCommentDto);
                return CreatedAtAction(nameof(GetComment), new { commentId = commentDto.CommentId }, commentDto); // Or just Ok(commentDto)
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Post or User not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }

        [HttpGet("comments/{commentId}", Name = "GetComment")] // For CreatedAtAction in AddCommentToPost
        public IActionResult GetComment(Guid commentId) // Dummy action for CreatedAtAction
        {
            return Ok(); // You might not need to retrieve single comment, just return Ok
        }


        [HttpDelete("comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(Guid commentId)
        {
            // **Important: Replace with actual user authentication to get current UserId**
            Guid currentUserId = Guid.Parse("YOUR_USER_ID_HERE"); // Replace placeholder

            try
            {
                bool deleted = await _socialActionsService.DeleteCommentAsync(commentId, currentUserId);
                if (!deleted) return NotFound("Comment not found or unauthorized.");
                return NoContent(); // 204 No Content
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message); // 403 Forbidden
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }
    }
}
