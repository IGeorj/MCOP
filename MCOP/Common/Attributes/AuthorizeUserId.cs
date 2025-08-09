using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace MCOP.Common.Attributes
{
    public sealed class AuthorizeUserIdAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly ulong _requiredOwnerId;

        public AuthorizeUserIdAttribute(ulong requiredOwnerId)
        {
            _requiredOwnerId = requiredOwnerId;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User.Identity is null || !context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null || ulong.Parse(userId) != _requiredOwnerId)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
