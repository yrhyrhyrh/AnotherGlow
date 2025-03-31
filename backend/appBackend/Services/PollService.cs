using System;
using System.Collections.Generic;
using System.Linq;


    public class PollService : IPollService  // Assuming you have an interface
    {
        private readonly IPollRepository _pollRepository;

        public PollService(IPollRepository pollRepository)
        {
            _pollRepository = pollRepository;
        }

        public void CastVote(int pollId, int optionIndex)
        {
            // Implement vote casting logic here, handling potential errors
            Poll poll = _pollRepository.GetById(pollId);

            if (poll == null)
            {
                throw new ArgumentException($"Poll with ID {pollId} not found.");
            }

            if (optionIndex < 0 || optionIndex >= poll.Options.Count)
            {
                throw new ArgumentException($"Invalid option index {optionIndex} for poll with ID {pollId}.");
            }

            // Update vote counts.
            if (poll.Votes.ContainsKey(optionIndex))
            {
                poll.Votes[optionIndex]++;
            }
            else
            {
                poll.Votes[optionIndex] = 1;
            }

            _pollRepository.Update(poll);  // You'll need to add an Update method to your repository
        }

        public void CreatePoll(Poll poll)
        {
            _pollRepository.Add(poll);
        }

        public IEnumerable<Poll> GetAllPolls()
        {
            return _pollRepository.GetAll();
        }
    }
