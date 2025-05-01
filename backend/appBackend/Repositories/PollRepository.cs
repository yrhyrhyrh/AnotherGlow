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

    public async Task<IEnumerable<Poll>> GetAllAsync()
    {
        return await _dbContext.Polls.ToListAsync();
    }

    public async Task<Poll?> GetByIdAsync(Guid pollId)
    {
        return await _dbContext.Polls.FirstOrDefaultAsync(p => p.PollId == pollId);
    }

    public async Task AddAsync(Poll poll)
    {
        await _dbContext.Polls.AddAsync(poll);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Poll poll)
    {
        _dbContext.Entry(poll).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Vote>> GetVotesByPollAndUserAsync(Guid pollId, Guid userId)
    {
        return await _dbContext.Votes
            .Where(v => v.PollId == pollId && v.UserId == userId)
            .ToListAsync();
    }

    public async Task AddVoteAsync(Vote vote)
    {
        await _dbContext.Votes.AddAsync(vote);
    }

    public async Task RemoveVoteAsync(Vote vote)
    {
        _dbContext.Votes.Remove(vote);
    }


}
