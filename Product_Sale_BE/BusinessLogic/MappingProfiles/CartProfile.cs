using AutoMapper;
using DataAccess.DTOs.CartDTOs;
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
            CreateMap<Cart, AddCartDTO>().ReverseMap();
            CreateMap<Cart, UpdateCartDTO>().ReverseMap();
        }

    }
}
