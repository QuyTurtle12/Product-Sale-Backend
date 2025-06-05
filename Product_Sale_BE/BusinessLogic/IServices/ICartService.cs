using DataAccess.DTOs.CartDTOs;
using DataAccess.DTOs.ProductDTOs;
using DataAccess.PaginatedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.IServices
{
    public interface ICartService
    {
        Task<PaginatedList<GetCartDTO>> GetPaginatedCartsAsync(int pageIndex, int pageSize, int? idSearch, int? userIdSearch, string? statusSearch);
        Task<GetCartDTO> GetCartById(int id);
        Task CreateCart(AddCartDTO cartDTO);
        Task UpdateCart(UpdateCartDTO cartDTO);
        Task DeleteCart(int id);

    }
}
