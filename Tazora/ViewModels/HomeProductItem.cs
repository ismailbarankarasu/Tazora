namespace Tazora.ViewModels;

public class HomeProductItem
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Unit { get; init; } = string.Empty;

    public string? ImageName { get; init; }

    public decimal Price { get; init; }

    public int DiscountRate { get; init; }

    public bool HasDiscount =>
        DiscountRate > 0;

    public decimal DiscountedPrice =>
        HasDiscount
            ? Math.Round(
                Price * (100 - DiscountRate) / 100,
                2)
            : Price;

    public string DiscountText =>
        $"%{DiscountRate}";
}