using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using appBackend.Services;
using appBackend.Dtos;

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
    public async Task<IActionResult> CreateNewGroup([FromBody] CreateNewGroupRequest request)
    {
        if (request.UserId == Guid.Empty)
        {
            return BadRequest(new { message = "Group Owner User id required." });
        }

        var groupRequest = new GroupRequest{Name=request.Name};
        var group_id = await _groupService.CreateGroupAsync(groupRequest);
        
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

    [HttpGet("detail/{group_id}")]
    public async Task<IActionResult> GetGroupDetailById(Guid group_id)
    {
        if (group_id == Guid.Empty)
        {
            return BadRequest(new { message = "Group ID is required." });
        }

        var group = await _groupService.GetGroupAsync(group_id);

        if (group == null)
        {
            return NotFound(new { message = "Group not found." });
        }

        return Ok(group);
    }


}
