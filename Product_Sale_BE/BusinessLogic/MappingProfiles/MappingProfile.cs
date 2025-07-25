﻿using AutoMapper;
using DataAccess.DTOs.ChatDTOs;
using DataAccess.DTOs.LocationDTOs;
using DataAccess.DTOs.UserDTOs;
using DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.MappingProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDTO>();
            CreateMap<StoreLocation, StoreLocationDTO>();

            CreateMap<ChatMessage, ChatMessageDTO>()
                .ForMember(d => d.Username,o => o.MapFrom(s => s.User!.Username))
                .ForMember(d => d.ChatBoxId,o => o.MapFrom(s => s.ChatBoxId));
        }
    }
}
