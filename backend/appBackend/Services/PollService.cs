using appBackend.DTOs; // For VoteRequest
using appBackend.Models;
using appBackend.Repositories; // For IPollRepository
using Microsoft.EntityFrameworkCore; // For DbContext and ToListAsync potentially
using System;
using System.Collections.Generic;
using System.Linq;

namespace appBackend.Services // Ensure namespace matches registration
{
    public class PollService : IPollService // Implement the interface
    {
        private readonly IPollRepository _pollRepository;
        private readonly SocialMediaDbContext _context; // Need DbContext for Vote table access

        public PollService(IPollRepository pollRepository, SocialMediaDbContext context)
        {
            _pollRepository = pollRepository ?? throw new ArgumentNullException(nameof(pollRepository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void CreatePoll(Poll poll)
        {
            // Basic validation
            if (poll == null || string.IsNullOrWhiteSpace(poll.Question) || poll.Options == null || poll.Options.Count < 2 || poll.Options.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("Invalid poll data provided. Ensure question and at least two non-empty options are present.");
            }
            // Initialize Votes dictionary if it wasn't provided (it has a default in model)
            poll.Votes ??= new Dictionary<int, int>();
            // Ensure creator UserId is set (Controller should have done this from claims)
            if (poll.UserId == Guid.Empty)
            {
                throw new ArgumentException("Poll creator User ID is missing.");
            }
            // Assign a new Guid if not already set (though controller might do this)
            if (poll.PollId == Guid.Empty) poll.PollId = Guid.NewGuid();
            poll.CreatedAt = DateTime.UtcNow;

            _pollRepository.Add(poll); // Assumes repository calls SaveChanges
        }

        public IEnumerable<Poll> GetAllPolls()
        {
            // Fetch all polls, consider ordering
            // Add .AsNoTracking() if you don't intend to modify the polls after fetching in this context
            return _pollRepository.GetAll().OrderByDescending(p => p.CreatedAt);
        }

        public Poll? GetPollById(Guid pollId)
        {
            // Add .AsNoTracking() if just reading
            return _pollRepository.GetById(pollId);
        }

        // --- Updated CastVote to handle VoteRequest DTO and changing votes ---
        public void CastVote(Guid pollId, VoteRequest voteRequest)
        {
            // Use DbContext directly for transactional integrity of Vote and Poll updates
            // This avoids potential issues with SaveChanges being called multiple times
            // if the repository also calls it.

            // 1. Fetch the Poll (must exist)
            Poll poll = _context.Polls.FirstOrDefault(p => p.PollId == pollId); // Use context directly
            if (poll == null) throw new ArgumentException($"Poll with ID {pollId} not found.");

            Guid userId = voteRequest.UserId;

            // 2. Determine the new option index/indices based on poll type
            List<int> newVoteIndices = new List<int>();
            if (poll.AllowMultipleSelections)
            {
                if (voteRequest.OptionIndices == null || !voteRequest.OptionIndices.Any())
                    throw new ArgumentException("At least one option must be selected for a multiple-choice poll.");
                foreach (var index in voteRequest.OptionIndices.Distinct())
                {
                    if (index >= 0 && index < poll.Options.Count) newVoteIndices.Add(index);
                    else Console.WriteLine($"Warning: Invalid option index {index} submitted for multi-select poll {pollId}. Skipping.");
                }
                if (!newVoteIndices.Any()) throw new ArgumentException("No valid options were selected.");
            }
            else // Single selection
            {
                if (voteRequest.OptionIndex == null)
                    throw new ArgumentException("An option must be selected for a single-choice poll.");
                int optionIndex = voteRequest.OptionIndex.Value;
                if (optionIndex < 0 || optionIndex >= poll.Options.Count)
                    throw new ArgumentException($"Invalid option index {optionIndex} for poll {pollId}.");
                newVoteIndices.Add(optionIndex);
            }

            // 3. Find and Remove ALL existing votes by this user for this poll
            var existingVotes = _context.Votes
                                       .Where(v => v.PollId == pollId && v.UserId == userId)
                                       .ToList();

            poll.Votes ??= new Dictionary<int, int>(); // Ensure dictionary exists

            if (existingVotes.Any())
            {
                Console.WriteLine($"Removing {existingVotes.Count} existing vote(s) for User {userId} on Poll {pollId}.");
                foreach (var oldVote in existingVotes)
                {
                    // Decrement aggregate count
                    if (poll.Votes.ContainsKey(oldVote.OptionIndex) && poll.Votes[oldVote.OptionIndex] > 0)
                    {
                        poll.Votes[oldVote.OptionIndex]--;
                    }
                    _context.Votes.Remove(oldVote); // Remove the individual vote record
                }
            }

            // 4. Add new votes (increment counts, add new Vote records)
            Console.WriteLine($"Adding {newVoteIndices.Count} new vote(s) for User {userId} on Poll {pollId}.");
            foreach (int newIndex in newVoteIndices)
            {
                // Increment aggregate count
                if (poll.Votes.ContainsKey(newIndex))
                {
                    poll.Votes[newIndex]++;
                }
                else
                {
                    poll.Votes[newIndex] = 1;
                }
                // Add the new individual vote record
                _context.Votes.Add(new Vote
                {
                    VoteId = Guid.NewGuid(),
                    UserId = userId,
                    PollId = pollId,
                    OptionIndex = newIndex,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // 5. Mark Poll as modified explicitly because we changed the Votes dictionary
            _context.Entry(poll).State = EntityState.Modified;

            // 6. Persist ALL changes (Vote removals, Vote additions, Poll update) in one transaction
            try
            {
                int changes = _context.SaveChanges();
                Console.WriteLine($"SaveChanges successful. Affected {changes} records.");
            }
            catch (DbUpdateException dbEx)
            {
                // Log detailed database update errors
                Console.WriteLine($"DbUpdateException saving vote: {dbEx.Message}");
                Console.WriteLine($"Inner Exception: {dbEx.InnerException?.Message}");
                // You might want to inspect dbEx.Entries here for more details
                throw new InvalidOperationException("Could not save vote due to a database issue.", dbEx); // Re-throw a more specific or generic exception
            }
            catch (Exception ex) // Catch any other potential errors during SaveChanges
            {
                Console.WriteLine($"Generic Exception saving vote: {ex.Message}");
                throw; // Re-throw the original exception
            }
        }

        // RetractVote also needs to use the context directly and save changes
        public void RetractVote(Guid pollId, Guid userId)
        {
            var existingVotes = _context.Votes
                                       .Where(v => v.PollId == pollId && v.UserId == userId)
                                       .ToList();

            if (!existingVotes.Any())
            {
                Console.WriteLine($"User {userId} has no vote to retract for poll {pollId}.");
                return;
            }

            Poll poll = _context.Polls.FirstOrDefault(p => p.PollId == pollId); // Get poll via context
            if (poll == null) throw new ArgumentException($"Poll {pollId} not found.");

            poll.Votes ??= new Dictionary<int, int>();
            bool changed = false;

            foreach (var vote in existingVotes)
            {
                if (poll.Votes.ContainsKey(vote.OptionIndex) && poll.Votes[vote.OptionIndex] > 0)
                {
                    poll.Votes[vote.OptionIndex]--;
                    changed = true;
                }
                _context.Votes.Remove(vote);
                changed = true;
            }

            if (changed)
            {
                _context.Entry(poll).State = EntityState.Modified; // Mark poll as modified
                try
                {
                    int changes = _context.SaveChanges(); // Save both Vote removal and Poll update
                    Console.WriteLine($"RetractVote SaveChanges successful. Affected {changes} records.");
                }
                catch (DbUpdateException dbEx)
                {
                    Console.WriteLine($"DbUpdateException retracting vote: {dbEx.Message}");
                    Console.WriteLine($"Inner Exception: {dbEx.InnerException?.Message}");
                    throw new InvalidOperationException("Could not retract vote due to a database issue.", dbEx);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Generic Exception retracting vote: {ex.Message}");
                    throw;
                }
            }
        }
    }
}