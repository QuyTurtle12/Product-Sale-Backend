using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTOs.ChatDTOs
{
    public class ChatMessageDTO
    {
        public int ChatMessageId { get; set; }
        public int? UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime SentAt { get; set; }
    }
}
