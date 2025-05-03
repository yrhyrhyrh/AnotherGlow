using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using appBackend.Services;
using appBackend.Dtos.Group;
using appBackend.Adapters;

namespace appBackend.Controllers;

[ApiController]
[Route("api/group")]
public class GroupController : ControllerBase
{
    private readonly GroupService _groupService;
    private readonly GroupMemberService _groupMemberService;

    public GroupController(IConfiguration configuration, GroupService groupService, GroupMemberService groupMemberService)
    {
        _groupService = groupService;
        _groupMemberService = groupMemberService;
    }

    [HttpPost("getbyuserid")]
    public async Task<IActionResult> GetGroupsByUserIdAsync([FromBody] GetGroupsByUseridRequest request)
    {
        if (request.UserId == Guid.Empty)
        {
            return BadRequest(new { message = "Group Owner User id required." });
        }

        var groups = await _groupService.GetGroupsByUserIdAsync(request);
        
        return Ok(new {groups = groups });
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateNewGroup([FromForm] CreateNewGroupRequest request)
    {
        if (request.UserId == Guid.Empty)
        {
            return BadRequest(new { message = "Group Owner User id required." });
        }

        if (request.GroupPicture != null && request.GroupPicture.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(request.GroupPicture.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { message = "Invalid file type. Allowed types: jpg, jpeg, png, gif" });
            }

            if (request.GroupPicture.Length > 5 * 1024 * 1024) // 5MB limit
            {
                return BadRequest(new { message = "File size too large. Maximum size: 5MB" });
            }
        }

        var group_id = await _groupService.CreateGroupAsync(request);
        
        if (group_id == Guid.Empty)
        {
            return Conflict(new { message = "Group Name already exists." });
        }

        var groupMemberRequest = new GroupMemberRequest{
            GroupId = group_id,
            UserId = request.UserId,
            IsAdmin = true,
        };
        var added = await _groupMemberService.AddGroupMembersAsync([groupMemberRequest]);

        if (added)
          return Ok(new {message = "Group created successfully!" });
        else
          return StatusCode(500, new { message = "Database error while adding group members"});
    }

    [HttpGet("detail/{groupId}")]
    public async Task<IActionResult> GetGroupDetailById(Guid groupId)
    {
        if (groupId == Guid.Empty)
        {
            return BadRequest(new { message = "Group ID is required." });
        }

        // Extract user ID from JWT token
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid currentUserId))
        {
            return Unauthorized(new { message = "Invalid or missing user ID in token." });
        }

        var group = await _groupService.GetGroupAsync(groupId, currentUserId);

        if (group == null)
        {
            return NotFound(new { message = "Group not found." });
        }

        return Ok(group);
    }

    [HttpGet("getUsersToAdd/{groupId}")]
    [AdminAuthorization]
    public async Task<IActionResult> SearchUsersNotInGroupAsync(Guid groupId, [FromQuery] string? keyword)
    {
        var users = await _groupService.SearchUsersNotInGroupAsync(groupId, keyword ?? string.Empty);

        if (!users.Any())
        {
            return NotFound(new { message = "No users to add." });
        }

        return Ok(users);
    }

    [HttpPost("addMembers/{groupId}")]
    [AdminAuthorization]
    public async Task<IActionResult> AddNewMembers(Guid groupId, [FromBody] AddNewGroupMembersRequest request)
    {
        var groupMemberRequests = new List<GroupMemberRequest>();
        foreach (var userid in request.UserIds)
        {
            groupMemberRequests.Add(new GroupMemberRequest{
                GroupId = groupId,
                UserId = userid,
                IsAdmin = false,
            });
        }
        
        var added = await _groupMemberService.AddGroupMembersAsync(groupMemberRequests);

        if (added)
          return Ok(new {message = "Member added successfully!" });
        else
          return StatusCode(500, new { message = "Database error while adding group members"});
    }    

    [HttpPost("removeMember/{groupId}/{groupMemberId}")]
    [AdminAuthorization]
    public async Task<IActionResult> RemoveMember(Guid groupId, Guid groupMemberId)
    {
        var removed = await _groupMemberService.RemoveMemberAsync(groupMemberId);
        
        if (removed)
          return Ok(new {message = "Member removed successfully!" });
        else
          return StatusCode(500, new { message = "Database error while removing group members"});
    }

    [HttpPost("makeAdmin/{groupId}/{groupMemberId}")]
    [AdminAuthorization]
    public async Task<IActionResult> MakeAdmin(Guid groupId, Guid groupMemberId)
    {
        var madeAdmin = await _groupMemberService.MakeAdminAsync(groupMemberId);
        
        if (madeAdmin)
          return Ok(new {message = "Member made admin successfully!" });
        else
          return StatusCode(500, new { message = "Database error while making member admin"});
    }

    [HttpPut("update/{groupId}")]
    [AdminAuthorization]
    public async Task<IActionResult> UpdateGroup(Guid groupId, [FromForm] UpdateGroupRequest request)
    {
        var updated = await _groupService.UpdateGroupAsync(request);
        if (updated)
          return Ok(new {message = "Group updated successfully!" });
        else
          return StatusCode(500, new { message = "Database error while updating group"});
    }

    [HttpDelete("delete/{groupId}")]
    [AdminAuthorization]
    public async Task<IActionResult> DeleteGroup(Guid groupId)
    {
        var deleted = await _groupService.DeleteGroupAsync(groupId);
        if (deleted)
        {
            return Ok(new { message = "Group deleted successfully!" });
        }
        else
        {
            return StatusCode(500, new { message = "Failed to delete group." });
        }
    }
}
