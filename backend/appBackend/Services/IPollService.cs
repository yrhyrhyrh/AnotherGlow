using appBackend.Dtos.GlobalPostWall;
using appBackend.DTOs;
using appBackend.Models;

public interface IPollService
{
    Task CastVoteAsync(Guid pollId, VoteRequest voteRequest);  // Change signature to match the service
    Task RetractVoteAsync(Guid pollId, Guid userId);
    Task CreatePollAsync(CreatePollDTO poll);
    Task<IEnumerable<Poll>> GetAllPollsAsync();
    Task<Poll?> GetPollByIdAsync(Guid pollId);
}