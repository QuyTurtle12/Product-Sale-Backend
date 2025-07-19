using AutoMapper;
using BusinessLogic.IServices;
using DataAccess.Constant;
using DataAccess.DTOs.CartDTOs;
using DataAccess.DTOs.CartItemDTOs;
using DataAccess.Entities;
using DataAccess.ExceptionCustom;
using DataAccess.IRepositories;
using DataAccess.PaginatedList;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class CartItemService : ICartItemService
    {
        private readonly IMapper _mapper;
        private readonly IUOW _unitOfWork;

        // Constructor
        public CartItemService(IMapper mapper, IUOW uow)
        {
            _mapper = mapper;
            _unitOfWork = uow;
        }

        public async Task<PaginatedList<GetCartItemDTO>> GetPaginatedCartItemsAsync(int pageIndex, int pageSize, int? idSearch, int? cartIdSearch, int? productIdSearch, int? quantitySearch)
        {
            if (pageIndex < 1 && pageSize < 1)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BAD_REQUEST, "Page index or page size must be greater than or equal to 1.");
            }

            IQueryable<CartItem> query = _unitOfWork.GetRepository<CartItem>().Entities;

            // Apply id search filters if provided
            if (idSearch.HasValue)
            {
                query = query.Where(p => p.CartItemId == idSearch.Value);
            }

            if (cartIdSearch.HasValue)
            {
                query = query.Where(p => p.CartId == cartIdSearch.Value);
            }

            if (productIdSearch.HasValue)
            {
                query = query.Where(p => p.ProductId == productIdSearch.Value);
            }

            if (quantitySearch.HasValue)
            {
                query = query.Where(p => p.Quantity == quantitySearch.Value);
            }


            // Sort the query by CartId
            query = query.OrderByDescending(p => p.CartId);

            // Change to paginated list to facilitate mapping process
            PaginatedList<CartItem> resultQuery = await _unitOfWork.GetRepository<CartItem>()
                .GetPagging(query, pageIndex, pageSize);

            // Map the result to GetCartDTO
            IReadOnlyCollection<GetCartItemDTO> result = resultQuery.Items.Select(item =>
            {
                GetCartItemDTO cartItemDTO = _mapper.Map<GetCartItemDTO>(item);

                return cartItemDTO;
            }).ToList();

            PaginatedList<GetCartItemDTO> paginatedList = new PaginatedList<GetCartItemDTO>(result, resultQuery.TotalCount, resultQuery.PageNumber, resultQuery.PageSize);

            return paginatedList;
        }

        public async Task<GetCartItemDTO> GetCartItemById(int id)
        {
            CartItem? cart = await _unitOfWork.GetRepository<CartItem>().GetByIdAsync(id);
            if (cart == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Cart not found!");
            }
            GetCartItemDTO responseItem = _mapper.Map<GetCartItemDTO>(cart);
            return responseItem;
        }

        public async Task CreateCartItem(AddCartItemDTO cartItemDTO)
        {

            if (cartItemDTO == null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BAD_REQUEST, "Cart Item data is required!");
            }

            if (cartItemDTO.Quantity == 0)
            {
                cartItemDTO.Quantity = 1;
            }

            IGenericRepository<Product> productRepository = _unitOfWork.GetRepository<Product>();
            Product? existingProduct = await productRepository.GetByIdAsync(cartItemDTO.ProductId!);
            if (existingProduct == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.BAD_REQUEST, "Product not found!");
            }

            CartItem cartItem = _mapper.Map<CartItem>(cartItemDTO);
            cartItem.Price = existingProduct.Price;

            await _unitOfWork.GetRepository<CartItem>().InsertAsync(cartItem);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateCartItem(int id, UpdateCartItemDTO cartItemDTO)
        {
            IGenericRepository<CartItem> cartItemRepository = _unitOfWork.GetRepository<CartItem>();
            CartItem? existingCartItem = await cartItemRepository.GetByIdAsync(id);
            if (existingCartItem == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.BAD_REQUEST, "Cart Item not found!");
            }

            if (cartItemDTO.Quantity <= 0)
            {
                cartItemDTO.Quantity = 1;
            }

            if (cartItemDTO.CartId == 0 || cartItemDTO.CartId == null)
            {
                cartItemDTO.CartId = existingCartItem.CartId;
            }

            if (cartItemDTO.ProductId == 0 || cartItemDTO.ProductId == null)
            {
                cartItemDTO.ProductId = existingCartItem.ProductId;
            }

            IGenericRepository<Product> productRepository = _unitOfWork.GetRepository<Product>();
            Product? existingProduct = await productRepository.GetByIdAsync(cartItemDTO.ProductId!);
            if (existingProduct == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.BAD_REQUEST, "Product not found!");
            }

            CartItem cartItem = _mapper.Map(cartItemDTO, existingCartItem);
            cartItem.Price = existingProduct.Price;

            cartItemRepository.Update(existingCartItem);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteCartItem(int id)
        {
            IGenericRepository<CartItem> repository = _unitOfWork.GetRepository<CartItem>();
            CartItem? existingCartItem = await repository.GetByIdAsync(id);
            if (existingCartItem == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.BAD_REQUEST, "Cart Item not found!");
            }

            repository.Delete(existingCartItem);
            await _unitOfWork.SaveAsync();
        }
    }
}
