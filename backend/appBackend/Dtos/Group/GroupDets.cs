namespace appBackend.Dtos.Group;

public class GroupDto
{
    public Guid GroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public string? GroupPictureUrl { get; set; } = string.Empty;
    public List<GroupMemberDto> Members { get; set; } = new();
    public bool IsAdmin { get; set; }
}

public class GroupMemberDto
{
    public Guid GroupMemberId { get; set; }
    public bool IsAdmin { get; set; }
    public UserDto User { get; set; } = new();
}

public class UserDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; } = string.Empty;
}
