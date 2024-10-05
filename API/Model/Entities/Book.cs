using API.Model.Shared;

namespace API.Model.Entities;

public class Book : BaseEntity
{
    // Properties
    public required string Title { get; set; }
    public string Description { get; set; } = string.Empty;
    public required string Author { get; set; }
    public required decimal Price { get; set; }
    // Relations
    public IList<CartItem> CartItems { get; set; } = [];
    public IList<OrderItem> OrderItems { get; set; } = [];
}