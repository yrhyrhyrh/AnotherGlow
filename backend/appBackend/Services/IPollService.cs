public interface IPollService
{
    void CreatePoll(Poll poll);
    IEnumerable<Poll> GetAllPolls();
}