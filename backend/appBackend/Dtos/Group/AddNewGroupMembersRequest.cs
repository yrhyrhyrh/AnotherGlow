namespace appBackend.Dtos.Group;

public class AddNewGroupMembersRequest
{
  public Guid GroupId { get; set; } = Guid.Empty;
  public List<Guid> UserIds { get; set; } = []; 
}