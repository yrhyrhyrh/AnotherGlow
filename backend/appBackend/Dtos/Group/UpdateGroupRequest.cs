using Microsoft.AspNetCore.Http;

namespace appBackend.Dtos.Group;

public class UpdateGroupRequest
{
  public Guid GroupId { get; set; } = Guid.Empty;
  public string Name { get; set; } = "";
  public string? Description { get; set; } = "";
  public IFormFile? GroupPicture { get; set; }
}
