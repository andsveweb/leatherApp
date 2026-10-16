using leatherApp.Models;
using Microsoft.EntityFrameworkCore;

namespace leatherApp.Data;

public class LeatherShopContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

    public LeatherShopContext(DbContextOptions<LeatherShopContext> options)
        : base(options)
    {
    }
}