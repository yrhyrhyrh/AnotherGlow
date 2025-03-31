using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

[Route("api/polls")]
[ApiController]
public class PollController : ControllerBase
{
    private readonly IPollService _pollService;

    public PollController(IPollService pollService) // Inject IPollService, not PollService
    {
        _pollService = pollService;
    }


    [HttpPost("create")]
    public IActionResult CreatePoll([FromBody] Poll poll)
    {
        _pollService.CreatePoll(poll);
        return Ok(new { message = "Poll created successfully!" });
    }

    [HttpGet("all")]
    public IActionResult GetAllPolls() => Ok(_pollService.GetAllPolls());

    [HttpPost("{pollId}/vote")] // Route includes pollId
    public IActionResult Vote(int pollId, [FromBody] int optionIndex)
    {
        try
        {
            _pollService.CastVote(pollId, optionIndex);
            return Ok(new { message = "Vote cast successfully!" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message); // Handle invalid pollId or optionIndex
        }
        catch (SqlException ex)
        {
            return StatusCode(500, "Database error occurred.");
        }
        catch (Exception ex)
        {
            // Log exception (optional for better debugging)
            return StatusCode(500, "An unexpected error occurred.");
        }
    }
}