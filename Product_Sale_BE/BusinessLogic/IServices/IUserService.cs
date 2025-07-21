using DataAccess.DTOs.UserDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.IServices
{
    public interface IUserService
    {
        int GetUserId();
        bool IsTokenValid();
        Task<string> GetUsernameByIdAsync(int userId);

    }
}
