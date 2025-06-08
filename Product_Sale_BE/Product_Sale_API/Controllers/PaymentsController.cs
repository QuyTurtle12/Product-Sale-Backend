using BusinessLogic.IServices;
using BusinessLogic.Services;
using DataAccess.Constant;
using DataAccess.DTOs.PaymentDTOs;
using DataAccess.DTOs.PaymentDTOs;
using DataAccess.PaginatedList;
using DataAccess.ResponseModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Product_Sale_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        // Constructor
        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPaginatedPaymentsAsync(int pageIndex = 1, int pageSize = 10, int? idSearch = null, int? orderIdSearch = null,
            decimal? amountSearch = null, string? statusSearch = null, DateTime? paymentDateSearch = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            PaginatedList<GetPaymentDTO> result = await _paymentService.GetPaginatedPaymentsAsync(pageIndex, pageSize, idSearch, orderIdSearch, amountSearch, statusSearch,
                paymentDateSearch, startDate, endDate);
            return Ok(new BaseResponseModel<PaginatedList<GetPaymentDTO>>(
                    statusCode: StatusCodes.Status200OK,
                    code: ResponseCodeConstants.SUCCESS,
                    data: result,
                    message: "Payments retrieved successfully."
                ));
        }

        [HttpPost]
        public async Task<IActionResult> PostPaymentAsync(AddPaymentDTO PaymentDTO)
        {
            if (PaymentDTO == null)
            {
                return BadRequest(new BaseResponseModel<string>(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BADREQUEST,
                data: null,
                    message: "Payment data is required!"
                ));
            }

            try
            {
                await _paymentService.CreatePayment(PaymentDTO);

                return Ok(new BaseResponseModel<string>(
                    statusCode: StatusCodes.Status200OK,
                    code: ResponseCodeConstants.SUCCESS,
                    data: null,
                    message: "Payment created successfully."
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
        public async Task<IActionResult> UpdatePaymentAsync(int id, [FromBody] UpdatePaymentDTO PaymentDTO)
        {
            if (PaymentDTO == null || id == 0)
            {
                return BadRequest(new BaseResponseModel<string>(
                    statusCode: StatusCodes.Status400BadRequest,
                    code: ResponseCodeConstants.BADREQUEST,
                    data: null,
                    message: "Invalid Payment data!"
                ));
            }

            try
            {
                await _paymentService.UpdatePayment(id, PaymentDTO);

                return Ok(new BaseResponseModel<string>(
                    statusCode: StatusCodes.Status200OK,
                    code: ResponseCodeConstants.SUCCESS,
                    data: null,
                    message: "Payment updated successfully."
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
        public async Task<IActionResult> SoftDeletePaymentAsync(int id)
        {
            try
            {
                await _paymentService.SoftDeletePayment(id);

                return Ok(new BaseResponseModel<string>(
                    statusCode: StatusCodes.Status200OK,
                    code: ResponseCodeConstants.SUCCESS,
                    data: null,
                    message: "Payment deleted successfully."
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
