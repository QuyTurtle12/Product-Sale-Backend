using BusinessLogic.IServices;
using DataAccess.Constant;
using DataAccess.DTOs.ProductDTOs;
using DataAccess.PaginatedList;
using DataAccess.ResponseModel;
using Microsoft.AspNetCore.Mvc;

namespace Product_Sale_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        // Constructor
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Get paginated list of products with optional search filters.
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="idSearch">product id</param>
        /// <param name="nameSearch">product name</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetPaginatedProductsAsync(int pageIndex = 1, int pageSize = 10, int? idSearch = null, string? nameSearch = null)
        {
            PaginatedList<GetProductDTO> result = await _productService.GetPaginatedProductsAsync(pageIndex, pageSize, idSearch, nameSearch);
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
