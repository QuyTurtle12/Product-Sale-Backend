using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTOs.PaymentDTOs
{
    public class UpdatePaymentDTO
    {
        public decimal Amount { get; set; }

        public string PaymentStatus { get; set; } = null!;

    }
}
