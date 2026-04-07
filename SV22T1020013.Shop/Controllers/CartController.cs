using Microsoft.AspNetCore.Mvc;
using SV22T1020013.Shop.AppCodes;
using SV22T1020013.Shop.Models;

namespace SV22T1020013.Shop.Controllers
{
    public class CartController : BaseController
    {
        // GET: Cart/Index
        public IActionResult Index()
        {
            var cart = CartHelper.GetCart(HttpContext.Session);
            return View(cart);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        public IActionResult AddToCart(int productID, int quantity = 1)
        {
            var product = ProductRepository.GetByID(productID);
            if (product == null || !product.IsSelling)
                return Json(new { success = false, message = "Sản phẩm không tồn tại hoặc đã ngừng bán." });

            var item = new CartItem
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                Price = product.Price,
                Quantity = quantity,
                Photo = product.Photo,
                Unit = product.Unit
            };

            CartHelper.AddToCart(HttpContext.Session, item);
            var count = CartHelper.GetCount(HttpContext.Session);

            return Json(new { success = true, message = $"Đã thêm \"{product.ProductName}\" vào giỏ hàng.", cartCount = count });
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        public IActionResult UpdateQuantity(int productID, int quantity)
        {
            CartHelper.UpdateQuantity(HttpContext.Session, productID, quantity);
            var cart = CartHelper.GetCart(HttpContext.Session);
            var total = cart.Sum(x => x.Subtotal);
            var item = cart.FirstOrDefault(x => x.ProductID == productID);

            return Json(new
            {
                success = true,
                subtotal = item?.Subtotal.ToString("N0") + " ₫",
                total = total.ToString("N0") + " ₫",
                cartCount = CartHelper.GetCount(HttpContext.Session)
            });
        }

        // POST: Cart/RemoveItem
        [HttpPost]
        public IActionResult RemoveItem(int productID)
        {
            CartHelper.RemoveItem(HttpContext.Session, productID);
            var cart = CartHelper.GetCart(HttpContext.Session);
            var total = cart.Sum(x => x.Subtotal);

            return Json(new
            {
                success = true,
                total = total.ToString("N0") + " ₫",
                cartCount = CartHelper.GetCount(HttpContext.Session)
            });
        }

        // POST: Cart/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            CartHelper.ClearCart(HttpContext.Session);
            return RedirectToAction("Index");
        }

        // GET: Cart/Count (API)
        public IActionResult Count()
        {
            return Json(new { count = CartHelper.GetCount(HttpContext.Session) });
        }
    }
}
