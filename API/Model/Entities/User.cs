using API.Model.Enums;
using API.Model.Shared;

namespace API.Model.Entities;

public class User : BaseEntity
{
    // Properties
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string Email { get; set; }
    public required string Address { get; set; }
    public required string Phone { get; set; }
    public UserRoleEnum Role { get; set; } = UserRoleEnum.Customer;
    // Relations   
    public List<Order> Orders { get; set; } = [];
    public List<Transaction> Transactions { get; set; } = [];
    public List<Ship> Ships { get; set; } = [];
    public Cart Cart { get; set; } = null!;
    public List<Book> Books { get; set; } = [];
    public List<CartItem> CartItems { get; set; } = [];
    public List<OrderItem> OrderItems { get; set; } = [];

    public bool IsManager() => Role == UserRoleEnum.Manager;
    public bool IsShipper() => Role == UserRoleEnum.Shipper;
    public bool IsStaff() => Role == UserRoleEnum.Staff;
}