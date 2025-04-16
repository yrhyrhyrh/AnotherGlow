using appBackend.DTOs;
using appBackend.Models;

public interface IPollService
{
    void CastVote(Guid pollId, VoteRequest voteRequest);  // Change signature to match the service
    void RetractVote(Guid pollId, Guid userId);
    void CreatePoll(Poll poll);
    IEnumerable<Poll> GetAllPolls();
    Poll? GetPollById(Guid pollId);
}