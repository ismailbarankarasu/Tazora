using SQLite;

namespace Tazora.Models;

[Table("Discounts")]
public class Discount
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int? ProductId { get; set; }

    [NotNull, MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(250)]
    public string? ImageName { get; set; }

    public int DiscountRate { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; } = true;
}