using Microsoft.AspNetCore.Mvc;

namespace Product_Sale_API.Controllers
{
    public class CartsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
