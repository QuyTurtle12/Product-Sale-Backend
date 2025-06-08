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

namespace BusinessLogic.Services
{
    public class ProductService : IProductService
    {
        private readonly IMapper _mapper;
        private readonly IUOW _unitOfWork;

        // Constructor
        public ProductService(IMapper mapper, IUOW uow)
        {
            _mapper = mapper;
            _unitOfWork = uow;
        }
        public async Task<PaginatedList<GetProductDTO>> GetPaginatedProductsAsync(int pageIndex, int pageSize, int? idSearch, string? nameSearch)
        {
            // Validate page parameters
            if (pageIndex < 1 && pageSize < 1)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BAD_REQUEST, "Page index or page size must be greater than or equal to 1.");
            }

            IQueryable<Product> query = _unitOfWork.GetRepository<Product>().Entities
                                                                        .Include(p => p.Category);

            // Apply id search filters if provided
            if (idSearch.HasValue)
            {
                query = query.Where(p => p.ProductId == idSearch.Value);
            }

            // Apply name search filters if provided
            if (!string.IsNullOrEmpty(nameSearch))
            {
                query = query.Where(p => p.ProductName.Contains(nameSearch));
            }

            // Sort the query by ProductId
            query = query.OrderBy(p => p.ProductId);

            // Change to paginated list to facilitate mapping process
            PaginatedList<Product> resultQuery = await _unitOfWork.GetRepository<Product>()
                .GetPagging(query, pageIndex, pageSize);

            // Map the result to GetProductDTO
            IReadOnlyCollection<GetProductDTO> result = resultQuery.Items.Select(item =>
            {
                GetProductDTO productDTO = _mapper.Map<GetProductDTO>(item);

                productDTO.CategoryName = item.Category?.CategoryName ?? string.Empty;

                return productDTO;
            }).ToList();

            PaginatedList<GetProductDTO> paginatedList = new PaginatedList<GetProductDTO>(result, resultQuery.TotalCount, resultQuery.PageNumber, resultQuery.PageSize);

            return paginatedList;
        }

        public async Task<GetProductDTO> getProductDTO(int productId)
        {
            var product = _mapper.Map<GetProductDTO>(_unitOfWork.GetRepository<Product>().Entities
                .Include(p => p.Category)
                .FirstOrDefault(p => p.ProductId == productId));
            return product;
        }
    }
}
