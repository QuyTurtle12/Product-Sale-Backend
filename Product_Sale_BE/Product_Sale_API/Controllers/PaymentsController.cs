using BusinessLogic.IServices;
using BusinessLogic.Services;
using DataAccess.Constant;
using DataAccess.DTOs.PaymentDTOs;
using DataAccess.DTOs.PaymentDTOs.VNPay;
using DataAccess.Entities;
using DataAccess.PaginatedList;
using DataAccess.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using VNPAY.NET;
using VNPAY.NET.Enums;
using VNPAY.NET.Utilities;

namespace Product_Sale_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IVnpay _vnpay;
        private readonly TimeZoneInfo vietnamTimeZone;
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly IConfiguration _configuration;


        // Constructor
        public PaymentsController(IPaymentService paymentService, IOrderService orderService, ICartService cartService, IConfiguration configuration, IVnpay vnpay)
        {
            _paymentService = paymentService;
            _vnpay = vnpay;
            vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            _orderService = orderService;
            _cartService = cartService;
            _configuration = configuration;
            _vnpay.Initialize(_configuration["Vnpay:TmnCode"], _configuration["Vnpay:HashSecret"], _configuration["Vnpay:BaseUrl"], _configuration["Vnpay:CallbackUrl"]);

        }

        #region CRUD
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
                    code: ResponseCodeConstants.BAD_REQUEST,
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
                    code: ResponseCodeConstants.BAD_REQUEST,
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
        #endregion

        #region VNPay

        [HttpPost("VNPay/CreatePayment")]
        public async Task<ActionResult<string>> CreatePayment(int orderId)
        {
            try
            {
                var order = await _orderService.GetOrderById(orderId);
                if (order == null)
                {
                    return NotFound(new BaseResponseModel<string>(
                    statusCode: StatusCodes.Status404NotFound,
                    code: ResponseCodeConstants.NOT_FOUND,
                    data: null,
                    message: "Order not found."
                    ));
                }

                var cart = await _cartService.GetCartById((int)order.CartId!);

                var ipAddress = NetworkHelper.GetIpAddress(HttpContext);
                DateTime utcNow = DateTime.UtcNow; // Lấy thời gian hiện tại theo UTC
                DateTime expireTime = utcNow.AddMinutes(15); // Đặt thời gian hết hạn giao dịch (15 phút sau)

                var request = new VNPAY.NET.Models.PaymentRequest
                {
                    PaymentId = orderId,
                    Money = (double)cart.TotalPrice,
                    Description = $"Thanh toán đơn hàng #{orderId}",
                    IpAddress = ipAddress,
                    BankCode = BankCode.ANY,
                    CreatedDate = utcNow, // Thời gian tạo giao dịch UTC
                    Currency = Currency.VND,
                    Language = DisplayLanguage.Vietnamese
                };

                var paymentUrl = _vnpay.GetPaymentUrl(request);

                // return Created(paymentUrl, paymentUrl);
                return Ok(new BaseResponseModel<string>(
                           statusCode: StatusCodes.Status200OK,
                           code: ResponseCodeConstants.SUCCESS,
                           data: paymentUrl,
                           message: "Payment URL created successfully."
               ));
            }
            catch (Exception ex)
            {
                // return BadRequest(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponseModel<string>(
                            statusCode: StatusCodes.Status500InternalServerError,
                            code: ResponseCodeConstants.INTERNAL_SERVER_ERROR,
                            data: null,
                            message: "An unexpected error occurred while creating the payment."
                ));
            }
        }


        [HttpGet("VNPay/IpnAction")]
        public IActionResult IpnAction()
        {
            if (Request.QueryString.HasValue)
            {
                try
                {
                    var paymentResult = _vnpay.GetPaymentResult(Request.Query);
                    if (paymentResult.IsSuccess)
                    {
                        // Thực hiện hành động nếu thanh toán thành công tại đây. Ví dụ: Cập nhật trạng thái đơn hàng trong cơ sở dữ liệu.
                        return Ok();
                    }

                    // Thực hiện hành động nếu thanh toán thất bại tại đây. Ví dụ: Hủy đơn hàng.
                    return BadRequest("Thanh toán thất bại");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            return NotFound("Không tìm thấy thông tin thanh toán.");
        }


        

        [HttpGet("VNPay/Callback")]
        public async Task<IActionResult> Callback()
        {
            try
            {
                var paymentResult = _vnpay.GetPaymentResult(Request.Query);
                if (!paymentResult.IsSuccess)
                {
                    // Chuyển hướng đến trang thất bại nếu thanh toán không thành công
                    return Redirect("https://localhost:7050/swagger/Fail");
                    // return Ok(new{status = "Failed",message = "Payment was not successful"});
                }

                // Extract orderId từ vnp_TxnRef
                if (!int.TryParse(Request.Query["vnp_TxnRef"], out int orderId))
                {
                    return BadRequest("Invalid Order ID");
                }

                // Xử lý lưu thông tin thanh toán và cập nhật đơn hàng (giữ nguyên phần này)
                var paymentDto = new PaymentAPIVNP
                {
                    OrderId = orderId,
                    Amount = decimal.Parse(Request.Query["vnp_Amount"]),
                    BankCode = Request.Query["vnp_BankCode"],
                    BankTranNo = Request.Query["vnp_BankTranNo"],
                    CardType = Request.Query["vnp_CardType"],
                    OrderInfo = Request.Query["vnp_OrderInfo"],
                    PayDate = Request.Query["vnp_PayDate"],
                    ResponseCode = Request.Query["vnp_ResponseCode"],
                    TransactionNo = Request.Query["vnp_TransactionNo"],
                    TransactionStatus = Request.Query["vnp_TransactionStatus"],
                    TxnRef = Request.Query["vnp_TxnRef"],
                    SecureHash = Request.Query["vnp_SecureHash"],
                    CreatedAt = DateTime.UtcNow
                };

                // Lưu thông tin thanh toán và cập nhật đơn hàng (giữ nguyên phần này)
                var paymentEntity = new AddPaymentDTO
                {
                    OrderId = paymentDto.OrderId,
                    PaymentStatus = "Paid",
                };

                await _paymentService.CreatePayment(paymentEntity);

                // Cập nhật trạng thái đơn hàng (giữ nguyên phần này)
                var existingOrder = await _orderService.GetOrderById(paymentDto.OrderId);
                if (existingOrder == null)
                {
                    return BadRequest($"OrderId {paymentDto.OrderId} does not exist.");
                    // return BadRequest(new { status = "fail", message = $"OrderId {orderId} does not exist." });
                }

                // Xử lý OrderStage (giữ nguyên phần này)


                // Chuyển hướng đến trang thành công
                return Redirect("https://localhost:7050/swagger/Success");
               //  return Ok(new{status = "Success",message = "Payment processed successfully",orderId = paymentDto.OrderId});
            }
            catch (Exception ex)
            {
                // Chuyển hướng đến trang lỗi nếu có exception
                return Redirect("https://localhost:7050/swagger/payment-error?message=" + WebUtility.UrlEncode(ex.Message));
                //return StatusCode(500, new
                //{
                //    status = "error",
                //    message = "Internal server error during VNPay callback",
                //    error = ex.Message
                //});
            }
        }

        [HttpGet("payment-status/{orderId}")]
        public async Task<IActionResult> CheckPaymentStatus(int orderId)
        {
            var payment = await _paymentService.GetPaymentByOrderId(orderId);

            if (payment == null)
                return NotFound(new { status = "not_found" });

            return Ok(new
            {
                status = payment.PaymentStatus, // "Pending", "Paid", etc.
                orderId = payment.OrderId
            });
        }


        #endregion


    }
}
