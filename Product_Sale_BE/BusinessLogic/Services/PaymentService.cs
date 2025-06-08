using AutoMapper;
using BusinessLogic.IServices;
using DataAccess.Constant;
using DataAccess.DTOs.PaymentDTOs;
using DataAccess.DTOs.PaymentDTOs;
using DataAccess.DTOs.PaymentDTOs;
using DataAccess.Entities;
using DataAccess.ExceptionCustom;
using DataAccess.IRepositories;
using DataAccess.PaginatedList;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IMapper _mapper;
        private readonly IUOW _unitOfWork;

        // Constructor
        public PaymentService(IMapper mapper, IUOW uow)
        {
            _mapper = mapper;
            _unitOfWork = uow;
        }
        public async Task<PaginatedList<GetPaymentDTO>> GetPaginatedPaymentsAsync(int pageIndex, int pageSize, int? idSearch, int? orderIdSearch, decimal? amountSearch, 
            string? statusSearch, DateTime? paymentDateSearch, DateTime? startDate, DateTime? endDate)
        {
            if (pageIndex < 1 && pageSize < 1)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BAD_REQUEST, "Page index or page size must be greater than or equal to 1.");
            }

            IQueryable<Payment> query = _unitOfWork.GetRepository<Payment>().Entities;

            // Apply id search filters if provided
            if (idSearch.HasValue)
            {
                query = query.Where(p => p.PaymentId == idSearch.Value);
            }

            if (orderIdSearch.HasValue)
            {
                query = query.Where(p => p.OrderId == orderIdSearch.Value);
            }

            if (amountSearch.HasValue)
                query = query.Where(p => p.Amount <= amountSearch.Value && p.Amount >= 0);

            // Apply name search filters if provided
            if (!string.IsNullOrEmpty(statusSearch))
            {
                query = query.Where(p => p.PaymentStatus.Contains(statusSearch));
            }

            if (paymentDateSearch.HasValue)
            {
                var targetDate = paymentDateSearch.Value.Date;
                query = query.Where(p => p.PaymentDate.Date == targetDate);
            }

            // 🔍 Date range filter
            if (startDate.HasValue)
                query = query.Where(p => p.PaymentDate.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(p => p.PaymentDate.Date <= endDate.Value.Date);

            // Sort the query by PaymentId
            query = query.OrderByDescending(p => p.PaymentId);

            // Change to paginated list to facilitate mapping process
            PaginatedList<Payment> resultQuery = await _unitOfWork.GetRepository<Payment>()
                .GetPagging(query, pageIndex, pageSize);

            // Map the result to GetPaymentDTO
            IReadOnlyCollection<GetPaymentDTO> result = resultQuery.Items.Select(item =>
            {
                GetPaymentDTO PaymentDTO = _mapper.Map<GetPaymentDTO>(item);

                return PaymentDTO;
            }).ToList();

            PaginatedList<GetPaymentDTO> paginatedList = new PaginatedList<GetPaymentDTO>(result, resultQuery.TotalCount, resultQuery.PageNumber, resultQuery.PageSize);

            return paginatedList;
        }

        public async Task<GetPaymentDTO> GetPaymentById(int id)
        {
            Payment? Payment = await _unitOfWork.GetRepository<Payment>().GetByIdAsync(id);
            if (Payment == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Payment not found!");
            }
            GetPaymentDTO responseItem = _mapper.Map<GetPaymentDTO>(Payment);
            return responseItem;
        }

        public async Task CreatePayment(AddPaymentDTO PaymentDTO)
        {
            if (PaymentDTO == null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BAD_REQUEST, "Payment data is required!");
            }

            Payment Payment = _mapper.Map<Payment>(PaymentDTO);
            Payment.PaymentDate = DateTime.Now;

            if (!PaymentDTO.OrderId.HasValue)
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BAD_REQUEST, "Order ID is required.");
            Payment.Amount = await GetPaymentTotalFromPaymentAsync(PaymentDTO.OrderId.Value);

            await _unitOfWork.GetRepository<Payment>().InsertAsync(Payment);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdatePayment(int id, UpdatePaymentDTO PaymentDTO)
        {
            IGenericRepository<Payment> repository = _unitOfWork.GetRepository<Payment>();
            Payment? existingPayment = await repository.GetByIdAsync(id);
            if (existingPayment == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.BAD_REQUEST, "Payment not found!");
            }

            _mapper.Map(PaymentDTO, existingPayment);

            repository.Update(existingPayment);
            await _unitOfWork.SaveAsync();
        }

        public Task DeletePayment(int id)
        {
            throw new NotImplementedException();
        }

        public async Task SoftDeletePayment(int id)
        {
            IGenericRepository<Payment> repository = _unitOfWork.GetRepository<Payment>();
            Payment? existingPayment = await repository.GetByIdAsync(id);
            if (existingPayment == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.BAD_REQUEST, "Payment not found!");
            }

            existingPayment.PaymentStatus = "Cancel";

            repository.Update(existingPayment);
            await _unitOfWork.SaveAsync();
        }

        

        public async Task<decimal> GetPaymentTotalFromPaymentAsync(int paymentId)
        {
            // Get the payment
            var payment = await _unitOfWork.GetRepository<Payment>().Entities
                .Where(p => p.PaymentId == paymentId)
                .FirstOrDefaultAsync();

            if (payment == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Payment not found.");

            // Get the related order
            var order = await _unitOfWork.GetRepository<Order>().Entities
                .Where(o => o.OrderId == payment.OrderId)
                .FirstOrDefaultAsync();

            if (order == null)
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Order not found.");

            // Get all items in the related cart
            var cartItems = _unitOfWork.GetRepository<CartItem>().Entities
                .Where(ci => ci.CartId == order.CartId);

            // Sum total (Quantity * Price)
            var cartTotal = await cartItems
                .SumAsync(ci => ci.Quantity * ci.Price);

            return cartTotal;
        }

    }
}
