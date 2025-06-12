using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace BusinessLogic.Helpers
{
    public static class ControllerExtensions
    {
        public static int GetUserId(this ControllerBase controller)
            => int.Parse(controller.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        public static string GetUserRole(this ControllerBase controller)
            => controller.User.FindFirst(ClaimTypes.Role)!.Value;
    }
}
