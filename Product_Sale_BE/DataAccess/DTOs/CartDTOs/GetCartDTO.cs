using DataAccess.DTOs.CartItemDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTOs.CartDTOs
{
    public class GetCartDTO : BaseCartDTO
    {
        public int CartId { get; set; }

        public decimal TotalPrice { get; set; }

        public List<GetCartItemDTO>? CartItems { get; set; }
    }
}
