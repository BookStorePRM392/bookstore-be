using System.ComponentModel.DataAnnotations.Schema;
using API.Model.Shared;

namespace API.Model.Entities;

public class OrderItem : BaseEntity
{
    // Properties
    public Guid OrderId { get; set; }
    public Guid BookId { get; set; }
    public int Quantity { get; set; }
    // Relations
    [ForeignKey(nameof(OrderId))]
    public Order Order { get; set; } = null!;
    [ForeignKey(nameof(BookId))]
    public Book Book { get; set; } = null!;
}