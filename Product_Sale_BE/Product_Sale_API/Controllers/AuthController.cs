// Product_Sale_API/Controllers/AuthController.cs
using BusinessLogic.IServices;
using BusinessLogic.Services;
using DataAccess.Constant;
using DataAccess.DTOs.AuthDTOs;
using DataAccess.DTOs.UserDTOs;
using DataAccess.ResponseModel;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IUserService _userService;

        public AuthController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
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


        [HttpGet("test/admin")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> RequireAdminRole()
        {
            return Ok();
        }

        [HttpGet("test/customer")]
        [Authorize(Roles = RoleConstants.Customer)]
        public async Task<IActionResult> RequireCustomerRole()
        {
            return Ok();
        }

        [HttpGet("test/user")]
        [Authorize]
        public async Task<IActionResult> RequireAnyUserRole()
        {
            return Ok();
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUsernameByIdAsync(int id)
        {
            var username = await _userService.GetUsernameByIdAsync(id);
            return Ok(new BaseResponseModel<string>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: username,
                message: "Username retrieved successfully."
            ));
        }

    }
}
