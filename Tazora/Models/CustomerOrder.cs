using SQLite;

namespace Tazora.Models;

[Table("Orders")]
public class CustomerOrder
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed, NotNull]
    public int UserId { get; set; }

    [NotNull]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [NotNull]
    public decimal Subtotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal DeliveryFee { get; set; }

    [NotNull]
    public decimal TotalAmount { get; set; }

    [NotNull]
    public OrderStatus Status { get; set; } = OrderStatus.Preparing;
}