using Microsoft.EntityFrameworkCore;
using MyShop.Api.Models;

namespace MyShop.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
}
