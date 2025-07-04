using AutoMapper;
using BusinessLogic.IServices;
using DataAccess.Constant;
using DataAccess.DTOs.LocationDTOs;
using DataAccess.Entities;
using DataAccess.ExceptionCustom;
using DataAccess.IRepositories;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class StoreLocationService : IStoreLocationService
    {
        private readonly IUOW _unitOfWork;
        private readonly IMapper _mapper;

        public StoreLocationService(IUOW unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<StoreLocationDTO>> GetAllAsync()
        {
            var repo = _unitOfWork.GetRepository<StoreLocation>();
            var locations = repo.Entities.ToList();  

            if (!locations.Any())
            {
                throw new ErrorException(
                    StatusCodes.Status404NotFound,
                    ResponseCodeConstants.NOT_FOUND,
                    "No store locations found.");
            }

            var dtos = _mapper.Map<IEnumerable<StoreLocationDTO>>(locations);
            return await Task.FromResult(dtos);
        }
    }
}
