using System.ComponentModel.DataAnnotations.Schema;
using API.Model.Shared;

namespace API.Model.Entities;

public class CartItem : BaseEntity
{
    // Properties
    public Guid CartId { get; set; }
    public Guid BookId { get; set; }
    public int Quantity { get; set; }
    // Relations
    [ForeignKey(nameof(CartId))]
    public Cart Cart { get; set; } = null!;
    [ForeignKey(nameof(BookId))]
    public Book Book { get; set; } = null!;
}