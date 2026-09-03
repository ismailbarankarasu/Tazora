using Tazora.Models;

namespace Tazora.ViewModels;

public class OrderSummaryItem
{
    public int Id { get; init; }

    public DateTime OrderDate { get; init; }

    public decimal TotalAmount { get; init; }

    public OrderStatus Status { get; init; }

    public int ProductCount { get; init; }

    public string OrderNumber =>
        $"Sipariş #{Id}";

    public string OrderDateText =>
        OrderDate.ToLocalTime()
            .ToString("dd.MM.yyyy HH:mm");

    public string ProductCountText =>
        $"{ProductCount} ürün";

    public string StatusText =>
        Status switch
        {
            OrderStatus.Preparing => "Hazırlanıyor",
            OrderStatus.OnTheWay => "Yolda",
            OrderStatus.Delivered => "Teslim Edildi",
            OrderStatus.Cancelled => "İptal Edildi",
            _ => "Bilinmiyor"
        };
}