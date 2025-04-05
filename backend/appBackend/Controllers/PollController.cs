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

        // Extract userId from the authenticated user's claims
        var userIdClaim = User.FindFirst("userId");
        if (userIdClaim == null)
        {
            Console.WriteLine("Invalid request: UserId is missing.");
            return Unauthorized("User ID claim is missing.");
        }

        if (!Guid.TryParse(userIdClaim.Value, out var userId))
        {
            Console.WriteLine("Invalid request: UserId claim is not a valid GUID.");
            return BadRequest("User ID must be a valid GUID.");
        }

        // Set the UserId on the poll object
        poll.UserId = userId;

        _pollService.CreatePoll(poll);
        Console.WriteLine("Poll created successfully.");
        return Ok(new { message = "Poll created successfully!" });
    }

    [HttpGet("all")]
    public IActionResult GetAllPolls()
    {
        Console.WriteLine("PollController - GetAllPolls");
        var polls = _pollService.GetAllPolls();
        Console.WriteLine($"Retrieved {polls.Count()} polls.");
        return Ok(polls);
    }

    [HttpPost("{pollId:guid}/vote")]
    public IActionResult Vote(Guid pollId, [FromBody] VoteRequest request)
    {
        Console.WriteLine("PollController - Vote");
        Console.WriteLine("pollId: " + pollId);
        Console.WriteLine("VoteRequest - userId: " + request.UserId);
        Console.WriteLine("VoteRequest - optionIndex: " + request.OptionIndex);
        Console.WriteLine("VoteRequest - retract: " + request.Retract);

        if (request == null || request.UserId == Guid.Empty)
        {
            Console.WriteLine("Invalid request: UserId is missing.");
            return BadRequest("Invalid request. User ID must be provided.");
        }

        try
        {
            if (request.Retract)
            {
                _pollService.RetractVote(pollId, request.UserId);
                Console.WriteLine("Vote retracted successfully.");
                return Ok(new { message = "Vote retracted successfully!" });
            }
            else
            {
                _pollService.CastVote(pollId, request.UserId, request.OptionIndex);
                Console.WriteLine("Vote cast successfully.");
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
