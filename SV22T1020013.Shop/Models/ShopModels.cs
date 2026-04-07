namespace SV22T1020013.Shop.Models
{
    // === CATALOG ===
    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class Product
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductDescription { get; set; }
        public int? SupplierID { get; set; }
        public int? CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Photo { get; set; }
        public bool IsSelling { get; set; }
    }

    // === PARTNER ===
    public class Customer
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string? Province { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string Email { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
        public string? Password { get; set; }
    }

    // === SALES ===
    public enum OrderStatusEnum
    {
        Rejected = -2,
        Cancelled = -1,
        New = 1,
        Accepted = 2,
        Shipping = 3,
        Completed = 4
    }

    public class Order
    {
        public int OrderID { get; set; }
        public int? CustomerID { get; set; }
        public DateTime OrderTime { get; set; }
        public string? DeliveryProvince { get; set; }
        public string? DeliveryAddress { get; set; }
        public int? EmployeeID { get; set; }
        public DateTime? AcceptTime { get; set; }
        public int? ShipperID { get; set; }
        public DateTime? ShippedTime { get; set; }
        public DateTime? FinishedTime { get; set; }
        public OrderStatusEnum Status { get; set; }
    }

    public class OrderViewInfo : Order
    {
        public string CustomerName { get; set; } = "";
        public string CustomerContact { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
        public string CustomerEmail { get; set; } = "";
        public string EmployeeName { get; set; } = "";
        public string ShipperName { get; set; } = "";
        public string ShipperPhone { get; set; } = "";
        public List<OrderDetailViewInfo> Details { get; set; } = new();
    }

    public class OrderDetail
    {
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal SalePrice { get; set; }
        public decimal TotalPrice => Quantity * SalePrice;
    }

    public class OrderDetailViewInfo : OrderDetail
    {
        public string ProductName { get; set; } = "";
        public string Unit { get; set; } = "";
        public string? Photo { get; set; }
    }

    // === CART ===
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? Photo { get; set; }
        public string Unit { get; set; } = "";
        public decimal Subtotal => Price * Quantity;
    }

    // === VIEW MODELS ===
    public class ProductSearchViewModel
    {
        public string Keyword { get; set; } = "";
        public int? CategoryID { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class CheckoutViewModel
    {
        public string ContactName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string DeliveryProvince { get; set; } = "";
        public string DeliveryAddress { get; set; } = "";
        public List<CartItem> CartItems { get; set; } = new();
        public decimal Total => CartItems.Sum(x => x.Subtotal);
    }

    public class RegisterViewModel
    {
        public string CustomerName { get; set; } = "";
        public string ContactName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";
        public string? Phone { get; set; }
        public string? Province { get; set; }
        public string? Address { get; set; }
    }

    public class LoginViewModel
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public bool RememberMe { get; set; }
    }
}
