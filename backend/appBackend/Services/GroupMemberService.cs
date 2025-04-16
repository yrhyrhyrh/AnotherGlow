using appBackend.Adapters;
using appBackend.Dtos.Group;
using appBackend.Models;
using appBackend.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace appBackend.Services
{
    public class GroupMemberService
    {
        private readonly IGroupMemberRepository _groupMemberRepository;
        private readonly IGroupMemberAdapter _groupMemberAdapter;

        public GroupMemberService(
            IGroupMemberRepository groupMemberRepository, 
            IGroupMemberAdapter groupMemberAdapter
        )
        {
            _groupMemberRepository = groupMemberRepository;
            _groupMemberAdapter = groupMemberAdapter;
        }

        public async Task<bool> AddGroupMembersAsync(List<GroupMemberRequest> groupMemberRequests)
        {
            Console.WriteLine("Adding new group members");

            // Create a list to hold new GroupMembers
            var newGroupMembers = new List<GroupMember>();

            foreach (var groupMemberRequest in groupMemberRequests)
            {
                var newGroupMember = _groupMemberAdapter.ToGroupMember(groupMemberRequest);

                newGroupMembers.Add(newGroupMember); // Add to the list
            }

            var added = await _groupMemberRepository.AddGroupMembersAsync(newGroupMembers);

            return added;

        }
    }
}
