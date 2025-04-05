using Microsoft.EntityFrameworkCore;
using appBackend.Models;
using appBackend.Repositories;

public class PollService : IPollService
{
    private readonly IPollRepository _pollRepository;
    private readonly SocialMediaDbContext _context;  // Assuming _context is your DbContext

    public PollService(IPollRepository pollRepository, SocialMediaDbContext context)
    {
        _pollRepository = pollRepository;
        _context = context;
    }

    public void CastVote(Guid pollId, Guid userId, int optionIndex)
    {
        var poll = _pollRepository.GetById(pollId);
        if (poll == null)
        {
            throw new ArgumentException($"Poll with ID {pollId} not found.");
        }

        if (optionIndex < 0 || optionIndex >= poll.Options.Count)
        {
            throw new ArgumentException($"Invalid option index {optionIndex} for poll with ID {pollId}.");
        }

        // Check if user has already voted
        var existingVote = _context.Set<Vote>()
            .FirstOrDefault(v => v.UserId == userId && v.PollId == pollId);

        if (existingVote != null)
        {
            // Decrement the count for the previously selected option
            if (poll.Votes.ContainsKey(existingVote.OptionIndex) && poll.Votes[existingVote.OptionIndex] > 0)
            {
                poll.Votes[existingVote.OptionIndex]--;
            }

            // Update the existing vote to the new option
            existingVote.OptionIndex = optionIndex;
            _context.Update(existingVote);

            // Increment the count for the new option
            if (poll.Votes.ContainsKey(optionIndex))
            {
                poll.Votes[optionIndex]++;
            }
            else
            {
                poll.Votes[optionIndex] = 1;
            }
        }
        else
        {
            // Add a new vote
            var newVote = new Vote
            {
                UserId = userId,
                PollId = pollId,
                OptionIndex = optionIndex,
                CreatedAt = DateTime.UtcNow
            };
            _context.Add(newVote);

            // Increment the count for the chosen option
            if (poll.Votes.ContainsKey(optionIndex))
            {
                poll.Votes[optionIndex]++;
            }
            else
            {
                poll.Votes[optionIndex] = 1;
            }
        }

        // Update poll
        _pollRepository.Update(poll);  // Assuming this internally calls _context.Update(poll)
        _context.SaveChanges();
    }

    public void RetractVote(Guid pollId, Guid userId)
    {
        var poll = _pollRepository.GetById(pollId);
        if (poll == null)
            throw new ArgumentException("Poll not found.");

        var vote = _context.Votes.FirstOrDefault(v => v.PollId == pollId && v.UserId == userId);
        if (vote == null)
            throw new InvalidOperationException("Vote not found.");

        // Decrement vote count in poll
        if (poll.Votes.ContainsKey(vote.OptionIndex) && poll.Votes[vote.OptionIndex] > 0)
        {
            poll.Votes[vote.OptionIndex]--;
        }

        // Remove vote from DB
        _context.Votes.Remove(vote);

        // Save both vote removal and poll update
        _pollRepository.Update(poll);         // Saves the poll
        _context.SaveChanges();               // Saves the vote removal
    }



    public void CreatePoll(Poll poll)
    {
        _pollRepository.Add(poll);
    }

    public IEnumerable<Poll> GetAllPolls()
    {
        return _context.Polls.OrderBy(p => p.CreatedAt).ToList();
    }
}