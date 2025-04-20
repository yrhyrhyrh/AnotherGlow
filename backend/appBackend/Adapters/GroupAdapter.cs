using Microsoft.AspNetCore.Identity;
using appBackend.Models;
using appBackend.Dtos.Group;

namespace appBackend.Adapters
{
    public interface IGroupAdapter
    {
        Group ToGroup(CreateNewGroupRequest dto);
    }

    public class GroupAdapter : IGroupAdapter
    {
        public Group ToGroup(CreateNewGroupRequest dto)
        {
            var group = new Group
            {
                Name = dto.Name,
                Description = dto.Description,
                GroupPictureUrl = dto.GroupPictureUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            return group;
        }
    }
}
