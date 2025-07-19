using BusinessLogic.IServices;
using BusinessLogic.Services;
using DataAccess.Constant;
using DataAccess.DTOs.CartDTOs;
using DataAccess.DTOs.CartItemDTOs;
using DataAccess.PaginatedList;
using DataAccess.ResponseModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Product_Sale_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartItemsController : Controller
    {
        private readonly ICartItemService _cartItemService;

        // Constructor
        public CartItemsController(ICartItemService cartItemService)
        {
            _cartItemService = cartItemService;
        }

        /// <summary>
        /// Get paginated list of Carts with optional search filters.
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="idSearch">product id</param>
        /// <param name="cartIdSearch">user id</param>
        /// <param name="productIdSearch">status</param>
        /// <param name="quantitySearch">status</param>
        /// <returns></returns>
        //[HttpGet]
        //public async Task<IActionResult> GetPaginatedCartItemsAsync(int pageIndex = 1, int pageSize = 10, int? idSearch = null, int? cartIdSearch = null, int? productIdSearch = null, int? quantitySearch = null)
        //{
        //    PaginatedList<GetCartItemDTO> result = await _cartItemService.GetPaginatedCartItemsAsync(pageIndex, pageSize, idSearch, cartIdSearch, productIdSearch, quantitySearch);
        //    return Ok(new BaseResponseModel<PaginatedList<GetCartItemDTO>>(
        //            statusCode: StatusCodes.Status200OK,
        //            code: ResponseCodeConstants.SUCCESS,
        //            data: result,
        //            message: "Cart items retrieved successfully."
        //        ));
        //}

        [HttpPost]
        public async Task<IActionResult> PostCartItemAsync(AddCartItemDTO cartItemDTO)
        {
            if (cartItemDTO == null)
            {
                return BadRequest(new BaseResponseModel<string>(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BAD_REQUEST,
                data: null,
                    message: "Cart Item data is required!"
                ));
            }

            try
            {
                await _cartItemService.CreateCartItem(cartItemDTO);

                return Ok(new BaseResponseModel<string>(
                    statusCode: StatusCodes.Status200OK,
                    code: ResponseCodeConstants.SUCCESS,
                    data: null,
                    message: "Cart Item created successfully."
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
        public async Task<IActionResult> UpdateCartItemAsync(int id, [FromBody] UpdateCartItemDTO cartItemDTO)
        {
            if (cartItemDTO == null || id == 0)
            {
                return BadRequest(new BaseResponseModel<string>(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BAD_REQUEST,
                data: null,
                    message: "Invalid cart item data!"
                ));
            }

            try
            {
                await _cartItemService.UpdateCartItem(id, cartItemDTO);

                return Ok(new BaseResponseModel<string>(
                    statusCode: StatusCodes.Status200OK,
                    code: ResponseCodeConstants.SUCCESS,
                    data: null,
                    message: "Cart Item updated successfully."
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
        public async Task<IActionResult> DeleteCartItemAsync(int id)
        {
            try
            {
                await _cartItemService.DeleteCartItem(id);

                return Ok(new BaseResponseModel<string>(
                    statusCode: StatusCodes.Status200OK,
                    code: ResponseCodeConstants.SUCCESS,
                    data: null,
                    message: "Cart Item deleted successfully."
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



    }
}
