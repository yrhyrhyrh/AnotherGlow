public interface IPollService
{
    void CreatePoll(Poll poll);
    IEnumerable<Poll> GetAllPolls();

    void CastVote(int pollId, int optionIndex);
}