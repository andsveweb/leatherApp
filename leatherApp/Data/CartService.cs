using leatherApp.Models;
using Microsoft.EntityFrameworkCore;

namespace leatherApp.Data;

public class CartService
{
    private readonly IDbContextFactory<LeatherShopContext> _dbFactory;
    private readonly EmailService _email;

    public CartService(IDbContextFactory<LeatherShopContext> dbFactory, EmailService email)
    {
        _dbFactory = dbFactory;
        _email = email;
    }
    
    public event Action? OnChange;
    private void NotifyChanged() => OnChange?.Invoke();

    // Get all items in a user's cart
    public async Task<List<CartItem>> GetCartItems(string userId)
    {
        await using var db = _dbFactory.CreateDbContext();
        return await db.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();
    }

    // Add a product to the cart (or increase quantity if already there)
    public async Task AddToCart(string userId, int productId)
    {
        await using var db = _dbFactory.CreateDbContext();

        var existing = await db.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

        if (existing != null)
        {
            existing.Quantity++;
        }
        else
        {
            db.CartItems.Add(new CartItem
            {
                UserId = userId,
                ProductId = productId,
                Quantity = 1,
                DateAdded = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync();
        NotifyChanged();

    }

    // Remove an item from the cart
    public async Task RemoveFromCart(int cartItemId)
    {
        await using var db = _dbFactory.CreateDbContext();
        var item = await db.CartItems.FindAsync(cartItemId);
        if (item != null)
        {
            db.CartItems.Remove(item);
            await db.SaveChangesAsync();
            NotifyChanged();
        }
    }
    
    // Increase quantity by 1
    public async Task IncreaseQuantity(int cartItemId)
    {
        await using var db = _dbFactory.CreateDbContext();
        var item = await db.CartItems.FindAsync(cartItemId);
        if (item != null)
        {
            item.Quantity++;
            await db.SaveChangesAsync();
            NotifyChanged();
        }
    }

    // Decrease quantity by 1 (removes item if it hits 0)
    public async Task DecreaseQuantity(int cartItemId)
    {
        await using var db = _dbFactory.CreateDbContext();
        var item = await db.CartItems.FindAsync(cartItemId);
        if (item != null)
        {
            item.Quantity--;
            if (item.Quantity <= 0)
                db.CartItems.Remove(item);
            await db.SaveChangesAsync();
            NotifyChanged();
        }
    }

    // Count total items in a user's cart
    public async Task<int> GetCartCount(string userId)
    {
        await using var db = _dbFactory.CreateDbContext();
        return await db.CartItems
            .Where(c => c.UserId == userId)
            .SumAsync(c => c.Quantity);
    }
    
    // Turn the cart into an order, then clear the cart
    public async Task<int> Checkout(string userId, string? email,
        string? shippingName, string? shippingAddress, string? shippingPostalCode, string? shippingCity)
    {
        await using var db = _dbFactory.CreateDbContext();

        var cartItems = await db.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (cartItems.Count == 0)
            return 0;

        // Create the order
        var order = new Order
        {
            UserId = null,
            Auth0UserId = userId,
            OrderDate = DateTime.UtcNow,
            Status = "Pending",
            ShippingName = shippingName,
            ShippingAddress = shippingAddress,
            ShippingPostalCode = shippingPostalCode,
            ShippingCity = shippingCity,
            TotalAmount = cartItems.Sum(c => (c.Product?.Price ?? 0) * c.Quantity),
            Items = cartItems.Select(c => new OrderItem
            {
                ProductId = c.ProductId,
                Quantity = c.Quantity,
                PriceAtPurchase = c.Product?.Price ?? 0
            }).ToList()
        };

        db.Orders.Add(order);

        // Reduce stock for each product ordered
        foreach (var cartItem in cartItems)
        {
            var product = await db.Products.FindAsync(cartItem.ProductId);
            if (product != null)
            {
                product.StockQuantity -= cartItem.Quantity;
                if (product.StockQuantity < 0)
                    product.StockQuantity = 0;
            }
        }

        // Clear the cart
        db.CartItems.RemoveRange(cartItems);

        await db.SaveChangesAsync();
        NotifyChanged();

        // Send confirmation email (if we have an address)
        if (!string.IsNullOrEmpty(email))
        {
            try
            {
                await _email.SendOrderConfirmation(email, order.OrderId, order.TotalAmount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EMAIL ERROR: {ex.Message}");
            }
        }

        return order.OrderId;
    }
    
    // Get ALL orders (for admin) — newest first
    // Active (non-archived) orders for the main admin page
    public async Task<List<Order>> GetAllOrders()
    {
        await using var db = _dbFactory.CreateDbContext();
        return await db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => !o.IsArchived)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    // Archived orders for the archive page
    public async Task<List<Order>> GetArchivedOrders()
    {
        await using var db = _dbFactory.CreateDbContext();
        return await db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.IsArchived)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    // Archive or un-archive an order
    public async Task SetArchived(int orderId, bool archived)
    {
        await using var db = _dbFactory.CreateDbContext();
        var order = await db.Orders.FindAsync(orderId);
        if (order != null)
        {
            order.IsArchived = archived;
            await db.SaveChangesAsync();
        }
    }

    // Update an order's status
    public async Task UpdateOrderStatus(int orderId, string status)
    {
        await using var db = _dbFactory.CreateDbContext();
        var order = await db.Orders.FindAsync(orderId);
        if (order != null)
        {
            order.Status = status;
            await db.SaveChangesAsync();
        }
    }
}