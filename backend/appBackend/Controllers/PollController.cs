using appBackend.DTOs;
using appBackend.Models;
using Microsoft.AspNetCore.Mvc;

[Route("api/polls")]
[ApiController]
public class PollController : ControllerBase
{
    private readonly IPollService _pollService;

    public PollController(IPollService pollService)
    {
        _pollService = pollService;
    }

    [HttpPost("create")]
    public IActionResult CreatePoll([FromBody] Poll poll)
    {
        Console.WriteLine("PollController - CreatePoll");
        Console.WriteLine("Poll Question: " + poll.Question);
        Console.WriteLine("Available Claims:");
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"{claim.Type}: {claim.Value}");
        }

        var userIdClaim = User.FindFirst("user_id");
        if (userIdClaim != null)
        {
            poll.UserId = Guid.Parse(userIdClaim.Value);
        }
        else
        {
            Console.WriteLine("No user ID found in claims.");
            return BadRequest("User ID not found.");
        }
    
        _pollService.CreatePoll(poll);
        return Ok(new { message = "Poll created successfully!" });
    }


    [HttpGet("all")]
    public IActionResult GetAllPolls()
    {
        Console.WriteLine("PollController - GetAllPolls");
        return Ok(_pollService.GetAllPolls());
    }

    [HttpPost("{pollId:guid}/vote")]
    public IActionResult Vote(Guid pollId, [FromBody] VoteRequest request)
    {
        Console.WriteLine("PollController - Vote");
        Console.WriteLine("pollId: " + pollId);
        Console.WriteLine("VoteRequest - userId: " + request.UserId);
        Console.WriteLine("VoteRequest - optionIndex: " + request.OptionIndex);
        Console.WriteLine("VoteRequest - retract: " + request.Retract);

        try
        {
            if (request.Retract)
            {
                _pollService.RetractVote(pollId, request.UserId);
                return Ok(new { message = "Vote retracted successfully!" });
            }
            else
            {
                _pollService.CastVote(pollId, request.UserId, request.OptionIndex);
                return Ok(new { message = "Vote cast successfully!" });
            }
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine("ArgumentException: " + ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: " + ex.Message);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }
}
