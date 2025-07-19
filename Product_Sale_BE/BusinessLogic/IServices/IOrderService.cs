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
            string? addressSearch, string? statusSearch, DateTime? orderDateSearch, DateTime? startDate, DateTime? endDate, bool userIdInToken);
        Task<GetOrderDTO> GetOrderById(int id);
        Task<int> CreateOrder(AddOrderDTO OrderDTO);
        Task UpdateOrder(int id, UpdateOrderDTO OrderDTO);
        Task DeleteOrder(int id);
        Task SoftDeleteOrder(int id);
        Task<PaginatedList<GetOrderDTO>> GetMyOrdersAsync(int pageIndex, int pageSize, int? idSearch, int? cartIdSearch,
            string? paymentMethodSearch, string? addressSearch, string? statusSearch, DateTime? orderDateSearch, DateTime? startDate, DateTime? endDate);
    }
}
