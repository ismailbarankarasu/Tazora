using Tazora.Services;

namespace Tazora.Pages;

public partial class StatisticsPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private readonly AppSession _appSession;

    public StatisticsPage(
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
        await LoadStatisticsAsync();
    }

    private async Task LoadStatisticsAsync()
    {
        try
        {
            StatisticsLoadingIndicator.IsVisible = true;
            StatisticsLoadingIndicator.IsRunning = true;

            var products =
                await _databaseService.GetProductsAsync();

            var categories =
                await _databaseService.GetCategoriesAsync();

            var basketItems =
                await _databaseService.GetBasketItemsAsync();

            var discounts =
                await _databaseService.GetActiveDiscountsAsync();

            var productsById =
                products.ToDictionary(product => product.Id);

            var discountsByProductId = discounts
                .Where(discount => discount.ProductId.HasValue)
                .GroupBy(discount => discount.ProductId!.Value)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderByDescending(item => item.DiscountRate)
                        .First());

            decimal basketTotal = 0;

            foreach (var basketItem in basketItems)
            {
                if (!productsById.TryGetValue(
                        basketItem.ProductId,
                        out var product))
                {
                    continue;
                }

                var unitPrice = product.Price;

                if (discountsByProductId.TryGetValue(
                        product.Id,
                        out var discount))
                {
                    unitPrice = Math.Round(
                        product.Price *
                        (100 - discount.DiscountRate) / 100,
                        2);
                }

                basketTotal +=
                    unitPrice * basketItem.Quantity;
            }

            var orderCount = 0;
            var currentUser =
                _appSession.CurrentUser;

            if (currentUser is not null)
            {
                var orders =
                    await _databaseService
                        .GetOrdersByUserAsync(currentUser.Id);

                orderCount = orders.Count;
            }

            ProductCountLabel.Text =
                products.Count.ToString();

            CategoryCountLabel.Text =
                categories.Count.ToString();

            BasketQuantityLabel.Text =
                basketItems.Sum(item => item.Quantity)
                    .ToString();

            DiscountCountLabel.Text =
                discounts.Count(discount =>
                    discount.ProductId.HasValue)
                    .ToString();

            BasketTotalLabel.Text =
                $"{basketTotal:N2} TL";

            OrderCountLabel.Text =
                $"{orderCount} sipariş";
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);

            await DisplayAlert(
                "Bir Hata Oluştu",
                "İstatistikler yüklenemedi.",
                "Tamam");
        }
        finally
        {
            StatisticsLoadingIndicator.IsRunning = false;
            StatisticsLoadingIndicator.IsVisible = false;
        }
    }

    private async void OnBackTapped(
        object sender,
        TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}