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
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        // GET /api/chat?pageIndex=&pageSize=&chatBoxId=&userId=
        [HttpGet]
        public async Task<IActionResult> GetMessages(
            int pageIndex = 1, int pageSize = 20,
            int? chatBoxId = null, int? userId = null)
        {
            var paged = await _chatService.GetMessagesAsync(
                pageIndex, pageSize, chatBoxId, userId);

            return Ok(new BaseResponseModel<PaginatedList<ChatMessageDTO>>(
                StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: paged,
                message: "Messages retrieved."));
        }

        // GET /api/chat/box/{chatBoxId}?pageIndex=&pageSize=
        [HttpGet("box/{chatBoxId}")]
        public async Task<IActionResult> GetByBox(
            int chatBoxId, int pageIndex = 1, int pageSize = 20)
        {
            var paged = await _chatService.GetMessagesAsync(
                pageIndex, pageSize, chatBoxId, null);

            return Ok(new BaseResponseModel<PaginatedList<ChatMessageDTO>>(
                StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: paged,
                message: "Box messages retrieved."));
        }

        // GET /api/chat/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var msg = await _chatService.GetMessageByIdAsync(id);
            return Ok(new BaseResponseModel<ChatMessageDTO>(
                StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: msg,
                message: "Message retrieved."));
        }

        // POST /api/chat
        [HttpPost]
        public async Task<IActionResult> Send([FromBody] SendChatMessageRequestDTO dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var sent = await _chatService.SendMessageAsync(userId, dto);

            return StatusCode(StatusCodes.Status201Created,
                new BaseResponseModel<ChatMessageDTO>(
                    StatusCodes.Status201Created,
                    code: ResponseCodeConstants.SUCCESS,
                    data: sent,
                    message: "Message sent."));
        }

        // PUT /api/chat/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] UpdateChatMessageRequestDTO dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var updated = await _chatService.UpdateMessageAsync(userId, id, dto);

            return Ok(new BaseResponseModel<ChatMessageDTO>(
                StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: updated,
                message: "Message updated."));
        }

        // DELETE /api/chat/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _chatService.DeleteMessageAsync(userId, id);

            return Ok(new BaseResponseModel<object>(
                StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: null,
                message: "Message deleted."));
        }
    }

}
