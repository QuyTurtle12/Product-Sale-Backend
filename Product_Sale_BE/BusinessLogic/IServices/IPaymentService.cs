using DataAccess.DTOs.PaymentDTOs;
using DataAccess.PaginatedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.IServices
{
    public interface IPaymentService
    {
        Task<PaginatedList<GetPaymentDTO>> GetPaginatedPaymentsAsync(int pageIndex, int pageSize, int? idSearch, int? orderIdSearch, decimal? amountSearch, 
            string? statusSearch, DateTime? paymentDateSearch, DateTime? startDate, DateTime? endDate);
        Task<GetPaymentDTO> GetPaymentById(int id);
        Task CreatePayment(AddPaymentDTO PaymentDTO);
        Task UpdatePayment(int id, UpdatePaymentDTO PaymentDTO);
        Task DeletePayment(int id);
        Task SoftDeletePayment(int id);
    }
}
