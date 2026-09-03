using Tazora.Services;
using Tazora.ViewModels;

namespace Tazora.Pages;

public partial class DiscountsPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private bool _isLoaded;

    public DiscountsPage(DatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isLoaded)
        {
            await LoadDiscountsAsync();
        }
    }

    private async Task LoadDiscountsAsync()
    {
        try
        {
            DiscountLoadingIndicator.IsVisible = true;
            DiscountLoadingIndicator.IsRunning = true;

            var products =
                await _databaseService.GetProductsAsync();

            var discounts =
                await _databaseService.GetActiveDiscountsAsync();

            var productsById =
                products.ToDictionary(product => product.Id);

            var items = discounts
                .Where(discount =>
                    discount.ProductId.HasValue &&
                    productsById.ContainsKey(
                        discount.ProductId.Value))
                .GroupBy(discount =>
                    discount.ProductId!.Value)
                .Select(group =>
                    group.OrderByDescending(
                        discount => discount.DiscountRate)
                    .First())
                .Select(discount =>
                {
                    var product =
                        productsById[discount.ProductId!.Value];

                    return new HomeProductItem
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Unit = product.Unit,
                        ImageName = product.ImageName,
                        Price = product.Price,
                        DiscountRate = discount.DiscountRate
                    };
                })
                .ToList();

            DiscountsCollectionView.ItemsSource =
                items;

            DiscountCountLabel.Text =
                $"{items.Count} indirimli ürün bulundu";

            _isLoaded = true;
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);

            await DisplayAlert(
                "Bir Hata Oluştu",
                "İndirimler yüklenemedi.",
                "Tamam");
        }
        finally
        {
            DiscountLoadingIndicator.IsRunning = false;
            DiscountLoadingIndicator.IsVisible = false;
        }
    }

    private async void OnProductSelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault()
            is not HomeProductItem selectedProduct)
        {
            return;
        }

        DiscountsCollectionView.SelectedItem = null;

        await Shell.Current.GoToAsync(
            $"{nameof(ProductDetailPage)}" +
            $"?productId={selectedProduct.Id}");
    }

    private async void OnAddToBasketClicked(
        object sender,
        EventArgs e)
    {
        if (sender is not Button button ||
            button.CommandParameter is not int productId)
        {
            return;
        }

        try
        {
            button.IsEnabled = false;
            button.Text = "Ekleniyor...";

            var quantity =
                await _databaseService
                    .AddProductToBasketAsync(productId);

            button.Text = $"Sepette: {quantity}";

            await Task.Delay(800);
        }
        finally
        {
            button.Text = "+ Ekle";
            button.IsEnabled = true;
        }
    }

    private async void OnBackTapped(
        object sender,
        TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}