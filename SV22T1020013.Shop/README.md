# SV22T1020013.Shop - Hướng dẫn cài đặt

## Yêu cầu hệ thống
- .NET 8.0 SDK
- SQL Server 2019+ (hoặc SQL Server Express)
- Visual Studio 2022 hoặc VS Code

## Các bước cài đặt

### Bước 1: Tạo Database
1. Mở SQL Server Management Studio (SSMS)
2. Mở file `Database_Setup.sql`
3. Chạy toàn bộ script để tạo database và dữ liệu mẫu

### Bước 2: Cấu hình Connection String
Mở file `appsettings.json`, chỉnh sửa connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=SV22T1020488Shop;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```
> Thay `Server=.` bằng tên server SQL của bạn nếu cần.

### Bước 3: Restore NuGet packages
```bash
dotnet restore
```

### Bước 4: Chạy ứng dụng
```bash
dotnet run
```
Hoặc mở project trong Visual Studio và nhấn F5.

## Tài khoản demo
- **Email:** customer@example.com
- **Mật khẩu:** 123456

## Chức năng đã xây dựng
1. ✅ Đăng ký tài khoản mới (`/Account/Register`)
2. ✅ Đăng nhập vào hệ thống (`/Account/Login`)
3. ✅ Quản lý thông tin cá nhân và mật khẩu (`/Account/Profile`, `/Account/ChangePassword`)
4. ✅ Xem, tìm kiếm danh mục mặt hàng (`/Product?keyword=...&categoryID=...&minPrice=...&maxPrice=...`)
5. ✅ Xem thông tin chi tiết mặt hàng (`/Product/Detail/{id}`)
6. ✅ Thêm hàng vào giỏ hàng (AJAX, không reload trang)
7. ✅ Quản lý giỏ hàng - cập nhật số lượng, xóa (`/Cart`)
8. ✅ Đặt mua hàng (`/Order/Checkout`)
9. ✅ Theo dõi trạng thái đơn hàng (`/Order/Track/{id}`)
10. ✅ Lịch sử mua hàng (`/Order/History`)

## Cấu trúc project
```
SV22T1020013.Shop/
├── Controllers/
│   ├── BaseController.cs       - Base controller (inject NavCategories)
│   ├── HomeController.cs       - Trang chủ
│   ├── AccountController.cs    - Đăng ký, đăng nhập, profile
│   ├── ProductController.cs    - Danh sách, chi tiết sản phẩm
│   ├── CartController.cs       - Giỏ hàng (AJAX)
│   └── OrderController.cs      - Đặt hàng, lịch sử, theo dõi
├── Models/
│   └── ShopModels.cs           - Tất cả model và ViewModel
├── AppCodes/
│   └── DataAccess.cs           - Repository, DB helper, Cart helper
├── Views/
│   ├── Shared/_Layout.cshtml   - Layout chung
│   ├── Home/Index.cshtml       - Trang chủ
│   ├── Product/                - Index, Detail
│   ├── Cart/                   - Index
│   ├── Order/                  - Checkout, History, Detail, Track
│   └── Account/                - Login, Register, Profile, ChangePassword
├── wwwroot/
│   ├── css/shop.css            - CSS tùy chỉnh
│   └── js/shop.js              - JavaScript (AJAX cart)
├── appsettings.json
├── Program.cs
└── Database_Setup.sql          - Script tạo DB và dữ liệu mẫu
```

## Lưu ý
- Không sử dụng AdminLTE theme
- Giao diện tự thiết kế với Bootstrap 5 + CSS tùy chỉnh
- Giỏ hàng lưu trong Session
- Mật khẩu được hash bằng SHA-256
- Xác thực bằng Cookie Authentication
