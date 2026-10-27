using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace API.Authorization
{
    // the administrator of the whole site, User.IsAppAdmin is set by hand in the database;
    // the flag isn't in the token, so taking it away works at once
    public static class AppAdminPolicy
    {
        public const string Name = "AppAdmin";
    }

    public class AppAdminRequirement : IAuthorizationRequirement
    {
    }

    public class AppAdminHandler : AuthorizationHandler<AppAdminRequirement>
    {
        private readonly IUserRepository _userRepository;

        public AppAdminHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AppAdminRequirement requirement)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId) && await _userRepository.CustomAnyAsync(_userRepository.Where(u => u.Id == userId && u.IsAppAdmin)))
            {
                context.Succeed(requirement);
            }
        }
    }
}
