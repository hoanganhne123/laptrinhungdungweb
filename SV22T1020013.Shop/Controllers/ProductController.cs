using Microsoft.AspNetCore.Mvc;
using SV22T1020013.Shop.AppCodes;
using SV22T1020013.Shop.Models;

namespace SV22T1020013.Shop.Controllers
{
    public class ProductController : BaseController
    {
        // GET: Product/Index
        public IActionResult Index(string keyword = "", int? categoryID = null,
            decimal? minPrice = null, decimal? maxPrice = null, int page = 1)
        {
            var vm = new ProductSearchViewModel
            {
                Keyword = keyword,
                CategoryID = categoryID,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Page = page,
                PageSize = 12,
                Categories = CategoryRepository.GetAll()
            };

            var (products, totalCount) = ProductRepository.Search(keyword, categoryID, minPrice, maxPrice, page, vm.PageSize);
            vm.Products = products;
            vm.TotalCount = totalCount;

            return View(vm);
        }

        // GET: Product/Detail/5
        public IActionResult Detail(int id)
        {
            var product = ProductRepository.GetByID(id);
            if (product == null) return NotFound();

            ViewBag.RelatedProducts = ProductRepository.GetRelated(id, product.CategoryID);
            return View(product);
        }
    }
}
