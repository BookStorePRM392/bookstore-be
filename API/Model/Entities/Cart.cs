namespace API.Model.Entities;

public class Cart
{
    // Properties
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    // Relations
    public List<CartItem> CartItems { get; set; } = [];
    public User User { get; set; } = null!;
}