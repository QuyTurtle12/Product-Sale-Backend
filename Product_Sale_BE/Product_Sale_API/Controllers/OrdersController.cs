using BusinessLogic.IServices;
using DataAccess.Constant;
using DataAccess.DTOs.OrderItemDTOs;
using DataAccess.PaginatedList;
using DataAccess.ResponseModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Product_Sale_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : Controller
    {
        private readonly IOrderService _OrderService;

        // Constructor
        public OrdersController(IOrderService OrderService)
        {
            _OrderService = OrderService;
        }

        #region CRUD


        [HttpGet]
        public async Task<IActionResult> GetPaginatedOrdersAsync(int pageIndex = 1, int pageSize = 10, int? idSearch = null, int? orderIdSearch = null, int? userIdSearch = null, 
            string? paymentMethodSearch = null, string? addressSearch = null, string? statusSearch = null, DateTime? orderDateSearch = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            PaginatedList<GetOrderDTO> result = await _OrderService.GetPaginatedOrdersAsync(pageIndex, pageSize, idSearch, orderIdSearch, userIdSearch,
            paymentMethodSearch, addressSearch, statusSearch, orderDateSearch, startDate, endDate);
            return Ok(new BaseResponseModel<PaginatedList<GetOrderDTO>>(
                    statusCode: StatusCodes.Status200OK,
                    code: ResponseCodeConstants.SUCCESS,
                    data: result,
                    message: "Orders retrieved successfully."
                ));
        }

        [HttpPost]
        public async Task<IActionResult> PostOrderAsync(AddOrderDTO OrderDTO)
        {
            if (OrderDTO == null)
            {
                return BadRequest(new BaseResponseModel<string>(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BAD_REQUEST,
                    data: null,
                    message: "Order data is required!"
                ));
            }

            try
            {
                await _OrderService.CreateOrder(OrderDTO);

                return Ok(new BaseResponseModel<string>(
                    statusCode: StatusCodes.Status200OK,
                    code: ResponseCodeConstants.SUCCESS,
                    data: null,
                    message: "Order created successfully."
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
        public async Task<IActionResult> UpdateOrderAsync(int id, [FromBody] UpdateOrderDTO OrderDTO)
        {
            if (OrderDTO == null || id == 0)
            {
                return BadRequest(new BaseResponseModel<string>(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BAD_REQUEST,
                    data: null,
                    message: "Invalid Order data!"
                ));
            }

            try
            {
                await _OrderService.UpdateOrder(id, OrderDTO);

                return Ok(new BaseResponseModel<string>(
                    statusCode: StatusCodes.Status200OK,
                    code: ResponseCodeConstants.SUCCESS,
                    data: null,
                    message: "Order updated successfully."
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
