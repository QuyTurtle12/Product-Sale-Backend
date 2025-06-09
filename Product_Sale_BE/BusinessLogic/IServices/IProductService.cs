using DataAccess.DTOs.ProductDTOs;
using DataAccess.PaginatedList;

namespace BusinessLogic.IServices
{
    public interface IProductService
    {
        Task<PaginatedList<GetProductDTO>> GetPaginatedProductsAsync(
            int pageIndex, 
            int pageSize, 
            int? idSearch, 
            string? nameSearch, 
            string? sortBy = null, 
            string? sortOrder = null, 
            int? categoryId = null, 
            decimal? minPrice = null, 
            decimal? maxPrice = null);
            
        Task<GetProductDTO> getProductDTO(int productId);
    }
}
