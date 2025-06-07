using AutoMapper;
using BusinessLogic.IServices;
using DataAccess.DTOs.ChatDTOs;
using DataAccess.Entities;
using DataAccess.IRepositories;
using DataAccess.PaginatedList;
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

        public ChatService(IUOW uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<PaginatedList<ChatMessageDTO>> GetMessagesAsync(int pageIndex, int pageSize, int? userId = null)
        {
            var repo = _uow.GetRepository<ChatMessage>();

            IQueryable<ChatMessage> query = repo.Entities
                .Include(cm => cm.User)
                .OrderBy(cm => cm.SentAt);

            if (userId.HasValue)
                query = query.Where(cm => cm.UserId == userId.Value);

            var paged = await repo.GetPagging(query, pageIndex, pageSize);

            var dtoItems = paged.Items
                .Select(cm => _mapper.Map<ChatMessageDTO>(cm))
                .ToList();

            return new PaginatedList<ChatMessageDTO>(
                dtoItems, paged.TotalCount, paged.PageNumber, paged.PageSize);
        }

        public async Task<ChatMessageDTO> SendMessageAsync(int userId, SendChatMessageRequestDTO dto)
        {
            var repo = _uow.GetRepository<ChatMessage>();

            var entity = new ChatMessage
            {
                UserId = userId,
                Message = dto.Message,
                SentAt = DateTime.UtcNow
            };

            repo.Insert(entity);
            await _uow.SaveAsync();

            var saved = await repo.Entities
                .Include(cm => cm.User)                        
                .FirstOrDefaultAsync(cm => cm.ChatMessageId == entity.ChatMessageId);

            return _mapper.Map<ChatMessageDTO>(saved!);
        }
    }
}
