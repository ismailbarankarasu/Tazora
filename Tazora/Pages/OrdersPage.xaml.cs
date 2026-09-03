using Tazora.Services;
using Tazora.ViewModels;

namespace Tazora.Pages;

public partial class OrdersPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private readonly AppSession _appSession;

    public OrdersPage(
        DatabaseService databaseService,
        AppSession appSession)
    {
        InitializeComponent();

        _databaseService = databaseService;
        _appSession = appSession;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadOrdersAsync();
    }

    private async Task LoadOrdersAsync()
    {
        var currentUser =
            _appSession.CurrentUser;

        if (currentUser is null)
        {
            OrdersCollectionView.ItemsSource =
                Array.Empty<OrderSummaryItem>();

            return;
        }

        try
        {
            OrdersLoadingIndicator.IsVisible = true;
            OrdersLoadingIndicator.IsRunning = true;

            var orders =
                await _databaseService
                    .GetOrdersByUserAsync(currentUser.Id);

            var orderItems =
                new List<OrderSummaryItem>();

            foreach (var order in orders)
            {
                var items =
                    await _databaseService
                        .GetOrderItemsByOrderIdAsync(order.Id);

                orderItems.Add(new OrderSummaryItem
                {
                    Id = order.Id,
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    ProductCount =
                        items.Sum(item => item.Quantity)
                });
            }

            OrdersCollectionView.ItemsSource =
                orderItems;
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);

            await DisplayAlert(
                "Bir Hata Oluştu",
                "Siparişler yüklenemedi.",
                "Tamam");
        }
        finally
        {
            OrdersLoadingIndicator.IsRunning = false;
            OrdersLoadingIndicator.IsVisible = false;
        }
    }

    private async void OnBackTapped(
        object sender,
        TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnOrderSelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault()
            is not OrderSummaryItem selectedOrder)
        {
            return;
        }

        OrdersCollectionView.SelectedItem = null;

        await Shell.Current.GoToAsync(
            $"{nameof(OrderDetailPage)}" +
            $"?orderId={selectedOrder.Id}");
    }
}