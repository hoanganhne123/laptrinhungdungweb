using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SV22T1020013.Shop.AppCodes;
using SV22T1020013.Shop.Models;
using System.Security.Claims;

namespace SV22T1020013.Shop.Controllers
{
    public class AccountController : Controller
    {
        // ── ĐĂNG NHẬP ────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ email và mật khẩu");
                return View(model);
            }

            var customer = CustomerRepository.GetByEmail(model.Email.Trim());

            if (customer == null || !PasswordHelper.Verify(model.Password, customer.Password ?? ""))
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
                return View(model);
            }

            // ✅ Lưu claim "CustomerID" - OrderController và Profile cần claim này
            var claims = new List<Claim>
            {
                new Claim("CustomerID",              customer.CustomerID.ToString()),
                new Claim(ClaimTypes.NameIdentifier, customer.CustomerID.ToString()),
                new Claim(ClaimTypes.Name,           customer.CustomerName ?? ""),
                new Claim(ClaimTypes.Email,          customer.Email ?? "")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var properties = new AuthenticationProperties { IsPersistent = model.RememberMe };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);
            return RedirectToAction("Index", "Home");
        }

        // ── ĐĂNG KÝ ──────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Register()
        {
            try { ViewBag.Provinces = ProvinceRepository.GetAll(); }
            catch { ViewBag.Provinces = new List<string>(); }
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            try { ViewBag.Provinces = ProvinceRepository.GetAll(); }
            catch { ViewBag.Provinces = new List<string>(); }

            if (string.IsNullOrWhiteSpace(model.CustomerName))
                ModelState.AddModelError("CustomerName", "Vui lòng nhập họ tên");
            if (string.IsNullOrWhiteSpace(model.Email))
                ModelState.AddModelError("Email", "Vui lòng nhập email");
            if (string.IsNullOrWhiteSpace(model.Password) || model.Password.Length < 6)
                ModelState.AddModelError("Password", "Mật khẩu phải có ít nhất 6 ký tự");
            if (model.Password != model.ConfirmPassword)
                ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp");

            if (!ModelState.IsValid)
                return View(model);

            var customer = new Customer
            {
                CustomerName = model.CustomerName.Trim(),
                ContactName = string.IsNullOrWhiteSpace(model.ContactName)
                                   ? model.CustomerName.Trim()
                                   : model.ContactName.Trim(),
                Email = model.Email.Trim(),
                Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
                Province = string.IsNullOrWhiteSpace(model.Province) ? null : model.Province.Trim(),
                Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim(),
                Password = PasswordHelper.Hash(model.Password)
            };

            try
            {
                CustomerRepository.Register(customer);
                TempData["SuccessMsg"] = "Đăng ký thành công! Hãy đăng nhập.";
                return RedirectToAction("Login");
            }
            catch (InvalidOperationException ex)
            {
                string msg = ex.Message switch
                {
                    "EMAIL_EXISTS" => "Email này đã được đăng ký.",
                    "PHONE_EXISTS" => "Số điện thoại đã được sử dụng.",
                    _ => "Lỗi cơ sở dữ liệu, vui lòng thử lại."
                };
                ModelState.AddModelError("", msg);
                return View(model);
            }
        }

        // ── HỒ SƠ CÁ NHÂN ────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Profile()
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToAction("Login");

            var customerID = int.Parse(User.FindFirst("CustomerID")!.Value);
            var customer = CustomerRepository.GetByID(customerID);
            if (customer == null) return RedirectToAction("Logout");

            try { ViewBag.Provinces = ProvinceRepository.GetAll(); }
            catch { ViewBag.Provinces = new List<string>(); }

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(Customer model)
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToAction("Login");

            try { ViewBag.Provinces = ProvinceRepository.GetAll(); }
            catch { ViewBag.Provinces = new List<string>(); }

            var customerID = int.Parse(User.FindFirst("CustomerID")!.Value);
            model.CustomerID = customerID;

            if (string.IsNullOrWhiteSpace(model.CustomerName))
                ModelState.AddModelError("CustomerName", "Họ tên không được để trống");
            if (string.IsNullOrWhiteSpace(model.ContactName))
                ModelState.AddModelError("ContactName", "Tên giao dịch không được để trống");

            if (!ModelState.IsValid)
                return View(model);

            // Giữ nguyên Email & Password - form không gửi 2 field này
            var existing = CustomerRepository.GetByID(customerID);
            model.Email = existing?.Email ?? "";
            model.Password = existing?.Password ?? "";

            bool ok = CustomerRepository.Update(model);
            if (ok)
                TempData["Success"] = "Cập nhật thông tin thành công!";
            else
                ModelState.AddModelError("", "Cập nhật thất bại, vui lòng thử lại.");

            return View(model);
        }

        // ── ĐĂNG XUẤT ────────────────────────────────────────────────────────

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // ── ĐỔI MẬT KHẨU ─────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToAction("Login");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                ModelState.AddModelError("newPassword", "Mật khẩu mới phải có ít nhất 6 ký tự");
                return View();
            }
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("confirmPassword", "Mật khẩu xác nhận không khớp");
                return View();
            }

            var customerID = int.Parse(User.FindFirst("CustomerID")!.Value);
            var customer = CustomerRepository.GetByID(customerID);

            if (customer == null || !PasswordHelper.Verify(oldPassword, customer.Password ?? ""))
            {
                ModelState.AddModelError("oldPassword", "Mật khẩu cũ không đúng");
                return View();
            }

            // Dùng UpdatePassword riêng, không đụng các field khác
            CustomerRepository.UpdatePassword(customerID, PasswordHelper.Hash(newPassword));

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMsg"] = "Đổi mật khẩu thành công! Vui lòng đăng nhập lại.";
            return RedirectToAction("Login");
        }
    }
}