using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTOs.CartItemDTOs
{
    public class GetCartItemDTO : BaseCartItemDTO
    {
        public int CartItemId { get; set; }
        public decimal Price { get; set; }
        public string ProductName { get; set; } = null!;
        public string? FullDescription { get; set; }
        public string? ImageUrl { get; set; }


    }
}
