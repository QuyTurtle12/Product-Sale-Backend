using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTOs.ChatDTOs
{
    public class SendChatMessageRequestDTO
    {
        [Required(ErrorMessage = "Message is required.")]
        [StringLength(500, ErrorMessage = "Message can be at most 500 characters.")]
        public string Message { get; set; } = null!;
        
        [Required]
        public int ChatBoxId { get; set; }

    }
}
