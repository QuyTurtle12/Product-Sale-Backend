using DataAccess.DTOs.OrderItemDTOs;
using DataAccess.PaginatedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.IServices
{
    public interface IOrderService
    {
        Task<PaginatedList<GetOrderDTO>> GetPaginatedOrdersAsync(int pageIndex, int pageSize, int? idSearch, int? cartIdSearch, int? userIdSearch, string? paymentMethodSearch,
            string? addressSearch, string? statusSearch, DateTime? orderDateSearch, DateTime? startDate, DateTime? endDate);
        Task<GetOrderDTO> GetOrderById(int id);
        Task CreateOrder(AddOrderDTO OrderDTO);
        Task UpdateOrder(int id, UpdateOrderDTO OrderDTO);
        Task DeleteOrder(int id);
        Task SoftDeleteOrder(int id);
    }
}
