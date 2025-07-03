using AutoMapper;
using BusinessLogic.IServices;
using DataAccess.Constant;
using DataAccess.DTOs.CartDTOs;
using DataAccess.DTOs.CartItemDTOs;
using DataAccess.DTOs.OrderItemDTOs;
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
    public class OrderService : IOrderService
    {
        private readonly IMapper _mapper;
        private readonly IUOW _unitOfWork;
        private readonly IUserService _userService;

        // Constructor
        public OrderService(IMapper mapper, IUOW uow, IUserService userService)
        {
            _mapper = mapper;
            _unitOfWork = uow;
            _userService = userService;
        }

        public async Task<PaginatedList<GetOrderDTO>> GetPaginatedOrdersAsync(int pageIndex, int pageSize, int? idSearch, int? cartIdSearch, int? userIdSearch, 
            string? paymentMethodSearch, string? addressSearch, string? statusSearch, DateTime? orderDateSearch, DateTime? startDate, DateTime? endDate, bool userIdInToken)
        {
            if (pageIndex < 1 && pageSize < 1)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BAD_REQUEST, "Page index or page size must be greater than or equal to 1.");
            }

            IQueryable<Order> query = _unitOfWork.GetRepository<Order>().Entities.Include(u => u.User).Include(cart => cart.Cart)
                .ThenInclude(cartItem => cartItem.CartItems).ThenInclude(p => p.Product); ;

            // Apply id search filters if provided
            if (idSearch.HasValue)
            {
                query = query.Where(p => p.OrderId == idSearch.Value);
            }

            if (cartIdSearch.HasValue)
            {
                query = query.Where(p => p.CartId == cartIdSearch.Value);
            }

            if (userIdSearch.HasValue)
            {
                query = query.Where(p => p.UserId == userIdSearch.Value);
            }

            int userIdFromToken = 0;
            if (userIdInToken)
            {
                userIdFromToken = _userService.GetUserId();

                query = query.Where(p => p.UserId == userIdFromToken);

            }

            // Apply name search filters if provided
            if (!string.IsNullOrEmpty(paymentMethodSearch))
            {
                query = query.Where(p => p.PaymentMethod.Contains(paymentMethodSearch));
            }

            // Apply name search filters if provided
            if (!string.IsNullOrEmpty(addressSearch))
            {
                query = query.Where(p => p.BillingAddress.Contains(addressSearch));
            }

            if (!string.IsNullOrEmpty(statusSearch))
            {
                query = query.Where(p => p.BillingAddress.Contains(statusSearch));
            }

            if (orderDateSearch.HasValue)
            {
                query = query.Where(p => p.OrderDate.Date == orderDateSearch.Value.Date);
            }

            if (startDate.HasValue)
            {
                query = query.Where(p => p.OrderDate.Date >= startDate.Value.Date);
            }
            if (endDate.HasValue)
            {
                query = query.Where(p => p.OrderDate.Date <= endDate.Value.Date);
            }

            // Sort the query by OrderId
            query = query.OrderByDescending(p => p.OrderId);

            // Change to paginated list to facilitate mapping process
            PaginatedList<Order> resultQuery = await _unitOfWork.GetRepository<Order>()
                .GetPagging(query, pageIndex, pageSize);

            // Map the result to GetOrderDTO
            IReadOnlyCollection<GetOrderDTO> result = resultQuery.Items.Select(item =>
            {
                GetOrderDTO orderDTO = _mapper.Map<GetOrderDTO>(item);
                orderDTO.Username = item.User?.Username;
                if (item.Cart != null)
                {
                    orderDTO.Cart = _mapper.Map<GetCartDTO>(item.Cart);

                    if (item.Cart.CartItems != null)
                    {
                        orderDTO.Cart.CartItems = item.Cart.CartItems
                            .Select(ci => _mapper.Map<GetCartItemDTO>(ci))
                            .ToList();
                        
                    }
                }
                return orderDTO;
            }).ToList();

            PaginatedList<GetOrderDTO> paginatedList = new PaginatedList<GetOrderDTO>(result, resultQuery.TotalCount, resultQuery.PageNumber, resultQuery.PageSize);

            return paginatedList;
        }

        public async Task<GetOrderDTO> GetOrderById(int id)
        {
            Order? Order = await _unitOfWork.GetRepository<Order>().GetByIdAsync(id);
            if (Order == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Order not found!");
            }
            GetOrderDTO responseItem = _mapper.Map<GetOrderDTO>(Order);
            return responseItem;
        }

        public async Task<int> CreateOrder(AddOrderDTO OrderDTO)
        {
            if (OrderDTO == null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BAD_REQUEST, "Order data is required!");
            }

            OrderDTO.OrderStatus = "Pending";

            Order Order = _mapper.Map<Order>(OrderDTO);
            Order.OrderDate = DateTime.Now;

            await _unitOfWork.GetRepository<Order>().InsertAsync(Order);
            await _unitOfWork.SaveAsync();

            return Order.OrderId;
        }

        public async Task UpdateOrder(int id, UpdateOrderDTO OrderDTO)
        {
            IGenericRepository<Order> repository = _unitOfWork.GetRepository<Order>();
            Order? existingOrder = await repository.GetByIdAsync(id);
            if (existingOrder == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.BAD_REQUEST, "Order not found!");
            }

            _mapper.Map(OrderDTO, existingOrder);

            repository.Update(existingOrder);
            await _unitOfWork.SaveAsync();
        }

        public Task DeleteOrder(int id)
        {
            throw new NotImplementedException();
        }      

        public Task SoftDeleteOrder(int id)
        {
            throw new NotImplementedException();
        }


        public async Task<PaginatedList<GetOrderDTO>> GetMyOrdersAsync(int pageIndex, int pageSize, int? idSearch, int? cartIdSearch,
            string? paymentMethodSearch, string? addressSearch, string? statusSearch, DateTime? orderDateSearch, DateTime? startDate, DateTime? endDate)
        {
            if (pageIndex < 1 && pageSize < 1)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BAD_REQUEST, "Page index or page size must be greater than or equal to 1.");
            }
            int userId = _userService.GetUserId();
            IQueryable<Order> query = _unitOfWork.GetRepository<Order>().Entities.Include(u => u.User).Include(cart => cart.Cart).ThenInclude(cartItem => cartItem.CartItems).Where(uid => uid.UserId == userId); ;

            // Apply id search filters if provided
            if (idSearch.HasValue)
            {
                query = query.Where(p => p.OrderId == idSearch.Value);
            }

            if (cartIdSearch.HasValue)
            {
                query = query.Where(p => p.CartId == cartIdSearch.Value);
            }


            // Apply name search filters if provided
            if (!string.IsNullOrEmpty(paymentMethodSearch))
            {
                query = query.Where(p => p.PaymentMethod.Contains(paymentMethodSearch));
            }

            // Apply name search filters if provided
            if (!string.IsNullOrEmpty(addressSearch))
            {
                query = query.Where(p => p.BillingAddress.Contains(addressSearch));
            }

            if (!string.IsNullOrEmpty(statusSearch))
            {
                query = query.Where(p => p.BillingAddress.Contains(statusSearch));
            }

            if (orderDateSearch.HasValue)
            {
                query = query.Where(p => p.OrderDate.Date == orderDateSearch.Value.Date);
            }

            if (startDate.HasValue)
            {
                query = query.Where(p => p.OrderDate.Date >= startDate.Value.Date);
            }
            if (endDate.HasValue)
            {
                query = query.Where(p => p.OrderDate.Date <= endDate.Value.Date);
            }

            // Sort the query by OrderId
            query = query.OrderByDescending(p => p.OrderId);

            // Change to paginated list to facilitate mapping process
            PaginatedList<Order> resultQuery = await _unitOfWork.GetRepository<Order>()
                .GetPagging(query, pageIndex, pageSize);

            // Map the result to GetOrderDTO
            IReadOnlyCollection<GetOrderDTO> result = resultQuery.Items.Select(item =>
            {
                GetOrderDTO orderDTO = _mapper.Map<GetOrderDTO>(item);
                orderDTO.Username = item.User?.Username;
                if (item.Cart != null)
                {
                    orderDTO.Cart = _mapper.Map<GetCartDTO>(item.Cart);

                    if (item.Cart.CartItems != null)
                    {
                        orderDTO.Cart.CartItems = item.Cart.CartItems
                            .Select(ci => _mapper.Map<GetCartItemDTO>(ci))
                            .ToList();
                    }
                }
                return orderDTO;
            }).ToList();

            PaginatedList<GetOrderDTO> paginatedList = new PaginatedList<GetOrderDTO>(result, resultQuery.TotalCount, resultQuery.PageNumber, resultQuery.PageSize);

            return paginatedList;
        }

    }
}
