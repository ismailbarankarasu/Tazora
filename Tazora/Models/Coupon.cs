using SQLite;

namespace Tazora.Models;

[Table("Coupons")]
public class Coupon
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique]
    public string Code { get; set; } = string.Empty; 

    public decimal DiscountAmount { get; set; }

    public decimal DiscountRate { get; set; } 

    public decimal MinimumBasketAmount { get; set; } 

    public bool IsActive { get; set; } = true; 
}