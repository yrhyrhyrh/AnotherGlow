using System.Collections.Generic;
using System.Linq;
using appBackend.Repositories;

namespace MyAppBackend.Repositories // Replace with your actual namespace
{
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

        public Poll GetById(int id)
        {
            return _dbContext.Polls.FirstOrDefault(p => p.Id == id);
        }

        public void Add(Poll poll)
        {
            _dbContext.Polls.Add(poll);
            _dbContext.SaveChanges();
        }
    }
}