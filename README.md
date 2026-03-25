# MyShop — Kiến trúc MVVM + Supabase

## Luồng dữ liệu (Data Flow)

```
View (XAML)
    │  binding DataContext
    ▼
ViewModel (ObservableObject + RelayCommand)
    │  gọi Service qua DI
    ▼
Service (Interface + Implementation)
    │  gọi Repository qua DI
    ▼
Repository (Interface + Implementation)
    │  gọi Supabase SDK
    ▼
Supabase (Database)
```

## Cấu trúc thư mục

```
MyShop/
├── Models/              ← Mô hình dữ liệu (POCO, map từ bảng DB)
├── Repositories/        ← Giao tiếp Supabase SDK
│   ├── I{Entity}Repository.cs    ← Interface
│   └── {Entity}Repository.cs     ← Implementation
├── Services/            ← Logic nghiệp vụ (gọi Repository)
│   ├── I{Entity}Service.cs       ← Interface
│   └── {Entity}Service.cs        ← Implementation
├── ViewModels/          ← Logic + trạng thái UI (CommunityToolkit.Mvvm)
├── Views/               ← Giao diện XAML
├── Converters/          ← Helper chuyển đổi dữ liệu cho XAML
├── MauiProgram.cs       ← Đăng ký Dependency Injection
└── App.xaml(.cs)       ← Entry point
```
```

## Hướng dẫn thêm một tính năng mới

### Ví dụ: Thêm module "Sản phẩm" (Product)

---

### Bước 1 — Tạo Model

Tạo file: `Models/Product.cs`

```csharp
using Postgrest.Attributes;
using Postgrest.Models;

namespace MyShop.Models;

[Table("products")]
public class Product : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("price")]
    public decimal Price { get; set; }

    [Column("category_id")]
    public int CategoryId { get; set; }
}
```

---

### Bước 2 — Tạo Repository

**Interface** — Tạo file: `Repositories/IProductRepository.cs`

```csharp
using MyShop.Models;

namespace MyShop.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();
    Task<List<Product>> GetByCategoryAsync(int categoryId);
    Task<int> AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
}
```

**Implementation** — Tạo file: `Repositories/ProductRepository.cs`

```csharp
using MyShop.Models;
using Supabase;

namespace MyShop.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly Supabase.Client _client;

    public ProductRepository(Supabase.Client client) => _client = client;

    public async Task<List<Product>> GetAllAsync()
    {
        var response = await _client.From<Product>().Get();
        return response.Models;
    }

    public async Task<List<Product>> GetByCategoryAsync(int categoryId)
    {
        var response = await _client.From<Product>()
            .Where(x => x.CategoryId == categoryId)
            .Get();
        return response.Models;
    }

    public async Task<int> AddAsync(Product product)
    {
        var result = await _client.From<Product>().Insert(product);
        return result.Models.FirstOrDefault()?.Id ?? 0;
    }

    public async Task UpdateAsync(Product product)
        => await _client.From<Product>().Update(product);

    public async Task DeleteAsync(int id)
        => await _client.From<Product>().Where(x => x.Id == id).Delete();
}
```

---

### Bước 3 — Tạo Service

**Interface** — Tạo file: `Services/IProductService.cs`

```csharp
using MyShop.Models;

namespace MyShop.Services;

public interface IProductService
{
    Task<List<Product>> GetAllAsync();
    Task<List<Product>> GetByCategoryAsync(int categoryId);
    Task<int> AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
}
```

**Implementation** — Tạo file: `Services/ProductService.cs`

```csharp
using MyShop.Models;
using MyShop.Repositories;

namespace MyShop.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository) => _repository = repository;

    public async Task<List<Product>> GetAllAsync()
        => await _repository.GetAllAsync();

    public async Task<List<Product>> GetByCategoryAsync(int categoryId)
        => await _repository.GetByCategoryAsync(categoryId);

    public async Task<int> AddAsync(Product product)
        => await _repository.AddAsync(product);

    public async Task UpdateAsync(Product product)
        => await _repository.UpdateAsync(product);

    public async Task DeleteAsync(int id)
        => await _repository.DeleteAsync(id);
}
```

---

### Bước 4 — Tạo ViewModel

Tạo file: `ViewModels/ProductViewModel.cs`

```csharp
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Models;
using MyShop.Services;

namespace MyShop.ViewModels;

public partial class ProductViewModel : ObservableObject
{
    private readonly IProductService _service;

    public ProductViewModel(IProductService service)
    {
        _service = service;
    }

    // Property → sinh ra "Products" và command "LoadProductsCommand"
    [ObservableProperty]
    private ObservableCollection<Product> _products = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [RelayCommand]
    public async Task LoadProductsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var result = await _service.GetAllAsync();
            Products = new ObservableCollection<Product>(result);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

---

### Bước 5 — Tạo View (XAML)

**UI** — Tạo file: `Views/ProductPage.xaml`

```xml
<Page x:Class="MyShop.Views.ProductPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:m="using:MyShop.Models"
      xmlns:conv="using:MyShop.Converters"
      Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">

  <Page.Resources>
    <conv:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter"/>
    <conv:StringToVisibilityConverter x:Key="StringToVisibilityConverter"/>
  </Page.Resources>

  <Grid Margin="24">
    <Grid.RowDefinitions>
      <RowDefinition Height="Auto"/>
      <RowDefinition Height="Auto"/>
      <RowDefinition Height="*"/>
      <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>

    <!-- Tiêu đề + Loading -->
    <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,0,0,16">
      <TextBlock Text="Sản phẩm" FontSize="28" FontWeight="Bold"
                 VerticalAlignment="Center"/>
      <ProgressBar IsIndeterminate="True" Width="120" Margin="16,0,0,0"
                   VerticalAlignment="Center"
                   Visibility="{Binding IsLoading,
                             Converter={StaticResource BoolToVisibilityConverter}}"/>
    </StackPanel>

    <!-- Nút tải -->
    <Button Grid.Row="1" Content="Tải danh sách"
            Command="{Binding LoadProductsCommand}"
            HorizontalAlignment="Left" Margin="0,0,0,12"/>

    <!-- Danh sách -->
    <ListView Grid.Row="2" ItemsSource="{Binding Products}"
              BorderThickness="1" BorderBrush="Gray">
      <ListView.ItemTemplate>
        <DataTemplate x:DataType="m:Product">
          <StackPanel Padding="8" Orientation="Horizontal">
            <TextBlock Text="{x:Bind Name}" FontWeight="SemiBold" Width="200"/>
            <TextBlock Text="{x:Bind Price, StringFormat='{}{0:N0}đ'}"
                       Foreground="Gray" Width="120"/>
          </StackPanel>
        </DataTemplate>
      </ListView.ItemTemplate>
    </ListView>

    <!-- Thông báo lỗi -->
    <TextBlock Grid.Row="3" Text="{Binding ErrorMessage}"
               Foreground="Red" TextWrapping="Wrap" Margin="0,12,0,0"
               Visibility="{Binding ErrorMessage,
                             Converter={StaticResource StringToVisibilityConverter}}"/>
  </Grid>
</Page>
```

**Code-behind** — Tạo file: `Views/ProductPage.xaml.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;
using MyShop.ViewModels;

namespace MyShop.Views;

public sealed partial class ProductPage : Page
{
    public ProductPage()
    {
        this.InitializeComponent();
        DataContext = App.Services.GetRequiredService<ProductViewModel>();
    }
}
```

---

### Bước 6 — Đăng ký DI trong MauiProgram.cs

Mở `MauiProgram.cs`, thêm 2 dòng vào method `Build()`:

```csharp
// Thêm 2 dòng này
Services.AddScoped<IProductService, ProductService>();
Services.AddTransient<ProductViewModel>();
```

```csharp
public static IServiceProvider Build()
{
    Env.Load();

    // Supabase Client (giữ nguyên)
    var url     = Environment.GetEnvironmentVariable("SUPABASE_URL")
        ?? throw new InvalidOperationException("SUPABASE_URL not set");
    var anonKey = Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY")
        ?? throw new InvalidOperationException("SUPABASE_ANON_KEY not set");

    Services.AddSingleton(_ => new Supabase.Client(url, anonKey,
        new SupabaseOptions { AutoRefreshToken = true }));

    // ── Repositories ────────────────────────────────────────
    Services.AddScoped<ICategoryRepository, CategoryRepository>();

    // 👇 Thêm dòng này
    Services.AddScoped<IProductRepository, ProductRepository>();

    // ── Services ───────────────────────────────────────────
    Services.AddScoped<ICategoryService, CategoryService>();

    // 👇 Thêm dòng này
    Services.AddScoped<IProductService, ProductService>();

    // ── ViewModels ─────────────────────────────────────────
    Services.AddTransient<CategoryViewModel>();

    // 👇 Thêm dòng này
    Services.AddTransient<ProductViewModel>();

    return Services.BuildServiceProvider();
}
```

---

### Bước 7 — Chạy trang mới

Trong `App.xaml.cs`, sửa dòng điều hướng ở `OnLaunched`:

```csharp
// Thay vì CategoryPage, chuyển sang ProductPage
rootFrame.Navigate(typeof(ProductPage), args.Arguments);
```

---

## Cách gọi Supabase SDK trong Repository

```csharp
await _client.From<T>().Get();                                    // Lấy tất cả
await _client.From<T>().Where(x => x.Id == id).Single();          // Lấy 1 theo ID
await _client.From<T>().Where(x => x.CategoryId == catId).Get();  // Lọc theo điều kiện
await _client.From<T>().Order(x => x.Name, Constants.Ordering.Ascending).Get(); // Sắp xếp
await _client.From<T>().Insert(item);                              // Thêm mới
await _client.From<T>().Update(item);                              // Cập nhật
await _client.From<T>().Where(x => x.Id == id).Delete();           // Xóa theo ID
```

## Quy tắc đặt tên

| Thành phần | Quy tắc | Ví dụ |
|---|---|---|
| Model | `PascalCase`, kế thừa `BaseModel` | `Product` |
| Table attribute | `snake_case`, số nhiều | `[Table("products")]` |
| Column attribute | `snake_case` | `[Column("category_id")]` |
| Repository Interface | `I` + `TênModel` + `Repository` | `IProductRepository` |
| Repository Impl | `TênModel` + `Repository` | `ProductRepository` |
| Service Interface | `I` + `TênModel` + `Service` | `IProductService` |
| Service Impl | `TênModel` + `Service` | `ProductService` |
| ViewModel | `TênModel` + `ViewModel` | `ProductViewModel` |
| View Page | `TênModel` + `Page` | `ProductPage.xaml` |

## Chạy ứng dụng

```bash
# 1. Tạo file .env trong thư mục MyShop/ với:
#    SUPABASE_URL=https://xxx.supabase.co
#    SUPABASE_ANON_KEY=eyJ...

# 2. Chạy app
dotnet run --project MyShop/MyShop.csproj
```
