using Microsoft.AspNetCore.Mvc;

[Route("api/polls")]
[ApiController]
public class PollController : ControllerBase
{

    private readonly IPollService _pollService;

    public PollController(IPollService pollService)
    {
        _pollService = pollService ?? throw new ArgumentNullException(nameof(pollService));
    }

    [HttpPost("create")]
    public IActionResult CreatePoll([FromBody] Poll poll)
    {
        _pollService.CreatePoll(poll);
        return Ok(new { message = "Poll created successfully!" });
    }

    [HttpGet("all")]
    public IActionResult GetAllPolls() => Ok(_pollService.GetAllPolls());
}
