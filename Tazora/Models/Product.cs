using SQLite;

namespace Tazora.Models;

[Table("Products")]
public class Product
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed, NotNull]
    public int CategoryId { get; set; }

    [NotNull, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [NotNull, MaxLength(50)]
    public string Unit { get; set; } = string.Empty;

    [NotNull]
    public decimal Price { get; set; }

    [MaxLength(250)]
    public string? ImageName { get; set; }

    public bool IsPopular { get; set; }

    public bool IsActive { get; set; } = true;
}