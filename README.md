# MyShop — Kiến trúc Full Stack (Frontend + Backend API)

## Tổng quan kiến trúc

```
┌─────────────────────────────────────────────────────────────┐
│  FRONTEND (MyShop) — Uno Platform (WASM / Desktop / Mobile) │
│  View ←→ ViewModel ←→ HttpClient ←→ API (MyShop.Api)       │
└────────────────────────────┬────────────────────────────────┘
                             │ HTTP
                             ↓
┌─────────────────────────────────────────────────────────────┐
│  BACKEND (MyShop.Api) — ASP.NET Core Web API                 │
│  Controller ←→ Service ←→ DbContext ←→ Supabase (Postgres)  │
└─────────────────────────────────────────────────────────────┘
```

## Cấu trúc thư mục

### Frontend — MyShop (Uno Platform)

```
MyShop/
├── App.xaml(.cs)           # Entry point
├── MainPage.xaml(.cs)      # Trang mặc định
├── Models/                  # Entity / DTO (đại diện bảng trong DB)
├── ViewModels/              # Logic xử lý cho View (ObservableProperty)
├── Views/                   # Giao diện XAML pages
├── Services/                # Kết nối HTTP đến API
│   └── ApiService.cs        # Gọi REST API
├── DataAccess/              # Repository pattern (interface + impl)
│   ├── IRepo.cs
│   └── ApiRepo.cs
└── .env                     # API URL (KHÔNG commit)
```

### Backend — MyShop.Api (ASP.NET Core)

```
MyShop.Api/
├── Program.cs               # Cấu hình DI, CORS, DbContext
├── .env                      # Connection string Supabase (KHÔNG commit)
├── Controllers/              # Nhận HTTP request
│   └── CategoriesController.cs
├── Services/                 # Logic nghiệp vụ
│   └── CategoryService.cs
├── Data/                     # Entity Framework DbContext
│   └── AppDbContext.cs
├── Models/                   # Entity (map từ bảng DB)
│   └── Category.cs
└── DTOs/                     # Request / Response objects
    ├── CategoryDto.cs
    └── ApiResponse.cs
```

## Chạy dự án

### 1. Backend (chạy trước)

```bash
cd MyShop.Api
dotnet run
# → API chạy tại http://localhost:5227
```

### 2. Frontend (chạy sau)

```bash
cd MyShop
dotnet run
# → App chạy tại http://localhost:5001 (hoặc cổng khác)
```

## Thêm bảng mới

### Phía Backend (MyShop.Api)

**1. Tạo Model** trong `Models/`

```csharp
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
}
```

**2. Đăng ký DbSet** trong `Data/AppDbContext.cs`

```csharp
public DbSet<Order> Orders => Set<Order>();
```

**3. Tạo DTO** trong `DTOs/`

```csharp
public record OrderResponse(int Id, string CustomerName, decimal Total);
public record CreateOrderRequest(string CustomerName, decimal Total);
```

**4. Tạo Service** trong `Services/`

```csharp
public interface IOrderService { ... }
public class OrderService : IOrderService { ... }
```

**5. Đăng ký DI** trong `Program.cs`

```csharp
builder.Services.AddScoped<IOrderService, OrderService>();
```

**6. Tạo Controller** trong `Controllers/`

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase { ... }
```

### Phía Frontend (MyShop)

**1. Tạo Model** trong `Models/`

```csharp
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}
```

**2. Cập nhật ApiService** trong `Services/`

```csharp
public async Task<List<Category>> GetCategoriesAsync()
{
    return await _httpClient.GetFromJsonAsync<List<Category>>($"{BaseUrl}/api/categories");
}
```

**3. Tạo ViewModel** trong `ViewModels/`

```csharp
public partial class CategoryViewModel : ObservableObject
{
    [ObservableProperty]
    private List<Category> _categories = [];

    public async Task LoadAsync()
    {
        Categories = await _apiService.GetCategoriesAsync();
    }
}
```

## Quy tắc đặt tên

| Thành phần | Quy tắc | Ví dụ |
|---|---|---|
| Model | PascalCase | `Category`, `Order` |
| DTO Request | `{Tên}Request` | `CreateCategoryRequest` |
| DTO Response | `{Tên}Response` | `CategoryResponse` |
| Controller | `{Tên}sController` | `CategoriesController` |
| Service | `{Tên}Service` | `CategoryService` |
| ViewModel | `{Tên}ViewModel` | `CategoryViewModel` |
| View | `{Tên}Page.xaml` | `CategoryPage.xaml` |
| Bảng DB | snake_case số nhiều | `categories`, `orders` |
