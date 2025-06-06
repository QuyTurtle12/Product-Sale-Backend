// Product_Sale_API/Controllers/AuthController.cs
using BusinessLogic.IServices;
using DataAccess.Constant;
using DataAccess.DTOs.AuthDTOs;
using DataAccess.DTOs.UserDTOs;
using DataAccess.ResponseModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Product_Sale_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequestDTO dto)
        {
            var createdUser = await _authService.RegisterAsync(dto);
            return Ok(new BaseResponseModel<UserDTO>(
                statusCode: StatusCodes.Status201Created,
                code: ResponseCodeConstants.SUCCESS,
                data: createdUser,
                message: "User registered successfully."
            ));
        }

        /// <summary>
        /// Logs in a user and returns a JWT + user info.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDTO dto)
        {
            var loginResult = await _authService.LoginAsync(dto);
            return Ok(new BaseResponseModel<LoginResponseDTO>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: loginResult,
                message: "Login successful."
            ));
        }
    }
}
