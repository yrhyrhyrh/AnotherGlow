using appBackend.Dtos.GlobalPostWall;
using appBackend.Interfaces.GlobalPostWall;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace appBackend.Controllers.GlobalPostWall
{
    [Authorize]
    [ApiController]
    [Route("api/posts")]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet]
        public async Task<IActionResult> GetGlobalPosts()
        {
            var posts = await _postService.GetGlobalPostsAsync();
            return Ok(posts);
        }

        [HttpGet("{postId}")]
        public async Task<IActionResult> GetPostById(Guid postId)
        {
            var post = await _postService.GetPostByIdAsync(postId);
            if (post == null) return NotFound();
            return Ok(post);
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 209715200)] // 200MB limit for attachments
        [DisableRequestSizeLimit] // or configure in web.config/appsettings.json
        public async Task<IActionResult> CreatePost([FromForm] CreatePostRequestDTO createPostDto)
        {
            Guid currentUserId = Guid.Parse(User.FindFirst("userId")!.Value);

            try
            {
                var createdPost = await _postService.CreatePostAsync(currentUserId, createPostDto);
                return CreatedAtAction(nameof(GetPostById), new { postId = createdPost.PostId }, createdPost);
            }
            catch (KeyNotFoundException)
            {
                return BadRequest("User not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error: " + ex.Message); // Log exception in real app
            }
        }

        [HttpPut("{postId}")]
        public async Task<IActionResult> UpdatePost(Guid postId, [FromBody] UpdatePostRequestDTO updatePostDto)
        {
            Guid currentUserId = Guid.Parse(User.FindFirst("userId")!.Value);


            try
            {
                var updatedPost = await _postService.UpdatePostAsync(postId, currentUserId, updatePostDto);
                if (updatedPost == null) return NotFound();
                return Ok(updatedPost);
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

        [HttpDelete("{postId}")]
        public async Task<IActionResult> DeletePost(Guid postId)
        {
            Guid currentUserId = Guid.Parse(User.FindFirst("userId")!.Value);


            try
            {
                bool deleted = await _postService.DeletePostAsync(postId, currentUserId);
                if (!deleted) return NotFound();
                return NoContent(); // 204 No Content - successful deletion
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
