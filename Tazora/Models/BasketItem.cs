using SQLite;

namespace Tazora.Models;

[Table("BasketItems")]
public class BasketItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed, Unique, NotNull]
    public int ProductId { get; set; }

    [NotNull]
    public int Quantity { get; set; } = 1;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}