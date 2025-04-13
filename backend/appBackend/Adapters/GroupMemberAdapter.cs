using Microsoft.AspNetCore.Identity;
using appBackend.Models;
using appBackend.Dtos.Group;

namespace appBackend.Adapters
{
    public interface IGroupMemberAdapter
    {
        GroupMember ToGroupMember(GroupMemberRequest dto);
    }

    public class GroupMemberAdapter : IGroupMemberAdapter
    {
        public GroupMember ToGroupMember(GroupMemberRequest dto)
        {
            var group_member = new GroupMember
            {
                GroupId = dto.GroupId,
                UserId = dto.UserId,
                IsAdmin = dto.IsAdmin,
                CreatedAt = DateTime.UtcNow,
            };
            return group_member;
        }
    }
}
