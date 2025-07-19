using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTOs.OrderItemDTOs
{
    public class UpdateOrderDTO
    {
        public string PaymentMethod { get; set; } = null!;

        public string BillingAddress { get; set; } = null!;

        public string OrderStatus { get; set; } = null!;
    }
}
