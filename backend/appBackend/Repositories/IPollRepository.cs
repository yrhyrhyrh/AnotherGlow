using appBackend.Repositories;
using appBackend.Models;

public interface IPollRepository
{
    Task<IEnumerable<Poll>> GetAllAsync();
    Task<Poll> GetByIdAsync(Guid id);
    Task AddAsync(Poll poll);
    Task UpdateAsync(Poll poll);
}
