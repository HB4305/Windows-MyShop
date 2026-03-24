using Microsoft.EntityFrameworkCore;
using MyShop.Api.Data;
using MyShop.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Load biến môi trường từ file .env
DotNetEnv.Env.Load();

// ===== DATABASE =====
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? throw new InvalidOperationException("DATABASE_URL not found in .env");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ===== SERVICES (Dependency Injection) =====
builder.Services.AddScoped<ICategoryService, CategoryService>();

// ===== CORS (cho phép app Uno gọi API) =====
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ===== CONTROLLERS =====
builder.Services.AddControllers();

var app = builder.Build();

// ===== MIDDLEWARE PIPELINE =====

// CORS phải đặt TRƯỚC MapControllers
app.UseCors();

// Map controllers (API endpoints)
app.MapControllers();

// Health check đơn giản
app.MapGet("/", () => Results.Ok(new { Status = "OK", Message = "MyShop API is running" }));

app.Run();
