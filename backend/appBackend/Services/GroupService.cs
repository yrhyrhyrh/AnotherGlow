using appBackend.Models;
using appBackend.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using appBackend.Adapters;
using appBackend.Dtos;

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

        public async Task<Group?> GetGroupAsync(Guid group_id)
        {
            Console.WriteLine("Getting group details");
            Console.WriteLine(group_id);

            var group = await _groupRepository.GetGroupAsync(group_id);

            return group;
        }

        public async Task<Guid> CreateGroupAsync(GroupRequest groupRequest)
        {
            Console.WriteLine("new group name");
            Console.WriteLine(groupRequest.Name);
            
            var groupEntity = _groupAdapter.ToGroup(groupRequest);

            var createdGroup_Id = await _groupRepository.CreateGroupAsync(groupEntity);

            return createdGroup_Id;
        }
    }
}
