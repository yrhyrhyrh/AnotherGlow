using Microsoft.AspNetCore.Identity;
using appBackend.Models;
using appBackend.Dtos;

namespace appBackend.Adapters
{
    public interface IGroupAdapter
    {
        Group ToGroup(GroupRequest dto);
    }

    public class GroupAdapter : IGroupAdapter
    {
        public Group ToGroup(GroupRequest dto)
        {
            var group = new Group
            {
                Name = dto.Name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            return group;
        }
    }
}
