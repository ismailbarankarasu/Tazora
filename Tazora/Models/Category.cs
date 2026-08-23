using SQLite;

namespace Tazora.Models;

[Table("Categories")]
public class Category
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? ImageName { get; set; }

    [MaxLength(20)]
    public string? IconCode { get; set; }

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }
}