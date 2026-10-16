namespace leatherApp.Models;

public class CartItem
{
    public int CartItemId { get; set; }
    public string UserId { get; set; } = "";   // Auth0 user id
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public DateTime DateAdded { get; set; }
}