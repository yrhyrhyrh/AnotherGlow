namespace appBackend.Dtos.Group;

public class CreateNewGroupRequest
{
  public string Name { get; set; } = "";
  public Guid UserId { get; set; } = Guid.Empty;
  public string? Description { get; set; } = "";
  public string? GroupPictureUrl { get; set; } = "";
}
