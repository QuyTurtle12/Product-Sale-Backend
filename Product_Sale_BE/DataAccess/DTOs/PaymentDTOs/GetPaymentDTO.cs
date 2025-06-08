using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTOs.PaymentDTOs
{
    public class GetPaymentDTO : BasePaymentDTO
    {
        public int PaymentId { get; set; }

        public DateTime PaymentDate { get; set; }

        public decimal Amount { get; set; }

    }
}
