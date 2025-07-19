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
            string? sortBy,
            string? sortOrder,
            int? categoryId,
            int? brandId, // Thêm brandId
            decimal? minPrice,
            decimal? maxPrice,
            decimal? minRating); // Thêm minRating

        Task<GetProductDTO> getProductDTO(int productId);
    }
}