namespace leatherApp.Models;

public class User
{
    public int UserId { get; set; }
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public string? Name { get; set; }
    public DateTime DateCreated { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}