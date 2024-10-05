using System.ComponentModel.DataAnnotations.Schema;
using API.Model.Shared;

namespace API.Model.Entities;

public class Transaction : BaseEntity
{
    // Properties
    public Guid OrderId { get; set; }
    // Relations
    [ForeignKey(nameof(OrderId))]
    public Order Order { get; set; } = null!;
}