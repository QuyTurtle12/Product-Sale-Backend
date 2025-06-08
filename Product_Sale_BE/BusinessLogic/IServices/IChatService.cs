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
        Task<PaginatedList<ChatMessageDTO>> GetMessagesAsync(int pageIndex, int pageSize, int? userId = null);

        Task<ChatMessageDTO> SendMessageAsync(int userId, SendChatMessageRequestDTO dto);
    }

}
