using AutoMapper;
using BusinessLogic.IServices;
using DataAccess.Constant;
using DataAccess.DTOs.ProductDTOs;
using DataAccess.Entities;
using DataAccess.ExceptionCustom;
using DataAccess.IRepositories;
using DataAccess.PaginatedList;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Thêm để log lỗi

namespace BusinessLogic.Services
{
    public class ProductService : IProductService
    {
        private readonly IMapper _mapper;
        private readonly IUOW _unitOfWork;
        private readonly ILogger<ProductService> _logger; // Thêm logger

        public ProductService(IMapper mapper, IUOW uow, ILogger<ProductService> logger)
        {
            _mapper = mapper;
            _unitOfWork = uow;
            _logger = logger; // Khởi tạo logger
        }

        public async Task<PaginatedList<GetProductDTO>> GetPaginatedProductsAsync(
            int pageIndex,
            int pageSize,
            int? idSearch,
            string? nameSearch,
            string? sortBy = null,
            string? sortOrder = null,
            int? categoryId = null,
            int? brandId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            decimal? minRating = null)
        {
            if (pageIndex < 1 || pageSize < 1)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BAD_REQUEST, "Page index or page size must be greater than or equal to 1.");
            }

            IQueryable<Product> query = _unitOfWork.GetRepository<Product>().Entities
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages);

            if (idSearch.HasValue)
            {
                query = query.Where(p => p.ProductId == idSearch.Value);
            }

            if (!string.IsNullOrEmpty(nameSearch))
            {
                query = query.Where(p => p.ProductName.Contains(nameSearch));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (brandId.HasValue)
            {
                query = query.Where(p => p.BrandId == brandId.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            if (minRating.HasValue)
            {
                query = query.Where(p => p.Rating >= minRating.Value);
            }

            if (!string.IsNullOrEmpty(sortBy))
            {
                bool isAscending = string.IsNullOrEmpty(sortOrder) || sortOrder.ToLower() == "asc";

                switch (sortBy.ToLower())
                {
                    case "price":
                        query = isAscending ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price);
                        break;
                    case "name":
                        query = isAscending ? query.OrderBy(p => p.ProductName) : query.OrderByDescending(p => p.ProductName);
                        break;
                    case "category":
                        query = isAscending ? query.OrderBy(p => p.Category.CategoryName) : query.OrderByDescending(p => p.Category.CategoryName);
                        break;
                    case "brand":
                        query = isAscending ? query.OrderBy(p => p.Brand.Name) : query.OrderByDescending(p => p.Brand.Name);
                        break;
                    case "rating":
                        query = isAscending ? query.OrderBy(p => p.Rating) : query.OrderByDescending(p => p.Rating);
                        break;
                    default:
                        query = isAscending ? query.OrderBy(p => p.ProductId) : query.OrderByDescending(p => p.ProductId);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(p => p.ProductId);
            }

            PaginatedList<Product> resultQuery = await _unitOfWork.GetRepository<Product>()
                .GetPagging(query, pageIndex, pageSize);

            IReadOnlyCollection<GetProductDTO> result = resultQuery.Items.Select(item =>
            {
                GetProductDTO productDTO = _mapper.Map<GetProductDTO>(item);
                productDTO.CategoryName = item.Category?.CategoryName ?? string.Empty;
                productDTO.BrandName = item.Brand?.Name ?? string.Empty;
                productDTO.ImageUrls = item.ProductImages?.Select(pi => pi.ImageUrl).ToList() ?? new List<string>();
                return productDTO;
            }).ToList();

            PaginatedList<GetProductDTO> paginatedList = new PaginatedList<GetProductDTO>(result, resultQuery.TotalCount, resultQuery.PageNumber, resultQuery.PageSize);

            return paginatedList;
        }

        public async Task<GetProductDTO> getProductDTO(int productId)
        {
            var productEntity = await _unitOfWork.GetRepository<Product>().Entities
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (productEntity == null)
            {
                return null;
            }

            var productDTO = _mapper.Map<GetProductDTO>(productEntity);
            productDTO.CategoryName = productEntity.Category?.CategoryName ?? string.Empty;
            productDTO.BrandName = productEntity.Brand?.Name ?? string.Empty;
            productDTO.ImageUrls = productEntity.ProductImages?.Select(pi => pi.ImageUrl).ToList() ?? new List<string>();

            return productDTO;
        }
    }
}