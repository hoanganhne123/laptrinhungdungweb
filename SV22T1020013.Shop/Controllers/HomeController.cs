using Microsoft.AspNetCore.Mvc;
using SV22T1020013.Shop.AppCodes;

namespace SV22T1020013.Shop.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            ViewBag.FeaturedProducts = ProductRepository.GetFeatured(8);
            ViewBag.Categories = CategoryRepository.GetAll();
            return View();
        }
    }
}
