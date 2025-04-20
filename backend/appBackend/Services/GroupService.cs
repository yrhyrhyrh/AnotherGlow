using appBackend.Models;
using appBackend.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using appBackend.Adapters;
using appBackend.Dtos.Group;

namespace appBackend.Services
{
    public class GroupService
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IGroupAdapter _groupAdapter;

        public GroupService(
            IGroupRepository groupRepository, 
            IGroupAdapter groupAdapter
        )
        {
            _groupRepository = groupRepository;
            _groupAdapter = groupAdapter;
        }

        public async Task<List<Group>> GetGroupsByUserIdAsync(GetGroupsByUseridRequest request)
        {
            Console.WriteLine("Getting groups where I am"+(!request.IsAdmin?" not":"")+" admin");
            Console.WriteLine(request.UserId);

            var groups = await _groupRepository.GetGroupsByUserIdAsync(request.UserId, request.IsAdmin);

            return groups;
        }

        public async Task<GroupDto?> GetGroupAsync(Guid group_id)
        {
            Console.WriteLine("Getting group details");
            Console.WriteLine(group_id);

            var group = await _groupRepository.GetGroupAsync(group_id);

            return group;
        }

        public async Task<Guid> CreateGroupAsync(CreateNewGroupRequest groupRequest)
        {
            Console.WriteLine("new group name");
            Console.WriteLine(groupRequest.Name);
            
            var groupEntity = _groupAdapter.ToGroup(groupRequest);

            var createdGroup_Id = await _groupRepository.CreateGroupAsync(groupEntity);

            return createdGroup_Id;
        }

        public async Task<List<UserDto>> SearchUsersNotInGroupAsync(Guid group_id, string keyword)
        {
            Console.WriteLine("Searching users not in group");
            var users = await _groupRepository.SearchUsersNotInGroupAsync(group_id, keyword);
            return users;
        }
    }
}
