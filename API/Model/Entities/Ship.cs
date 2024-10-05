using System.ComponentModel.DataAnnotations.Schema;
using API.Model.Enums;
using API.Model.Shared;

namespace API.Model.Entities;

public class Ship : BaseEntity
{
    // Properties
    public Guid ShipperId { get; set; }
    public Guid OrderId { get; set; }
    public ShipEnum Status { get; set; } = ShipEnum.Packaging;
    // Relations
    [ForeignKey(nameof(ShipperId))]
    public User Shipper { get; set; } = null!;
    [ForeignKey(nameof(OrderId))]
    public Order Order { get; set; } = null!;
}