using appBackend.Repositories;
using Microsoft.EntityFrameworkCore;
using appBackend.Models;

public class PollRepository : IPollRepository
{
    private readonly SocialMediaDbContext _dbContext;

    public PollRepository(SocialMediaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IEnumerable<Poll> GetAll()
    {
        return _dbContext.Polls.ToList();
    }

    public Poll GetById(Guid pollId)  // Change the parameter type to Guid
    {
        return _dbContext.Polls.FirstOrDefault(p => p.PollId == pollId);  // Ensure you're using Guid here
    }

    public void Add(Poll poll)
    {
        _dbContext.Polls.Add(poll);
        _dbContext.SaveChanges();
    }

    public void Update(Poll poll)
    {
        _dbContext.Entry(poll).State = EntityState.Modified;  // Flag it for update
        _dbContext.SaveChanges();
    }
}
