using BusinessLogic.IServices;
using DataAccess.Constant;
using DataAccess.DTOs.ChatDTOs;
using DataAccess.PaginatedList;
using DataAccess.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using BusinessLogic.Hubs;
using BusinessLogic.Helpers;

namespace Product_Sale_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(IChatService chatService, IHubContext<ChatHub> hubContext)
        {
            _chatService = chatService;
            _hubContext = hubContext;
        }

        // GET /api/chat?pageIndex=&pageSize=&chatBoxId=&userId=
        [HttpGet]
        public async Task<IActionResult> GetMessages(
            int pageIndex = 1,
            int pageSize = 20,
            int? chatBoxId = null)
        {
            var callerId = this.GetUserId();
            var callerRole = this.GetUserRole();

            if (callerRole == RoleConstants.Customer)
            {
                if (!chatBoxId.HasValue)
                    return BadRequest("Customers must specify chatBoxId.");
            }

            var paged = await _chatService.GetMessagesAsync(
                pageIndex, pageSize, chatBoxId,
                userId: callerRole == RoleConstants.Customer ? callerId : null);

            return Ok(new BaseResponseModel<PaginatedList<ChatMessageDTO>>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                paged,
                "Messages retrieved."));
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
            var callerId = this.GetUserId();
            var callerRole = this.GetUserRole();

            var msg = await _chatService.GetMessageByIdAsync(id);

            // if caller is Customer, they must own this message's box
            if (callerRole == RoleConstants.Customer && msg.UserId != callerId)
                return Forbid();


            return Ok(new BaseResponseModel<ChatMessageDTO>(
                StatusCodes.Status200OK,
                code: ResponseCodeConstants.SUCCESS,
                data: msg,
                message: "Message retrieved."));
        }

        // Controllers/ChatController.cs
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            string keyword,
            int pageIndex = 1,
            int pageSize = 20,
            int? chatBoxId = null)
        {
            var callerId = this.GetUserId();
            var callerRole = this.GetUserRole();

            if (callerRole == RoleConstants.Customer && !chatBoxId.HasValue)
                return BadRequest("Customers must specify chatBoxId.");

            var paged = await _chatService.SearchMessagesAsync(
               keyword, pageIndex, pageSize, chatBoxId,
               userId: callerRole == RoleConstants.Customer ? callerId : null);

            return Ok(new BaseResponseModel<PaginatedList<ChatMessageDTO>>(
                StatusCodes.Status200OK,
                ResponseCodeConstants.SUCCESS,
                paged,
                "Search results."));
        }

        // POST /api/chat
        [HttpPost]
        public async Task<IActionResult> Send([FromBody] SendChatMessageRequestDTO dto)
        {
            var userId = this.GetUserId();
            var sent = await _chatService.SendMessageAsync(userId, dto);

            // broadcast
            await _hubContext
                .Clients
                .Group($"box-{sent.ChatBoxId}")
                .SendAsync("ReceiveMessage", sent);

            return StatusCode(StatusCodes.Status201Created,
                new BaseResponseModel<ChatMessageDTO>(
                     StatusCodes.Status201Created,
                     ResponseCodeConstants.SUCCESS,
                     sent,
                     "Message sent."));
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
