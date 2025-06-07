using BusinessLogic.IServices;
using DataAccess.Constant;
using DataAccess.DTOs.ChatDTOs;
using DataAccess.PaginatedList;
using DataAccess.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Product_Sale_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        /// <summary>
        /// Get paginated chat messages. Optionally filter by userId.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMessages(
            int pageIndex = 1,
            int pageSize = 20,
            int? userId = null)
        {
            PaginatedList<ChatMessageDTO> result =
                await _chatService.GetMessagesAsync(pageIndex, pageSize, userId);

            return Ok(new BaseResponseModel<PaginatedList<ChatMessageDTO>>(
                statusCode: StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: result,
                message: "Messages retrieved."
            ));
        }

        /// <summary>
        /// Send a new chat message.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendChatMessageRequestDTO dto)
        {

            string? sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(sub) || !int.TryParse(sub, out int userId))
            {
                return Forbid();   // or return Unauthorized();
            }

            ChatMessageDTO sent =
                await _chatService.SendMessageAsync(userId, dto);

            return Ok(new BaseResponseModel<ChatMessageDTO>(
                statusCode: StatusCodes.Status201Created,
                code: ResponseCodeConstants.SUCCESS,
                data: sent,
                message: "Message sent."
            ));
        }
    }
}
