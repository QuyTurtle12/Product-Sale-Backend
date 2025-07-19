using DataAccess.DTOs.CartItemDTOs;
using DataAccess.PaginatedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.IServices
{
    public interface ICartItemService
    {
        Task<PaginatedList<GetCartItemDTO>> GetPaginatedCartItemsAsync(int pageIndex, int pageSize, int? idSearch, int? cartIdSearch, int? productIdSearch, int? quantitySearch);
        Task<GetCartItemDTO> GetCartItemById(int id);
        Task CreateCartItem(AddCartItemDTO cartItemDTO);
        Task UpdateCartItem(int id, UpdateCartItemDTO cartItemDTO);
        Task DeleteCartItem(int id);
    }
}
