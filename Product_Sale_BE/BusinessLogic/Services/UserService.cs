using BusinessLogic.IServices;
using DataAccess.DTOs.UserDTOs;
using DataAccess.Entities;        
using DataAccess.IRepositories;    
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace BusinessLogic.Services
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUOW _uow;

        public UserService(
            IHttpContextAccessor httpContextAccessor,
            IUOW unitOfWork)
        {
            _httpContextAccessor = httpContextAccessor;
            _uow = unitOfWork;
        }

        public int GetUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var claim = user.FindFirst(ClaimTypes.NameIdentifier)
                     ?? user.FindFirst(JwtRegisteredClaimNames.Sub);

            if (claim == null || !int.TryParse(claim.Value, out var id))
                throw new UnauthorizedAccessException("Invalid or missing user ID claim.");

            return id;
        }

        public bool IsTokenValid()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true) return false;
            var claim = user.FindFirst(ClaimTypes.NameIdentifier)
                     ?? user.FindFirst(JwtRegisteredClaimNames.Sub);
            return claim != null && int.TryParse(claim.Value, out _);
        }

        public async Task<string> GetUsernameByIdAsync(int userId)
        {
            var repo = _uow.GetRepository<User>();
            var user = await repo.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User {userId} not found.");
            return user.Username;
        }
    }
}
