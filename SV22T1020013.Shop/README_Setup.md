# Hướng dẫn cấu hình Shop dùng LiteCommerceDB

## 1. Cấu hình Connection String

Mở file `appsettings.json` và cập nhật server phù hợp:

```json
{
  "ConnectionStrings": {
    "LiteCommerceDB": "Server=TÊN_SERVER;Database=LiteCommerceDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

## 2. Thêm cột Password vào bảng Customers

Vì bảng `Customers` của LiteCommerceDB không có cột `Password`,
cần chạy script SQL sau để thêm cột này (chỉ cần chạy 1 lần):

```sql
-- Thêm cột Password vào Customers
ALTER TABLE Customers
ADD Password NVARCHAR(64) NULL;

-- (Tuỳ chọn) Thêm dữ liệu mẫu để test đăng nhập
-- Password '123456' đã hash SHA256:
UPDATE Customers 
SET Password = '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92'
WHERE Password IS NULL;
```

## 3. Schema dữ liệu Admin đang dùng

| Bảng         | Cột chính |
|---|---|
| Products     | ProductID, ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo, IsSelling |
| Categories   | CategoryID, CategoryName, Description |
| Customers    | CustomerID, CustomerName, ContactName, Province, Address, Phone, Email, IsLocked, **Password** (thêm mới) |
| Orders       | OrderID, CustomerID, OrderTime, DeliveryProvince, DeliveryAddress, EmployeeID, AcceptTime, ShipperID, ShippedTime, FinishedTime, Status |
| OrderDetails | OrderID, ProductID, Quantity, SalePrice |
| Employees    | EmployeeID, FullName, ... |
| Shippers     | ShipperID, ShipperName, Phone, ... |

## 4. Trạng thái đơn hàng (OrderStatusEnum)

| Giá trị | Ý nghĩa |
|---|---|
| -2 | Bị từ chối (Rejected) |
| -1 | Đã hủy (Cancelled) |
| 1  | Chờ xác nhận (New) |
| 2  | Đã xác nhận (Accepted) |
| 3  | Đang giao hàng (Shipping) |
| 4  | Hoàn tất (Completed) |

## 5. Ảnh sản phẩm

Ảnh sản phẩm lấy từ cột `Photo` (tên file), được phục vụ tại đường dẫn:
```
/images/products/{Photo}
```
Cần copy thư mục ảnh của Admin vào `wwwroot/images/products/` của Shop.
