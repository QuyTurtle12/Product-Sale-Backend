using DataAccess.DTOs.ProductDTOs;
using DataAccess.PaginatedList;

namespace BusinessLogic.IServices
{
    public interface IProductService
    {
        Task<PaginatedList<GetProductDTO>> GetPaginatedProductsAsync(int pageIndex, int pageSize, int? idSearch, string? nameSearch);
        Task<GetProductDTO> getProductDTO(int productId);
    }
}
