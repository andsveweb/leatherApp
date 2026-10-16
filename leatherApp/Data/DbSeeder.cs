using leatherApp.Models;

namespace leatherApp.Data;

public static class DbSeeder
{
    public static void Seed(LeatherShopContext db)
    {
        // Only seed if there are no products yet
        if (db.Products.Any())
            return;

        // Create categories
        var wallets = new Category { Name = "Wallets" };
        var bags = new Category { Name = "Bags" };
        var belts = new Category { Name = "Belts" };

        db.Categories.AddRange(wallets, bags, belts);
        db.SaveChanges();

        // Create products
        var products = new List<Product>
        {
            new Product
            {
                Name = "Classic Bifold Wallet",
                Description = "Full-grain leather bifold with six card slots.",
                Price = 79,
                CategoryId = wallets.CategoryId,
                StockQuantity = 12,
                ImageUrl = "/images/wallet.jpg",
                DateCreated = DateTime.UtcNow
            },
            new Product
            {
                Name = "Slim Card Holder",
                Description = "Minimal card holder for everyday carry.",
                Price = 45,
                CategoryId = wallets.CategoryId,
                StockQuantity = 20,
                ImageUrl = "/images/wallet.jpg",
                DateCreated = DateTime.UtcNow
            },
            new Product
            {
                Name = "Weekender Bag",
                Description = "Spacious leather duffel for short trips.",
                Price = 320,
                CategoryId = bags.CategoryId,
                StockQuantity = 5,
                ImageUrl = "/images/bag.jpg",
                DateCreated = DateTime.UtcNow
            },
            new Product
            {
                Name = "Leather Belt",
                Description = "Handmade full-grain belt with brass buckle.",
                Price = 65,
                CategoryId = belts.CategoryId,
                StockQuantity = 15,
                ImageUrl = "/images/belt.jpg",
                DateCreated = DateTime.UtcNow
            }
        };

        db.Products.AddRange(products);
        db.SaveChanges();
    }
}