﻿using BusinessLogic.IServices;
using DataAccess.Constant;
using DataAccess.DTOs.ProductDTOs;
using DataAccess.PaginatedList;
using DataAccess.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Product_Sale_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Get paginated list of products with optional search filters, sorting and filtering options.
        /// </summary>
        /// <param name="pageIndex">Page number (starts from 1)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="idSearch">Filter by product id</param>
        /// <param name="nameSearch">Filter by product name</param>
        /// <param name="sortBy">Sort by field (price, name, category, brand, rating)</param>
        /// <param name="sortOrder">Sort order (asc, desc)</param>
        /// <param name="categoryId">Filter by category id</param>
        /// <param name="brandId">Filter by brand id</param>
        /// <param name="minPrice">Filter by minimum price</param>
        /// <param name="maxPrice">Filter by maximum price</param>
        /// <param name="minRating">Filter by minimum rating</param>
        /// <returns>Paginated list of products</returns>
        [HttpGet]
        public async Task<IActionResult> GetPaginatedProductsAsync(
            int pageIndex = 1,
            int pageSize = 10,
            int? idSearch = null,
            string? nameSearch = null,
            string? sortBy = null,
            string? sortOrder = null,
            int? categoryId = null,
            int? brandId = null, // Thêm bộ lọc Brand
            decimal? minPrice = null,
            decimal? maxPrice = null,
            decimal? minRating = null) // Thêm bộ lọc Rating
        {
            PaginatedList<GetProductDTO> result = await _productService.GetPaginatedProductsAsync(
                pageIndex,
                pageSize,
                idSearch,
                nameSearch,
                sortBy,
                sortOrder,
                categoryId,
                brandId, // Truyền brandId
                minPrice,
                maxPrice,
                minRating); // Truyền minRating
            return Ok(new BaseResponseModel<PaginatedList<GetProductDTO>>(
                    statusCode: StatusCodes.Status200OK,
                    code: ResponseCodeConstants.SUCCESS,
                    data: result,
                    message: "Products retrieved successfully."
                ));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.getProductDTO(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
    }
}