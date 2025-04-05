namespace appBackend.Dtos;

public class CreateNewGroupRequest
{
  public string Name { get; set; } = "";
  public Guid UserId { get; set; } = Guid.Empty;
}
