namespace appBackend.Dtos.Group;

public class GroupMemberRequest
{
  public Guid UserId { get; set; } = Guid.Empty;
  public Guid GroupId { get; set; } = Guid.Empty;
  public bool IsAdmin { get; set; } = false;
}
