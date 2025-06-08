using BusinessLogic.IServices;
using DataAccess.Constant;
using DataAccess.DTOs.CartDTOs;
using DataAccess.DTOs.ProductDTOs;
using DataAccess.ExceptionCustom;
using DataAccess.PaginatedList;
using DataAccess.ResponseModel;
using Microsoft.AspNetCore.Mvc;

namespace Product_Sale_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : Controller
    {
        private readonly ICartService _cartService;

        // Constructor
        public CartsController(ICartService cartService)
        {
            _cartService = cartService;
        }

        #region CRUD

        /// <summary>
        /// Get paginated list of Carts with optional search filters.
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="idSearch">product id</param>
        /// <param name="userIdSearch">user id</param>
        /// <param name="statusSearch">status</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetPaginatedCartsAsync(int pageIndex = 1, int pageSize = 10, int? idSearch = null, int? userIdSearch = null, string? statusSearch = null)
        {
            PaginatedList<GetCartDTO> result = await _cartService.GetPaginatedCartsAsync(pageIndex, pageSize, idSearch, userIdSearch, statusSearch);
            return Ok(new BaseResponseModel<PaginatedList<GetCartDTO>>(
                    statusCode: StatusCodes.Status200OK,
                    code: ResponseCodeConstants.SUCCESS,
                    data: result,
                    message: "Carts retrieved successfully."
                ));
        }

        [HttpPost]
        public async Task<IActionResult> PostCartAsync(AddCartDTO cartDTO)
        {
            if (cartDTO == null)
            {
                return BadRequest(new BaseResponseModel<string>(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BADREQUEST,
                    data: null,
                    message: "Cart data is required!"
                ));
            }

            try
            {
                await _cartService.CreateCart(cartDTO);

                return Ok(new BaseResponseModel<string>(
                    statusCode: StatusCodes.Status200OK,
                    code: ResponseCodeConstants.SUCCESS,
                    data: null,
                    message: "Cart created successfully."
                ));
            }
            catch (Exception ex)
            {
                // Optional: Log the error
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseModel<string>(
                    statusCode: StatusCodes.Status500InternalServerError,
                    code: ResponseCodeConstants.INTERNAL_SERVER_ERROR,
                    data: null,
                    message: "An unexpected error occurred."
                ));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCartAsync(int id, [FromBody] UpdateCartDTO cartDTO)
        {
            if (cartDTO == null || id == 0)
            {
                return BadRequest(new BaseResponseModel<string>(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BADREQUEST,
                    data: null,
                    message: "Invalid cart data!"
                ));
            }

            try
            {
                await _cartService.UpdateCart(id, cartDTO);

                return Ok(new BaseResponseModel<string>(
                    statusCode: StatusCodes.Status200OK,
                    code: ResponseCodeConstants.SUCCESS,
                    data: null,
                    message: "Cart updated successfully."
                ));
            }
            catch (Exception ex)
            {
                // Optional: Log the error
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseModel<string>(
                    statusCode: StatusCodes.Status500InternalServerError,
                    code: ResponseCodeConstants.INTERNAL_SERVER_ERROR,
                    data: null,
                    message: "An unexpected error occurred."
                ));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCartAsync(int id)
        {
            try
            {
                await _cartService.DeleteCart(id);

                return Ok(new BaseResponseModel<string>(
                    statusCode: StatusCodes.Status200OK,
                    code: ResponseCodeConstants.SUCCESS,
                    data: null,
                    message: "Cart deleted successfully."
                ));
            }
            catch (Exception ex)
            {
                // Optional: Log the error
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseModel<string>(
                    statusCode: StatusCodes.Status500InternalServerError,
                    code: ResponseCodeConstants.INTERNAL_SERVER_ERROR,
                    data: null,
                    message: "An unexpected error occurred."
                ));
            }
        }

        [HttpDelete("soft-delete/{id}")]
        public async Task<IActionResult> SoftDeleteCartAsync(int id)
        {
            try
            {
                await _cartService.SoftDeleteCart(id);

                return Ok(new BaseResponseModel<string>(
                    statusCode: StatusCodes.Status200OK,
                    code: ResponseCodeConstants.SUCCESS,
                    data: null,
                    message: "Cart deleted successfully."
                ));
            }
            catch (Exception ex)
            {
                // Optional: Log the error
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseModel<string>(
                    statusCode: StatusCodes.Status500InternalServerError,
                    code: ResponseCodeConstants.INTERNAL_SERVER_ERROR,
                    data: null,
                    message: "An unexpected error occurred."
                ));
            }
        }

        #endregion 

    }
}
