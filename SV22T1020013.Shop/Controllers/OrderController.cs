using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020013.Shop.AppCodes;
using SV22T1020013.Shop.Models;

namespace SV22T1020013.Shop.Controllers
{
    [Authorize]
    public class OrderController : BaseController
    {
        // GET: Order/Checkout
        public IActionResult Checkout()
        {
            var cart = CartHelper.GetCart(HttpContext.Session);
            if (!cart.Any())
                return RedirectToAction("Index", "Cart");

            var customerID = int.Parse(User.FindFirst("CustomerID")!.Value);
            var customer = CustomerRepository.GetByID(customerID);

            var vm = new CheckoutViewModel
            {
                ContactName = customer?.ContactName ?? customer?.CustomerName ?? "",
                Phone = customer?.Phone ?? "",
                DeliveryProvince = customer?.Province ?? "",
                DeliveryAddress = customer?.Address ?? "",
                CartItems = cart
            };

            return View(vm);
        }

        // POST: Order/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PlaceOrder(CheckoutViewModel model)
        {
            var cart = CartHelper.GetCart(HttpContext.Session);
            if (!cart.Any())
                return RedirectToAction("Index", "Cart");

            if (string.IsNullOrWhiteSpace(model.ContactName))
                ModelState.AddModelError("ContactName", "Họ tên không được để trống.");
            if (string.IsNullOrWhiteSpace(model.Phone))
                ModelState.AddModelError("Phone", "Số điện thoại không được để trống.");
            if (string.IsNullOrWhiteSpace(model.DeliveryAddress))
                ModelState.AddModelError("DeliveryAddress", "Địa chỉ giao hàng không được để trống.");

            if (!ModelState.IsValid)
            {
                model.CartItems = cart;
                return View("Checkout", model);
            }

            var customerID = int.Parse(User.FindFirst("CustomerID")!.Value);

            var order = new OrderViewInfo
            {
                CustomerID = customerID,
                DeliveryProvince = model.DeliveryProvince,
                DeliveryAddress = model.DeliveryAddress,
                Status = OrderStatusEnum.New,
                OrderTime = DateTime.Now,
                Details = cart.Select(x => new OrderDetailViewInfo
                {
                    ProductID = x.ProductID,
                    SalePrice = x.Price,
                    Quantity = x.Quantity
                }).ToList()
            };

            var orderID = OrderRepository.CreateOrder(order);
            CartHelper.ClearCart(HttpContext.Session);

            TempData["Success"] = $"Đặt hàng thành công! Mã đơn hàng: #{orderID}";
            return RedirectToAction("Detail", new { id = orderID });
        }

        // GET: Order/History
        public IActionResult History()
        {
            var customerID = int.Parse(User.FindFirst("CustomerID")!.Value);
            var orders = OrderRepository.GetByCustomer(customerID);
            return View(orders);
        }

        // GET: Order/Detail/5
        public IActionResult Detail(int id)
        {
            var customerID = int.Parse(User.FindFirst("CustomerID")!.Value);
            var order = OrderRepository.GetByID(id, customerID);
            if (order == null) return NotFound();
            return View(order);
        }

        // GET: Order/Track/5
        public IActionResult Track(int id)
        {
            var customerID = int.Parse(User.FindFirst("CustomerID")!.Value);
            var order = OrderRepository.GetByID(id, customerID);
            if (order == null) return NotFound();
            return View(order);
        }
    }
}
