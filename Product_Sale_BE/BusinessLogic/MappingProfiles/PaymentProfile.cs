using AutoMapper;
using DataAccess.DTOs.PaymentDTOs;
using DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.MappingProfiles
{
    public class PaymentProfile: Profile
    {
        public PaymentProfile()
        {
            CreateMap<Payment, GetPaymentDTO>().ReverseMap();
            CreateMap<Payment, AddPaymentDTO>().ReverseMap();
            CreateMap<Payment, UpdatePaymentDTO>().ReverseMap();
        }
    }
}
