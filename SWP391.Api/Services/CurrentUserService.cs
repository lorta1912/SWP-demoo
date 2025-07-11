using Microsoft.AspNetCore.Http;
using SWP391.Api.Services.Interfaces;
using SWP391.Application.Services.Interfaces;
using System.Security.Claims;

namespace SWP391.Api.Services 
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<int> GetCurrentUserIdAsync()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || !user.Identity?.IsAuthenticated == true)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var userIdClaim = user.FindFirstValue("sub");

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException($"Invalid or missing UserId in token. Value was: '{userIdClaim}'");
            }

            return Task.FromResult(userId);
        }
    }
}
