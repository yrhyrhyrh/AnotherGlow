using appBackend.Repositories;
using appBackend.Models;

public interface IPollRepository
{
    IEnumerable<Poll> GetAll();
    Poll GetById(Guid id);
    void Add(Poll poll);
    void Update(Poll poll);
}