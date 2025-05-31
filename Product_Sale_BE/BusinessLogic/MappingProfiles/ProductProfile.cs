using AutoMapper;
using DataAccess.DTOs.ProductDTOs;
using DataAccess.Entities;

namespace BusinessLogic.MappingProfiles
{
    public class ProductProfile : Profile
    {
        public ProductProfile() 
        {
            CreateMap<Product, GetProductDTO>().ReverseMap();
            CreateMap<Product, AddProductDTO>().ReverseMap();
            CreateMap<Product, UpdateProductDTO>().ReverseMap();
        }
    }
}
