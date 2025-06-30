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
    public class CartProfile : Profile
    {
        public CartProfile() 
        {
            CreateMap<Cart, GetCartDTO>().ReverseMap();
            CreateMap<CartItem, GetCartItemDTO>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ProductName : string.Empty))
                .ForMember(dest => dest.FullDescription, opt => opt.MapFrom(src => src.Product != null ? src.Product.FullDescription : null))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Product != null ? src.Product.ProductImages!.FirstOrDefault()!.ImageUrl: null)); ;
            CreateMap<Cart, AddCartDTO>().ReverseMap();
            CreateMap<Cart, UpdateCartDTO>().ReverseMap();
        }

    }
}
