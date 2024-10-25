using System.ComponentModel.DataAnnotations.Schema;
using API.Model.Shared;

namespace API.Model.Entities;

public class Order : BaseEntity
{
    // Properties
    public required Guid UserId { get; set; }
    public required decimal Price { get; set; }
    // Relations
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = null!;
    public Transaction Transaction { get; set; } = null!;
    public Ship Ship { get; set; } = null!;
    public object Status { get; internal set; }
}