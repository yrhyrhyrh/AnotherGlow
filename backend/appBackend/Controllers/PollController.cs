using appBackend.DTOs; // For VoteRequest
using appBackend.Models; // For Poll
using appBackend.Services; // For IPollService
using Microsoft.AspNetCore.Authorization; // For [Authorize]
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims; // For ClaimTypes

namespace appBackend.Controllers // Assuming Controllers namespace
{
    [Route("api/polls")]
    [ApiController]
    [Authorize] // Require authentication for all actions in this controller
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
            Console.WriteLine("PollController - CreatePoll Request Received");

            // Extract userId securely from the authenticated user's claims
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
            {
                Console.WriteLine("Unauthorized: User ID claim (NameIdentifier) is missing.");
                return Unauthorized("User ID claim is missing."); // 401 Unauthorized
            }

            if (!Guid.TryParse(userIdClaim.Value, out var userId))
            {
                Console.WriteLine($"BadRequest: User ID claim value '{userIdClaim.Value}' is not a valid GUID.");
                return BadRequest("User ID claim is not a valid GUID."); // 400 Bad Request
            }

            // Set the UserId from the claim, ignore client value
            poll.UserId = userId;
            // Let service/repo handle PollId generation and CreatedAt
            poll.PollId = Guid.Empty; // Clear any client-sent ID
            poll.CreatedAt = DateTime.UtcNow; // Set server-side timestamp

            try
            {
                _pollService.CreatePoll(poll); // Service handles validation and saving
                Console.WriteLine($"Poll created successfully by User {userId}.");
                // Return 201 Created with the location and optionally the created object
                return CreatedAtAction(nameof(GetPollById), new { pollId = poll.PollId }, poll);
            }
            catch (ArgumentException ex) // Catch validation errors from service
            {
                Console.WriteLine($"BadRequest creating poll: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex) // Catch unexpected service/repo errors
            {
                Console.WriteLine($"InternalServerError creating poll: {ex.Message}");
                // Log the full exception (ex)
                return StatusCode(500, "An error occurred while creating the poll.");
            }
        }

        // Optional: Get a single poll by ID
        [HttpGet("{pollId:guid}", Name = "GetPollById")]
        [AllowAnonymous] // Or keep Authorize depending on requirements
        public IActionResult GetPollById(Guid pollId)
        {
            Console.WriteLine($"PollController - GetPollById Request Received for Poll {pollId}");
            var poll = _pollService.GetPollById(pollId);
            if (poll == null)
            {
                Console.WriteLine($"NotFound: Poll {pollId} not found.");
                return NotFound(); // 404 Not Found
            }
            return Ok(poll);
        }

        [HttpGet("all")]
        [AllowAnonymous] // Allow anyone to view polls
        public IActionResult GetAllPolls()
        {
            Console.WriteLine("PollController - GetAllPolls Request Received");
            try
            {
                var polls = _pollService.GetAllPolls();
                Console.WriteLine($"Retrieved {polls?.Count() ?? 0} polls.");
                return Ok(polls ?? Enumerable.Empty<Poll>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"InternalServerError getting polls: {ex.Message}");
                // Log the full exception (ex)
                return StatusCode(500, "An error occurred while retrieving polls.");
            }
        }

        [HttpPost("{pollId:guid}/vote")]
        public IActionResult Vote(Guid pollId, [FromBody] VoteRequest request)
        {
            Console.WriteLine($"PollController - Vote Request Received for Poll {pollId}");

            // Extract userId securely from claims
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var authenticatedUserId))
            {
                Console.WriteLine("Unauthorized: User ID claim (NameIdentifier) missing or invalid.");
                return Unauthorized("User ID claim missing or invalid."); // 401 Unauthorized
            }

            // Verify the UserId in the request matches the authenticated user from token
            if (request == null || request.UserId != authenticatedUserId)
            {
                Console.WriteLine($"Forbidden: Authenticated user {authenticatedUserId} cannot vote as user {request?.UserId}.");
                return Forbid(); // 403 Forbidden - More appropriate than BadRequest
            }

            // Basic validation already in controller
            if (!request.Retract && request.OptionIndex == null && (request.OptionIndices == null || !request.OptionIndices.Any()))
            {
                return BadRequest("Voting option(s) must be provided when not retracting.");
            }

            try
            {
                // Pass the necessary identifiers and DTO to the service
                if (request.Retract)
                {
                    _pollService.RetractVote(pollId, authenticatedUserId); // Pass authenticated user ID
                    Console.WriteLine($"Vote retracted successfully for Poll {pollId} by User {authenticatedUserId}.");
                    return Ok(new { message = "Vote retracted successfully!" });
                }
                else
                {
                    _pollService.CastVote(pollId, request); // Pass the whole request DTO
                    Console.WriteLine($"Vote cast successfully for Poll {pollId} by User {authenticatedUserId}.");
                    return Ok(new { message = "Vote cast successfully!" });
                }
            }
            catch (ArgumentException ex) // Specific errors from service (e.g., poll not found, invalid option)
            {
                Console.WriteLine($"BadRequest processing vote: {ex.Message}");
                return BadRequest(ex.Message); // 400 Bad Request
            }
            catch (InvalidOperationException ex) // Specific errors (e.g., vote not found for retraction)
            {
                Console.WriteLine($"Conflict processing vote: {ex.Message}");
                return Conflict(ex.Message); // 409 Conflict
            }
            catch (NotImplementedException ex) // RetractVote not implemented
            {
                Console.WriteLine($"NotImplemented: {ex.Message}");
                return StatusCode(501, "Retract vote functionality is not available."); // 501 Not Implemented
            }
            catch (Exception ex) // Catch unexpected server errors
            {
                Console.WriteLine($"InternalServerError processing vote: {ex.Message}");
                // Log the full exception (ex)
                return StatusCode(500, "An unexpected error occurred while processing your vote."); // 500 Internal Server Error
            }
        }
    }
}