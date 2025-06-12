using DataAccess.DTOs.ChatDTOs;
using DataAccess.PaginatedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.IServices
{
    public interface IChatService
    {
        Task<PaginatedList<ChatMessageDTO>> GetMessagesAsync(
            int pageIndex, int pageSize,
            int? chatBoxId = null,
            int? userId = null);

        Task<ChatMessageDTO> GetMessageByIdAsync(int id);

        Task<ChatMessageDTO> SendMessageAsync(
            int userId, SendChatMessageRequestDTO dto);

        Task<ChatMessageDTO> UpdateMessageAsync(
            int userId, int messageId, UpdateChatMessageRequestDTO dto);

        Task DeleteMessageAsync(int userId, int messageId);

        Task<PaginatedList<ChatMessageDTO>> SearchMessagesAsync(
            string keyword, int pageIndex, int pageSize, 
            int? chatBoxId, int? userId);

    }

}
