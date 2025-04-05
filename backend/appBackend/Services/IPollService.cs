using appBackend.Models;

public interface IPollService
{
    void CastVote(Guid pollId, Guid userId, int optionIndex);  // Change signature to match the service
    void RetractVote(Guid pollId, Guid userId);
    void CreatePoll(Poll poll);
    IEnumerable<Poll> GetAllPolls();
}