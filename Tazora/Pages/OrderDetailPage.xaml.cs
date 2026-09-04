using Tazora.Models;
using Tazora.Services;

namespace Tazora.Pages;

public class OrderItemDisplay
{
    public int Id { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }

    public string? ImageName { get; set; }
}

public partial class OrderDetailPage
    : ContentPage, IQueryAttributable
{
    private readonly DatabaseService _databaseService;
    private readonly AppSession _appSession;

    private int _orderId;
    private bool _isLoaded;

    public OrderDetailPage(
        DatabaseService databaseService,
        AppSession appSession)
    {
        InitializeComponent();

        _databaseService = databaseService;
        _appSession = appSession;
    }

    public void ApplyQueryAttributes(
        IDictionary<string, object> query)
    {
        if (query.TryGetValue(
                "orderId",
                out var orderIdValue) &&
            int.TryParse(
                orderIdValue?.ToString(),
                out var orderId))
        {
            _orderId = orderId;
            _isLoaded = false;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isLoaded && _orderId > 0)
        {
            await LoadOrderAsync();
        }
    }

    private async Task LoadOrderAsync()
    {
        var currentUser =
            _appSession.CurrentUser;

        if (currentUser is null)
            return;

        try
        {
            OrderLoadingIndicator.IsVisible = true;
            OrderLoadingIndicator.IsRunning = true;

            var order =
                await _databaseService.GetOrderByIdAsync(
                    _orderId,
                    currentUser.Id);

            if (order is null)
            {
                await DisplayAlert(
                    "Sipariş Bulunamadı",
                    "Bu sipariş görüntülenemiyor.",
                    "Tamam");

                await Shell.Current.GoToAsync("..");
                return;
            }

       var orderItems =
        await _databaseService
        .GetOrderItemsByOrderIdAsync(order.Id);

            var displayItems = new List<OrderItemDisplay>();

            foreach (var item in orderItems)
            {
                var product =
                    await _databaseService
                        .GetProductByIdAsync(item.ProductId);

                displayItems.Add(new OrderItemDisplay
                {
                    Id = item.Id,
                    ProductName = item.ProductName,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    TotalPrice = item.TotalPrice,
                    ImageName = product?.ImageName
                });
            }

            OrderItemsCollectionView.ItemsSource =
                displayItems;

            OrderNumberLabel.Text =
                $"Sipariş #{order.Id}";

            OrderDateLabel.Text =
                order.OrderDate
                    .ToLocalTime()
                    .ToString("dd.MM.yyyy HH:mm");

            OrderStatusLabel.Text =
                GetStatusText(order.Status);

            SubtotalLabel.Text =
                $"{order.Subtotal:N2} TL";

            DiscountAmountLabel.Text =
                $"-{order.DiscountAmount:N2} TL";

            DeliveryFeeLabel.Text =
                order.DeliveryFee == 0
                    ? "Ücretsiz"
                    : $"{order.DeliveryFee:N2} TL";

            TotalAmountLabel.Text =
                $"{order.TotalAmount:N2} TL";

            _isLoaded = true;
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);

            await DisplayAlert(
                "Bir Hata Oluştu",
                "Sipariş detayı yüklenemedi.",
                "Tamam");
        }
        finally
        {
            OrderLoadingIndicator.IsRunning = false;
            OrderLoadingIndicator.IsVisible = false;
        }
    }

    private static string GetStatusText(
        OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Preparing => "Hazırlanıyor",
            OrderStatus.OnTheWay => "Yolda",
            OrderStatus.Delivered => "Teslim Edildi",
            OrderStatus.Cancelled => "İptal Edildi",
            _ => "Bilinmiyor"
        };
    }

    private async void OnBackTapped(
        object sender,
        TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}