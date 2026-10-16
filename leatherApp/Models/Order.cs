namespace leatherApp.Models;

public class Order
{
    public string? ShippingName { get; set; }
    public string? ShippingAddress { get; set; }
    public string? ShippingPostalCode { get; set; }
    public string? ShippingCity { get; set; }
    public int OrderId { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Status { get; set; }
    public string? Auth0UserId { get; set; }
    public bool IsArchived { get; set; } = false;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}