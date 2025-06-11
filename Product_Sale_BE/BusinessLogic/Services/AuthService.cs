using AutoMapper;
using BusinessLogic.IServices;
using DataAccess.Constant;
using DataAccess.DTOs.AuthDTOs;
using DataAccess.DTOs.UserDTOs;
using DataAccess.Entities;
using DataAccess.ExceptionCustom;
using DataAccess.IRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class AuthService : IAuthService
    {
        private readonly IMapper _mapper;
        private readonly IUOW _unitOfWork;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            IMapper mapper,
            IUOW unitOfWork,
            IPasswordHasher<User> passwordHasher,
            IOptions<JwtSettings> jwtOptions)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _jwtSettings = jwtOptions.Value;
        }

        public async Task<UserDTO> RegisterAsync(RegisterRequestDTO dto)
        {
            // Check if username or email is already taken
            var userRepo = _unitOfWork.GetRepository<User>();

            bool usernameExists = userRepo.Entities.Any(u => u.Username == dto.Username);
            if (usernameExists)
                throw new ErrorException(
                    StatusCodes.Status400BadRequest,
                    ResponseCodeConstants.BAD_REQUEST,
                    $"Username '{dto.Username}' is already in use.");

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                if (userRepo.Entities.Any(u => u.Email == dto.Email))
                {
                    throw new ErrorException(
                        StatusCodes.Status400BadRequest,
                        ResponseCodeConstants.BAD_REQUEST,
                        $"Email '{dto.Email}' is already registered.");
                }
            }

            // Map DTO to Entity
            var newUser = new User
            {
                Username = dto.Username,
                Email = dto.Email ?? string.Empty,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                Role = dto.Role // default "Customer"
            };

            // Hash the password
            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, dto.Password);

            // Insert into DB
            userRepo.Insert(newUser);
            await _unitOfWork.SaveAsync();

            // Map Entity to UserDTO (do not expose PasswordHash)
            var userDto = _mapper.Map<UserDTO>(newUser);
            return userDto;
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO dto)
        {
            var userRepo = _unitOfWork.GetRepository<User>();

            // Find user by username or email
            var user = userRepo.Entities
                .FirstOrDefault(u =>
                    u.Username == dto.UsernameOrEmail ||
                    u.Email == dto.UsernameOrEmail);

            if (user == null)
                throw new ErrorException(
                    StatusCodes.Status401Unauthorized,
                    ResponseCodeConstants.UNAUTHORIZED,
                    "Invalid username/email or password.");

            // Verify password
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (result == PasswordVerificationResult.Failed)
                throw new ErrorException(
                    StatusCodes.Status401Unauthorized,
                    ResponseCodeConstants.UNAUTHORIZED,
                    "Invalid username/email or password.");

            // Create claims
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,     user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(ClaimTypes.Role,                  user.Role),
                new Claim(JwtRegisteredClaimNames.Email,    user.Email),
                new Claim(JwtRegisteredClaimNames.Jti,      Guid.NewGuid().ToString())
            };

            // Generate signing credentials
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Compute expiration
            var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

            // Create the token
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Return response DTO
            return new LoginResponseDTO
            {
                Token = tokenString,
                ExpiresAt = expires,
                UserId = user.UserId,
                Username = user.Username,
                Role = user.Role
            };
        }
    }
}
