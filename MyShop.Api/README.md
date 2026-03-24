# MyShop.Api — Backend REST API

## Cấu trúc thư mục

```
MyShop.Api/
├── Program.cs              # Cấu hình chương trình (DI, CORS, DbContext)
├── .env                    # Connection string Supabase (KHÔNG commit)
│
├── Controllers/            # Nhận request HTTP, gọi Service
│   ├── ProductsController.cs
│   └── CategoriesController.cs
│
├── Services/                # Logic nghiệp vụ (gọi DbContext)
│   ├── ProductService.cs
│   └── CategoryService.cs
│
├── Data/                   # Entity Framework DbContext
│   └── AppDbContext.cs
│
├── Models/                 # Entity (map từ bảng trong DB)
│   ├── Product.cs
│   └── Category.cs
│
├── DTOs/                   # Request / Response objects
│   ├── ProductDto.cs
│   ├── CategoryDto.cs
│   └── ApiResponse.cs
│
└── Helpers/                # Các hàm helper dùng chung (tương lai)
```

## Luồng xử lý request

```
HTTP Request
    ↓
[Controller]  ← nhận request, validate, gọi service
    ↓
[Service]     ← logic nghiệp vụ, gọi DB
    ↓
[DbContext]   ← EF Core tạo SQL, giao tiếp Supabase Postgres
    ↓
Supabase DB
```

## Cách chạy

```bash
cd MyShop.Api

# Restore packages
dotnet restore

# Chạy (tự đọc .env)
dotnet run
# → API chạy tại http://localhost:5000
```

## Các API Endpoints

### Products

| Method | Endpoint | Mô tả |
|---|---|---|
| GET | `/api/products` | Lấy tất cả sản phẩm |
| GET | `/api/products/{id}` | Lấy sản phẩm theo id |
| GET | `/api/products/category/{categoryId}` | Lấy sản phẩm theo danh mục |
| POST | `/api/products` | Tạo sản phẩm mới |
| PUT | `/api/products/{id}` | Cập nhật sản phẩm |
| DELETE | `/api/products/{id}` | Xóa sản phẩm |

### Categories

| Method | Endpoint | Mô tả |
|---|---|---|
| GET | `/api/categories` | Lấy tất cả danh mục |
| GET | `/api/categories/{id}` | Lấy danh mục theo id |
| POST | `/api/categories` | Tạo danh mục mới |
| PUT | `/api/categories/{id}` | Cập nhật danh mục |
| DELETE | `/api/categories/{id}` | Xóa danh mục |

## Định dạng Response

**Thành công:**
```json
{
  "success": true,
  "message": "Lấy danh sách sản phẩm thành công",
  "data": [ ... ]
}
```

**Lỗi:**
```json
{
  "success": false,
  "message": "Không tìm thấy sản phẩm với id = 999",
  "detail": null
}
```

## Thêm bảng mới

**Ví dụ:** thêm bảng `orders`

### 1. Tạo Model

```csharp
// Models/Order.cs
[Table("orders")]
public class Order
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("customer_name")]
    public string CustomerName { get; set; } = "";

    [Column("total")]
    public decimal Total { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### 2. Đăng ký DbSet

```csharp
// Data/AppDbContext.cs
public DbSet<Order> Orders => Set<Order>();
```

### 3. Tạo DTO

```csharp
// DTOs/OrderDto.cs
public record OrderResponse(int Id, string CustomerName, decimal Total, DateTime CreatedAt);
public record CreateOrderRequest(string CustomerName, decimal Total);
```

### 4. Tạo Service

```csharp
// Services/OrderService.cs
public interface IOrderService { ... }
public class OrderService : IOrderService { ... }
```

### 5. Đăng ký DI

```csharp
// Program.cs
builder.Services.AddScoped<IOrderService, OrderService>();
```

### 6. Tạo Controller

```csharp
// Controllers/OrdersController.cs
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase { ... }
```

## Cấu hình database

Sửa file `.env`:

```
DATABASE_URL=Host=<host>;Port=<port>;Database=<db>;Username=<user>;Password=<pass>;
```

Lấy từ **Supabase Dashboard → Project Settings → Connection Pooling**

## Quy tắc đặt tên

| Thành phần | Quy tắc | Ví dụ |
|---|---|---|
| Model | PascalCase, không suffix | `Product`, `Order` |
| DTO Request | `{Tên}Request` | `CreateProductRequest` |
| DTO Response | `{Tên}Response` | `ProductResponse` |
| Controller | `{Tên}sController` | `ProductsController` |
| Service | `{Tên}Service` | `ProductService` |
| Bảng DB | snake_case số nhiều | `products`, `orders` |

## Packages đã cài

| Package | Mục đích |
|---|---|
| `Npgsql.EntityFrameworkCore.PostgreSQL` | Kết nối EF Core → Postgres |
| `Microsoft.EntityFrameworkCore.Design` | Migration tooling |
| `DotNetEnv` | Đọc file `.env` |
