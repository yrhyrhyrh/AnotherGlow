public class PollService : IPollService
{
    private readonly IPollRepository _pollRepository;

    public PollService(IPollRepository pollRepository)
    {
        _pollRepository = pollRepository;
    }

    public void CreatePoll(Poll poll) => _pollRepository.Add(poll);
    public IEnumerable<Poll> GetAllPolls() => _pollRepository.GetAll();
}