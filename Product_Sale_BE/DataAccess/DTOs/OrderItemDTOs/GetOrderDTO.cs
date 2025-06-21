using DataAccess.DTOs.CartDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTOs.OrderItemDTOs
{
    public class GetOrderDTO : BaseOrderDTO
    {
        public int OrderId { get; set; }

        public string? Username { get; set; }
        public GetCartDTO? Cart { get; set; }

    }
}
