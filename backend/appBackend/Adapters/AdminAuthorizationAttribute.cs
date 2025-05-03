using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using appBackend.Services;
using System.Security.Claims;

namespace appBackend.Adapters
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AdminAuthorizationAttribute : TypeFilterAttribute
    {
        public AdminAuthorizationAttribute() : base(typeof(AdminAuthorizationFilter))
        {
        }

        private class AdminAuthorizationFilter : IAsyncActionFilter
        {
            private readonly GroupService _groupService;

            public AdminAuthorizationFilter(GroupService groupService)
            {
                _groupService = groupService;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                // Get groupId from route parameters
                var groupId = context.ActionArguments["groupId"] as Guid?;
                if (!groupId.HasValue)
                {
                    context.Result = new BadRequestObjectResult(new { message = "Group ID is required." });
                    return;
                }

                // Get user ID from claims
                var userIdClaim = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid currentUserId))
                {
                    context.Result = new UnauthorizedObjectResult(new { message = "Invalid or missing user ID in token." });
                    return;
                }

                // Check if user is admin of the group
                var group = await _groupService.GetGroupAsync(groupId.Value, currentUserId);
                if (group == null)
                {
                    context.Result = new NotFoundObjectResult(new { message = "Group not found." });
                    return;
                }

                if (!group.IsAdmin)
                {
                    context.Result = new StatusCodeResult(403);
                    return;
                }

                await next();
            }
        }
    }
} 