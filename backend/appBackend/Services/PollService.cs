using appBackend.Controllers;
using appBackend.Dtos.GlobalPostWall;
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

        public async Task CreatePollAsync(CreatePollDTO poll)
        {
            if (poll == null || string.IsNullOrWhiteSpace(poll.Question) || poll.Options == null || poll.Options.Count < 2 || poll.Options.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("Invalid poll data provided. Ensure question and at least two non-empty options are present.");
            }

            Poll pollEntity = new Poll()
            {
                PostId = poll.PostId,
                Question = poll.Question,
                Options = poll.Options,
                IsGlobal = poll.IsGlobal,
                AllowMultipleSelections = poll.AllowMultipleSelections,
                Votes = new Dictionary<int, int>(),
                PollId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            await _pollRepository.AddAsync(pollEntity);
        }

        public async Task<IEnumerable<Poll>> GetAllPollsAsync()
        {
            return (await _pollRepository.GetAllAsync()).OrderByDescending(p => p.CreatedAt);
        }

        public async Task<Poll?> GetPollByIdAsync(Guid pollId)
        {
            return await _pollRepository.GetByIdAsync(pollId);
        }

        public async Task CastVoteAsync(Guid pollId, VoteRequest voteRequest)
        {
            Poll poll = await _context.Polls.FirstOrDefaultAsync(p => p.PollId == pollId);
            if (poll == null) throw new ArgumentException($"Poll with ID {pollId} not found.");

            Guid userId = voteRequest.UserId;
            List<int> newVoteIndices = new List<int>();

            if (poll.AllowMultipleSelections)
            {
                if (voteRequest.OptionIndices == null || !voteRequest.OptionIndices.Any())
                    throw new ArgumentException("At least one option must be selected for a multiple-choice poll.");

                foreach (var index in voteRequest.OptionIndices.Distinct())
                {
                    if (index >= 0 && index < poll.Options.Count) newVoteIndices.Add(index);
                }

                if (!newVoteIndices.Any()) throw new ArgumentException("No valid options were selected.");
            }
            else
            {
                if (voteRequest.OptionIndex == null)
                    throw new ArgumentException("An option must be selected for a single-choice poll.");

                int optionIndex = voteRequest.OptionIndex.Value;
                if (optionIndex < 0 || optionIndex >= poll.Options.Count)
                    throw new ArgumentException($"Invalid option index {optionIndex} for poll {pollId}.");

                newVoteIndices.Add(optionIndex);
            }

            var existingVotes = await _context.Votes
                .Where(v => v.PollId == pollId && v.UserId == userId)
                .ToListAsync();

            poll.Votes ??= new Dictionary<int, int>();

            foreach (var oldVote in existingVotes)
            {
                if (poll.Votes.ContainsKey(oldVote.OptionIndex) && poll.Votes[oldVote.OptionIndex] > 0)
                {
                    poll.Votes[oldVote.OptionIndex]--;
                }
                _context.Votes.Remove(oldVote);
            }

            foreach (int newIndex in newVoteIndices)
            {
                if (poll.Votes.ContainsKey(newIndex))
                {
                    poll.Votes[newIndex]++;
                }
                else
                {
                    poll.Votes[newIndex] = 1;
                }

                await _context.Votes.AddAsync(new Vote
                {
                    VoteId = Guid.NewGuid(),
                    UserId = userId,
                    PollId = pollId,
                    OptionIndex = newIndex,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.Entry(poll).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }

        public async Task RetractVoteAsync(Guid pollId, Guid userId)
        {
            var existingVotes = await _context.Votes
                .Where(v => v.PollId == pollId && v.UserId == userId)
                .ToListAsync();

            if (!existingVotes.Any())
            {
                return;
            }

            Poll poll = await _context.Polls.FirstOrDefaultAsync(p => p.PollId == pollId);
            if (poll == null) throw new ArgumentException($"Poll {pollId} not found.");

            poll.Votes ??= new Dictionary<int, int>();

            foreach (var vote in existingVotes)
            {
                if (poll.Votes.ContainsKey(vote.OptionIndex) && poll.Votes[vote.OptionIndex] > 0)
                {
                    poll.Votes[vote.OptionIndex]--;
                }
                _context.Votes.Remove(vote);
            }

            _context.Entry(poll).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }
    }
}