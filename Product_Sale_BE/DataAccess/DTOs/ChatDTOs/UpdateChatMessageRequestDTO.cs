using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTOs.ChatDTOs
{
    public class UpdateChatMessageRequestDTO
    {
        [Required]
        [StringLength(500)]
        public string Message { get; set; } = null!;
    }
}
