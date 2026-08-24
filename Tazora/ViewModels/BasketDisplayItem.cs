namespace Tazora.ViewModels;

public class BasketDisplayItem
{
    public int ProductId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Unit { get; init; } = string.Empty;

    public string? ImageName { get; init; }

    public decimal Price { get; init; }

    public int DiscountRate { get; init; }

    public int Quantity { get; init; }

    public bool HasDiscount =>
        DiscountRate > 0;

    public decimal UnitPrice =>
        HasDiscount
            ? Math.Round(
                Price * (100 - DiscountRate) / 100,
                2)
            : Price;

    public decimal LineTotal =>
        UnitPrice * Quantity;

    public string DiscountText =>
        $"%{DiscountRate}";
}