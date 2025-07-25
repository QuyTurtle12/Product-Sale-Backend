﻿using AutoMapper;
using BusinessLogic.IServices;
using DataAccess.Constant;
using DataAccess.DTOs.CartDTOs;
using DataAccess.DTOs.CartItemDTOs;
using DataAccess.DTOs.ProductDTOs;
using DataAccess.Entities;
using DataAccess.ExceptionCustom;
using DataAccess.IRepositories;
using DataAccess.PaginatedList;
using DataAccess.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class CartService : ICartService
    {
        private readonly IMapper _mapper;
        private readonly IUOW _unitOfWork;
        private readonly IUserService _userService;
        private readonly ICartItemService _cartItemService;
        private readonly SalesAppDbContext _salesAppDbContext;


        // Constructor
        public CartService(IMapper mapper, IUOW uow, IUserService userService, ICartItemService cartItemService, SalesAppDbContext salesAppDbContext)
        {
            _mapper = mapper;
            _unitOfWork = uow;
            _userService = userService;
            _cartItemService = cartItemService;
            _salesAppDbContext = salesAppDbContext;
        }

        public async Task<PaginatedList<GetCartDTO>> GetPaginatedCartsAsync(int pageIndex, int pageSize, int? idSearch, int? userIdSearch, string? statusSearch, bool getLatestCart)
        {
            if (pageIndex < 1 && pageSize < 1)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BAD_REQUEST, "Page index or page size must be greater than or equal to 1.");
            }

            IQueryable<Cart> query = _unitOfWork.GetRepository<Cart>().Entities.Include(c => c.CartItems);

            int cartId = 0;
            if (getLatestCart)
            {
                int userId = _userService.GetUserId();

                var cart = await (from c in _unitOfWork.GetRepository<Cart>().Entities.Include(c => c.CartItems)
                                  where c.UserId == userId && (c.Status == "Pending" || c.Status == "Active") &&
                                        !_unitOfWork.GetRepository<Order>().Entities.Any(o => o.CartId == c.CartId)
                                  orderby c.CartId descending
                                  select c).FirstOrDefaultAsync();
                if (cart != null)
                {
                    cartId = cart.CartId;
                }
                else
                {
                    AddCartDTO cartDTO = new AddCartDTO { UserId = userId };
                    await CreateCart(cartDTO);
                    var createdcart = await (from c in _unitOfWork.GetRepository<Cart>().Entities.Include(c => c.CartItems)
                                      where c.UserId == userId && (c.Status == "Pending" || c.Status == "Active") &&
                                            !_unitOfWork.GetRepository<Order>().Entities.Any(o => o.CartId == c.CartId)
                                      orderby c.CartId descending
                                      select c).FirstOrDefaultAsync();
                    if (createdcart != null)
                    {
                        cartId = createdcart.CartId;
                    }
                }

                if (cartId != 0)
                {
                    query = query.Where(p => p.CartId == cartId);
                }
            }


            // Apply id search filters if provided
            if (idSearch.HasValue)
            {
                query = query.Where(p => p.CartId == idSearch.Value);
            }

            if (userIdSearch.HasValue)
            {
                query = query.Where(p => p.UserId == userIdSearch.Value);
            }

            // Apply name search filters if provided
            if (!string.IsNullOrEmpty(statusSearch))
            {
                query = query.Where(p => p.Status.Contains(statusSearch));
            }

            // Sort the query by CartId
            query = query.OrderByDescending(p => p.CartId);

            // Change to paginated list to facilitate mapping process
            PaginatedList<Cart> resultQuery = await _unitOfWork.GetRepository<Cart>()
                .GetPagging(query, pageIndex, pageSize);

            // Map the result to GetCartDTO
            IReadOnlyCollection<GetCartDTO> result = resultQuery.Items.Select(item =>
            {
                GetCartDTO cartDTO = _mapper.Map<GetCartDTO>(item);

                // Loop through each cart item to set Product info manually
                foreach (var cartItem in cartDTO.CartItems!)
                {
                    var product = _unitOfWork.GetRepository<Product>()
                        .Entities
                        .FirstOrDefault(p => p.ProductId == cartItem.ProductId);

                    if (product != null)
                    {
                        cartItem.ProductName = product.ProductName;
                        cartItem.FullDescription = product.FullDescription;
                        cartItem.ImageUrl = product.ProductImages?.FirstOrDefault()?.ImageUrl ?? "No image available"; // No table name ProductImages
                    }
                }

                return cartDTO;
            }).ToList();

            PaginatedList<GetCartDTO> paginatedList = new PaginatedList<GetCartDTO>(result, resultQuery.TotalCount, resultQuery.PageNumber, resultQuery.PageSize);

            return paginatedList;

        }

        public async Task<GetCartDTO> GetCartById(int id)
        {
            Cart? cart = await _unitOfWork.GetRepository<Cart>().GetByIdAsync(id);
            if (cart == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Cart not found!");
            }
            GetCartDTO responseItem = _mapper.Map<GetCartDTO>(cart);
            return responseItem;
        }

        public async Task CreateCart(AddCartDTO cartDTO)
        {

            if (cartDTO == null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BAD_REQUEST, "Cart data is required!");
            }

            if (cartDTO.UserId == 0)
            {
                cartDTO.UserId = null;
            }

            if (_userService.IsTokenValid())
            {
                cartDTO.UserId = _userService.GetUserId();
            }

            Cart cart = _mapper.Map<Cart>(cartDTO);
            cart.Status = "Pending";

            await _unitOfWork.GetRepository<Cart>().InsertAsync(cart);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateCart(int id, UpdateCartDTO cartDTO)
        {
            IGenericRepository<Cart> repository = _unitOfWork.GetRepository<Cart>();
            Cart? existingCart = await repository.GetByIdAsync(id);
            if (existingCart == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.BAD_REQUEST, "Cart not found!");
            }

            // Convert 0 to null
            if (cartDTO.UserId == 0)
            {
                if(existingCart.UserId == null)
                {
                    cartDTO.UserId = null;
                }
                else
                {
                    cartDTO.UserId = existingCart.UserId;
                }
                
            }
            if (_userService.IsTokenValid())
            {
                cartDTO.UserId = _userService.GetUserId();
            }

            _mapper.Map(cartDTO, existingCart);

            // Calculate total price 
            var cartItems = await _cartItemService.GetPaginatedCartItemsAsync(1, 100, null, id, null, null);
            decimal totalPrice = cartItems.Items.Sum(item => item.Price * item.Quantity);
            existingCart.TotalPrice = totalPrice;

            repository.Update(existingCart);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteCart(int id)
        {
            IGenericRepository<Cart> repository = _unitOfWork.GetRepository<Cart>();
            Cart? existingCart = await repository.GetByIdAsync(id);
            if (existingCart == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.BAD_REQUEST, "Cart not found!");
            }

            repository.Delete(existingCart);
            await _unitOfWork.SaveAsync();
        }

        public async Task SoftDeleteCart(int id)
        {
            IGenericRepository<Cart> repository = _unitOfWork.GetRepository<Cart>();
            Cart? existingCart = await repository.GetByIdAsync(id);
            if (existingCart == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.BAD_REQUEST, "Cart not found!");
            }

            existingCart.Status = "Deleted";

            repository.Update(existingCart);
            await _unitOfWork.SaveAsync();
        }

        public async Task<PaginatedList<GetCartDTO>> GetMyCartsAsync(int pageIndex, int pageSize, string? statusSearch)
        {
            int userId = _userService.GetUserId();
            if (pageIndex < 1 && pageSize < 1)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BAD_REQUEST, "Page index or page size must be greater than or equal to 1.");
            }

            IQueryable<Cart> query = _unitOfWork.GetRepository<Cart>().Entities.Include(c => c.CartItems).Where(uid => uid.UserId == userId);

            // Apply name search filters if provided
            if (!string.IsNullOrEmpty(statusSearch))
            {
                query = query.Where(p => p.Status.Contains(statusSearch));
            }

            // Sort the query by CartId
            query = query.OrderByDescending(p => p.CartId);

            // Change to paginated list to facilitate mapping process
            PaginatedList<Cart> resultQuery = await _unitOfWork.GetRepository<Cart>()
                .GetPagging(query, pageIndex, pageSize);

            // Map the result to GetCartDTO
            IReadOnlyCollection<GetCartDTO> result = resultQuery.Items.Select(item =>
            {
                GetCartDTO cartDTO = _mapper.Map<GetCartDTO>(item);

                return cartDTO;
            }).ToList();

            PaginatedList<GetCartDTO> paginatedList = new PaginatedList<GetCartDTO>(result, resultQuery.TotalCount, resultQuery.PageNumber, resultQuery.PageSize);

            return paginatedList;

        }


        public async Task<GetCartDTO?> GetMyLatestAvailableCartAsync()
        {
            int userId = _userService.GetUserId();

            var cart = await (from c in _unitOfWork.GetRepository<Cart>().Entities.Include(c => c.CartItems)
                              where c.UserId == userId &&
                                    !_unitOfWork.GetRepository<Order>().Entities.Any(o => o.CartId == c.CartId)
                              orderby c.CartId descending
                              select c).FirstOrDefaultAsync();

            if (cart == null)
                return null;

            return _mapper.Map<GetCartDTO>(cart);
        }

    }
}
