using Microsoft.AspNetCore.Http;

namespace appBackend.Dtos.Group;

public class CreateNewGroupRequest
{
  public string Name { get; set; } = "";
  public Guid UserId { get; set; } = Guid.Empty;
  public string? Description { get; set; } = "";
  public IFormFile? GroupPicture { get; set; }
}
