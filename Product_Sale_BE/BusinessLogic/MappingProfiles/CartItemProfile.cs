using AutoMapper;
using DataAccess.DTOs.CartDTOs;
using DataAccess.DTOs.CartItemDTOs;
using DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.MappingProfiles
{
    public class CartItemProfile : Profile
    {
        public CartItemProfile()
        {
            CreateMap<CartItem, GetCartItemDTO>().ReverseMap();
            CreateMap<CartItem, AddCartItemDTO>().ReverseMap();
            CreateMap<CartItem, UpdateCartItemDTO>().ReverseMap();
        }
    }
}
