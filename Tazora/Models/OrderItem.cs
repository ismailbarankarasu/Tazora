using SQLite;

namespace Tazora.Models;

[Table("OrderItems")]
public class OrderItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed, NotNull]
    public int OrderId { get; set; }

    [Indexed, NotNull]
    public int ProductId { get; set; }

    [NotNull]
    public string ProductName { get; set; } = string.Empty;

    [NotNull]
    public decimal UnitPrice { get; set; }

    [NotNull]
    public int Quantity { get; set; }

    [NotNull]
    public decimal TotalPrice { get; set; }
}