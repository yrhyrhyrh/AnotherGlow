namespace appBackend.Dtos.Group;

public class GetGroupsByUseridRequest
{
  public Guid UserId { get; set; } = Guid.Empty;
  public bool IsAdmin { get; set; } = false;
}
