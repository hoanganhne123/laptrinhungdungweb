using Microsoft.Data.SqlClient;
using Dapper;
using SV22T1020013.Shop.Models;

namespace SV22T1020013.Shop.AppCodes
{
    public class DatabaseHelper
    {
        private static string _connectionString = "";

        public static void Initialize(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("LiteCommerceDB") ?? "";
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }

    // ===================== CATEGORY =====================
    public class CategoryRepository
    {
        public static List<Category> GetAll()
        {
            using var conn = DatabaseHelper.GetConnection();
            return conn.Query<Category>("SELECT * FROM Categories ORDER BY CategoryName").ToList();
        }
    }

    // ===================== PRODUCT =====================
    public class ProductRepository
    {
        public static (List<Product> Products, int TotalCount) Search(
            string keyword = "", int? categoryID = null,
            decimal? minPrice = null, decimal? maxPrice = null,
            int page = 1, int pageSize = 12)
        {
            using var conn = DatabaseHelper.GetConnection();
            var conditions = new List<string> { "p.IsSelling = 1" };
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(keyword))
            {
                conditions.Add("(p.ProductName LIKE @Keyword OR p.ProductDescription LIKE @Keyword)");
                parameters.Add("Keyword", $"%{keyword}%");
            }
            if (categoryID.HasValue)
            {
                conditions.Add("p.CategoryID = @CategoryID");
                parameters.Add("CategoryID", categoryID.Value);
            }
            if (minPrice.HasValue)
            {
                conditions.Add("p.Price >= @MinPrice");
                parameters.Add("MinPrice", minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                conditions.Add("p.Price <= @MaxPrice");
                parameters.Add("MaxPrice", maxPrice.Value);
            }

            var where = "WHERE " + string.Join(" AND ", conditions);
            var countSql = $"SELECT COUNT(*) FROM Products p {where}";
            var totalCount = conn.ExecuteScalar<int>(countSql, parameters);

            parameters.Add("Offset", (page - 1) * pageSize);
            parameters.Add("PageSize", pageSize);

            var sql = $@"SELECT p.*, c.CategoryName FROM Products p
                         LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                         {where}
                         ORDER BY p.ProductID DESC
                         OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var products = conn.Query<Product>(sql, parameters).ToList();
            return (products, totalCount);
        }

        public static Product? GetByID(int id)
        {
            using var conn = DatabaseHelper.GetConnection();
            return conn.QueryFirstOrDefault<Product>(
                @"SELECT p.*, c.CategoryName FROM Products p
                  LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                  WHERE p.ProductID = @ID",
                new { ID = id });
        }

        public static List<Product> GetRelated(int productID, int? categoryID, int count = 4)
        {
            using var conn = DatabaseHelper.GetConnection();
            return conn.Query<Product>(
                @"SELECT TOP (@Count) p.*, c.CategoryName FROM Products p
                  LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                  WHERE p.CategoryID = @CategoryID AND p.ProductID != @ProductID AND p.IsSelling = 1
                  ORDER BY NEWID()",
                new { Count = count, CategoryID = categoryID ?? 0, ProductID = productID }).ToList();
        }

        public static List<Product> GetFeatured(int count = 8)
        {
            using var conn = DatabaseHelper.GetConnection();
            return conn.Query<Product>(
                @"SELECT TOP (@Count) p.*, c.CategoryName FROM Products p
                  LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                  WHERE p.IsSelling = 1
                  ORDER BY p.ProductID DESC",
                new { Count = count }).ToList();
        }
    }

    // ===================== PROVINCE =====================
    public class ProvinceRepository
    {
        public static List<string> GetAll()
        {
            using var conn = DatabaseHelper.GetConnection();
            return conn.Query<string>("SELECT ProvinceName FROM Provinces ORDER BY ProvinceName").ToList();
        }
    }

    // ===================== CUSTOMER =====================
    public class CustomerRepository
    {
        public static Customer? GetByEmail(string email)
        {
            using var conn = DatabaseHelper.GetConnection();
            return conn.QueryFirstOrDefault<Customer>(
                "SELECT * FROM Customers WHERE Email = @Email AND IsLocked = 0",
                new { Email = email });
        }

        public static Customer? GetByID(int id)
        {
            using var conn = DatabaseHelper.GetConnection();
            return conn.QueryFirstOrDefault<Customer>(
                "SELECT * FROM Customers WHERE CustomerID = @ID",
                new { ID = id });
        }

        public static bool EmailExists(string email, int excludeID = 0)
        {
            using var conn = DatabaseHelper.GetConnection();
            var count = conn.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM Customers WHERE Email = @Email AND CustomerID <> @ExcludeID",
                new { Email = email, ExcludeID = excludeID });
            return count > 0;
        }

        public static bool PhoneExists(string? phone, int excludeID = 0)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;
            using var conn = DatabaseHelper.GetConnection();
            var count = conn.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM Customers WHERE Phone = @Phone AND CustomerID <> @ExcludeID",
                new { Phone = phone.Trim(), ExcludeID = excludeID });
            return count > 0;
        }

        public static int Register(Customer customer)
        {
            // Chuẩn hóa dữ liệu
            customer.Email = customer.Email?.Trim() ?? "";
            customer.Phone = string.IsNullOrWhiteSpace(customer.Phone) ? null : customer.Phone.Trim();
            customer.Province = string.IsNullOrWhiteSpace(customer.Province) ? null : customer.Province.Trim();
            customer.Address = string.IsNullOrWhiteSpace(customer.Address) ? null : customer.Address.Trim();

            // Kiểm tra Email/Phone trùng
            if (EmailExists(customer.Email))
                throw new InvalidOperationException("EMAIL_EXISTS");
            if (!string.IsNullOrWhiteSpace(customer.Phone) && PhoneExists(customer.Phone))
                throw new InvalidOperationException("PHONE_EXISTS");

            // Kiểm tra Province có tồn tại trong DB không, nếu không hợp lệ thì set null
            if (!string.IsNullOrWhiteSpace(customer.Province))
            {
                using var connP = DatabaseHelper.GetConnection();
                int provinceCount = connP.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM Provinces WHERE ProvinceName = @p",
                    new { p = customer.Province });
                if (provinceCount == 0)
                    customer.Province = null;
            }

            using var conn = DatabaseHelper.GetConnection();
            var sql = @"INSERT INTO Customers(CustomerName, ContactName, Province, Address, Phone, Email, IsLocked, Password)
                        VALUES(@CustomerName, @ContactName, @Province, @Address, @Phone, @Email, 0, @Password);
                        SELECT SCOPE_IDENTITY();";
            try
            {
                return conn.ExecuteScalar<int>(sql, customer);
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                // Lỗi UNIQUE (Email/Phone trùng)
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    if (ex.Message.ToLower().Contains("email"))
                        throw new InvalidOperationException("EMAIL_EXISTS");
                    throw new InvalidOperationException("PHONE_EXISTS");
                }
                // Lỗi FK (Province không hợp lệ) hoặc bất kỳ lỗi SQL nào khác
                throw new InvalidOperationException("DB_ERROR");
            }
        }

        public static bool Update(Customer customer)
        {
            using var conn = DatabaseHelper.GetConnection();
            var sql = @"UPDATE Customers 
                        SET CustomerName=@CustomerName, ContactName=@ContactName,
                            Province=@Province, Address=@Address, Phone=@Phone
                        WHERE CustomerID=@CustomerID";
            return conn.Execute(sql, customer) > 0;
        }

        // Cập nhật riêng mật khẩu (dùng khi đổi mật khẩu)
        public static bool UpdatePassword(int customerID, string hashedPassword)
        {
            using var conn = DatabaseHelper.GetConnection();
            return conn.Execute(
                "UPDATE Customers SET Password=@Password WHERE CustomerID=@CustomerID",
                new { Password = hashedPassword, CustomerID = customerID }) > 0;
        }
    }

    // ===================== ORDER =====================
    public class OrderRepository
    {
        public static int CreateOrder(OrderViewInfo order)
        {
            using var conn = DatabaseHelper.GetConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();
            try
            {
                var orderID = conn.ExecuteScalar<int>(
                    @"INSERT INTO Orders (CustomerID, OrderTime, DeliveryProvince, DeliveryAddress, Status)
                      VALUES (@CustomerID, @OrderTime, @DeliveryProvince, @DeliveryAddress, @Status);
                      SELECT SCOPE_IDENTITY();",
                    new
                    {
                        order.CustomerID,
                        OrderTime = DateTime.Now,
                        order.DeliveryProvince,
                        order.DeliveryAddress,
                        Status = OrderStatusEnum.New
                    }, tran);

                foreach (var detail in order.Details)
                {
                    detail.OrderID = orderID;
                    conn.Execute(
                        @"INSERT INTO OrderDetails (OrderID, ProductID, Quantity, SalePrice)
                          VALUES (@OrderID, @ProductID, @Quantity, @SalePrice)",
                        detail, tran);
                }
                tran.Commit();
                return orderID;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public static List<OrderViewInfo> GetByCustomer(int customerID)
        {
            using var conn = DatabaseHelper.GetConnection();
            return conn.Query<OrderViewInfo>(
                @"SELECT o.*, c.CustomerName, c.ContactName as CustomerContact, c.Phone as CustomerPhone,
                         e.FullName as EmployeeName, s.ShipperName, s.Phone as ShipperPhone
                  FROM Orders o
                  LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                  LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
                  LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
                  WHERE o.CustomerID = @CustomerID
                  ORDER BY o.OrderTime DESC",
                new { CustomerID = customerID }).ToList();
        }

        public static OrderViewInfo? GetByID(int orderID, int customerID)
        {
            using var conn = DatabaseHelper.GetConnection();
            var order = conn.QueryFirstOrDefault<OrderViewInfo>(
                @"SELECT o.*, c.CustomerName, c.ContactName as CustomerContact, c.Phone as CustomerPhone,
                         e.FullName as EmployeeName, s.ShipperName, s.Phone as ShipperPhone
                  FROM Orders o
                  LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                  LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
                  LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
                  WHERE o.OrderID = @OrderID AND o.CustomerID = @CustomerID",
                new { OrderID = orderID, CustomerID = customerID });

            if (order != null)
            {
                order.Details = conn.Query<OrderDetailViewInfo>(
                    @"SELECT od.*, p.ProductName, p.Unit, p.Photo
                      FROM OrderDetails od
                      LEFT JOIN Products p ON od.ProductID = p.ProductID
                      WHERE od.OrderID = @OrderID",
                    new { OrderID = orderID }).ToList();
            }
            return order;
        }
    }

    // ===================== CART =====================
    public class CartHelper
    {
        private const string CART_KEY = "ShoppingCart";

        public static List<CartItem> GetCart(ISession session)
        {
            var json = session.GetString(CART_KEY);
            if (string.IsNullOrEmpty(json)) return new List<CartItem>();
            return System.Text.Json.JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }

        public static void SaveCart(ISession session, List<CartItem> cart)
        {
            session.SetString(CART_KEY, System.Text.Json.JsonSerializer.Serialize(cart));
        }

        public static void AddToCart(ISession session, CartItem item)
        {
            var cart = GetCart(session);
            var existing = cart.FirstOrDefault(x => x.ProductID == item.ProductID);
            if (existing != null)
                existing.Quantity += item.Quantity;
            else
                cart.Add(item);
            SaveCart(session, cart);
        }

        public static void UpdateQuantity(ISession session, int productID, int quantity)
        {
            var cart = GetCart(session);
            var item = cart.FirstOrDefault(x => x.ProductID == productID);
            if (item != null)
            {
                if (quantity <= 0) cart.Remove(item);
                else item.Quantity = quantity;
            }
            SaveCart(session, cart);
        }

        public static void RemoveItem(ISession session, int productID)
        {
            var cart = GetCart(session);
            cart.RemoveAll(x => x.ProductID == productID);
            SaveCart(session, cart);
        }

        public static void ClearCart(ISession session)
        {
            session.Remove(CART_KEY);
        }

        public static int GetCount(ISession session)
        {
            return GetCart(session).Sum(x => x.Quantity);
        }
    }

    // ===================== PASSWORD (dùng cho bảng riêng nếu cần) =====================
    public static class PasswordHelper
    {
        public static string Hash(string password)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLower();
        }

        public static bool Verify(string password, string hash)
        {
            return Hash(password) == hash;
        }
    }
}
// NOTE: Nếu bảng Customers trong LiteCommerceDB không có cột Password,
// cần chạy script SQL sau để thêm cột:
// ALTER TABLE Customers ADD Password NVARCHAR(64) NULL;
// UPDATE Customers SET Password = 'e10adc3949ba59abbe56e057f20f883e'; -- MD5('123456') or dùng SHA256