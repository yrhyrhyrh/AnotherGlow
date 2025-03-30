public interface IPollRepository
{
    IEnumerable<Poll> GetAll();
    Poll GetById(int id);
    void Add(Poll poll);
}