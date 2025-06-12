using AutoMapper;
using BusinessLogic.Hubs;
using BusinessLogic.IServices;
using DataAccess.DTOs.ChatDTOs;
using DataAccess.Entities;
using DataAccess.IRepositories;
using DataAccess.PaginatedList;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BusinessLogic.Services
{
    public class ChatService : IChatService
    {
        private readonly IUOW _uow;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHub> _hubContext;
        public ChatService(IUOW uow, IMapper mapper, IHubContext<ChatHub> hubContext)
        {
            _uow = uow;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task<PaginatedList<ChatMessageDTO>> GetMessagesAsync(
            int pageIndex, int pageSize, int? chatBoxId = null, int? userId = null)
        {
            var repo = _uow.GetRepository<ChatMessage>();
            var query = repo.Entities
                            .Include(cm => cm.User)
                            .OrderBy(cm => cm.SentAt);

            if (chatBoxId.HasValue)
                query = (IOrderedQueryable<ChatMessage>)query.Where(cm => cm.ChatBoxId == chatBoxId.Value);

            if (userId.HasValue)
                query = (IOrderedQueryable<ChatMessage>)query.Where(cm => cm.UserId == userId.Value);

            var paged = await repo.GetPagging(query, pageIndex, pageSize);
            var dto = paged.Items.Select(cm => _mapper.Map<ChatMessageDTO>(cm)).ToList();

            return new PaginatedList<ChatMessageDTO>(
                dto, paged.TotalCount, paged.PageNumber, paged.PageSize);
        }

        public async Task<ChatMessageDTO> GetMessageByIdAsync(int id)
        {
            var repo = _uow.GetRepository<ChatMessage>();

            // Use GetByIdAsync for PK lookup
            var entity = await repo.GetByIdAsync(id)
                       ?? throw new KeyNotFoundException($"Message {id} not found.");

            // Then include the User for mapping
            var withUser = await repo.Entities
                .Include(cm => cm.User)
                .FirstOrDefaultAsync(cm => cm.ChatMessageId == id)
                ?? throw new Exception($"Failed to load message {id} with user.");

            return _mapper.Map<ChatMessageDTO>(withUser);
        }

        public async Task<ChatMessageDTO> SendMessageAsync(
            int userId, SendChatMessageRequestDTO dto)
        {
            var repo = _uow.GetRepository<ChatMessage>();
            var entity = new ChatMessage
            {
                UserId = userId,
                ChatBoxId = dto.ChatBoxId,
                Message = dto.Message,
                SentAt = DateTime.UtcNow
            };
            repo.Insert(entity);
            await _uow.SaveAsync();

            var saved = await repo.Entities
                .Include(cm => cm.User)
                .FirstOrDefaultAsync(cm => cm.ChatMessageId == entity.ChatMessageId)
                ?? throw new Exception("Failed to load saved message.");

            return _mapper.Map<ChatMessageDTO>(saved);
        }

        public async Task<ChatMessageDTO> UpdateMessageAsync(
            int userId, int messageId, UpdateChatMessageRequestDTO dto)
        {
            var repo = _uow.GetRepository<ChatMessage>();
            // Use GetByIdAsync instead of FindAsync on IQueryable
            var entity = await repo.GetByIdAsync(messageId)
                        ?? throw new KeyNotFoundException($"Message {messageId} not found.");

            if (entity.UserId != userId)
                throw new UnauthorizedAccessException("Cannot edit another user’s message.");

            entity.Message = dto.Message;
            repo.Update(entity);
            await _uow.SaveAsync();

            var updated = await repo.Entities
                .Include(cm => cm.User)
                .FirstOrDefaultAsync(cm => cm.ChatMessageId == messageId)
                ?? throw new Exception("Failed to reload updated message.");

            return _mapper.Map<ChatMessageDTO>(updated);
        }
        // Services/ChatService.cs
        public async Task DeleteMessageAsync(int userId, int messageId)
        {
            var repo = _uow.GetRepository<ChatMessage>();
            var entity = await repo.GetByIdAsync(messageId)
                        ?? throw new KeyNotFoundException($"Message {messageId} not found.");

            // only author or admin can delete (you can pass role in or check here)
            if (entity.UserId != userId && /* not admin */ false)
                throw new UnauthorizedAccessException();

            // “Soft” delete:
            entity.Message = "this message is deleted";
            repo.Update(entity);
            await _uow.SaveAsync();

            // broadcast the update
            var dto = _mapper.Map<ChatMessageDTO>(entity);
            await _hubContext
                .Clients
                .Group($"box-{entity.ChatBoxId}")
                .SendAsync("MessageDeleted", dto);
        }

        public async Task<PaginatedList<ChatMessageDTO>> SearchMessagesAsync(
    string keyword,
    int pageIndex, int pageSize,
    int? chatBoxId, int? userId)
        {
            var repo = _uow.GetRepository<ChatMessage>();
            var query = repo.Entities
                            .Include(cm => cm.User)
                            .Where(cm => cm.Message.Contains(keyword))
                            .AsQueryable();

            if (chatBoxId.HasValue)
                query = query.Where(cm => cm.ChatBoxId == chatBoxId.Value);
            if (userId.HasValue)
                query = query.Where(cm => cm.UserId == userId.Value);

            var paged = await repo.GetPagging(query, pageIndex, pageSize);
            var dto = paged.Items.Select(cm => _mapper.Map<ChatMessageDTO>(cm)).ToList();

            return new PaginatedList<ChatMessageDTO>(
                dto, paged.TotalCount, paged.PageNumber, paged.PageSize);
        }

    }
}