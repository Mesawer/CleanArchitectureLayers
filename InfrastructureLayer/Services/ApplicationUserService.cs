using System.Security.Claims;
using Mesawer.ApplicationLayer.Interfaces;
using Mesawer.InfrastructureLayer.Models;
using Microsoft.AspNetCore.Http;

namespace Mesawer.InfrastructureLayer.Services
{
    public class ApplicationUserService : IApplicationUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationUserService(IHttpContextAccessor httpContextAccessor)
            => _httpContextAccessor = httpContextAccessor;

        private ClaimsPrincipal User => _httpContextAccessor.HttpContext?.User;

        public string Id => User?.FindFirstValue(AppClaims.Id);
        public string UserName => User?.FindFirstValue(AppClaims.UserName);
        public string Email => User?.FindFirstValue(AppClaims.Email);
    }
}
