// File này được giữ lại nhưng đã xóa các dependency không tồn tại trong project Shop.
// EmployeeController không dùng trong Shop - chỉ dùng ở Admin.
// Để tránh lỗi build, file này chỉ giữ class rỗng.

using Microsoft.AspNetCore.Mvc;

namespace SV22T1020013.Shop.Controllers
{
    // Controller này không dùng trong Shop (chỉ dùng ở Admin)
    // Giữ lại class rỗng để tránh xóa nhầm file
    public class EmployeeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}