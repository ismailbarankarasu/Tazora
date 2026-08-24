using Tazora.Models;
using Tazora.Services;
using Tazora.ViewModels;

namespace Tazora.Pages;

public partial class ProductListPage
    : ContentPage, IQueryAttributable
{
    private readonly DatabaseService _databaseService;

    private List<HomeProductItem> _products = [];
    private int _categoryId;
    private string _categoryName = string.Empty;
    private bool _isLoaded;

    public ProductListPage(DatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;
    }

    public void ApplyQueryAttributes(
        IDictionary<string, object> query)
    {
        if (query.TryGetValue(
                "categoryId",
                out var categoryIdValue) &&
            int.TryParse(
                categoryIdValue?.ToString(),
                out var categoryId))
        {
            _categoryId = categoryId;
        }

        if (query.TryGetValue(
                "categoryName",
                out var categoryNameValue))
        {
            _categoryName =
                Uri.UnescapeDataString(
                    categoryNameValue?.ToString()
                    ?? string.Empty);

            CategoryTitleLabel.Text = _categoryName;
        }

        _isLoaded = false;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isLoaded && _categoryId > 0)
        {
            await LoadProductsAsync();
        }
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            ProductErrorBorder.IsVisible = false;
            ProductLoadingIndicator.IsVisible = true;
            ProductLoadingIndicator.IsRunning = true;

            var products =
                await _databaseService
                    .GetProductsByCategoryAsync(_categoryId);

            var discounts =
                await _databaseService
                    .GetActiveDiscountsAsync();

            var discountsByProductId = discounts
                .Where(discount => discount.ProductId.HasValue)
                .GroupBy(discount => discount.ProductId!.Value)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderByDescending(item => item.DiscountRate)
                        .First());

            _products = products
                .Select(product =>
                {
                    discountsByProductId.TryGetValue(
                        product.Id,
                        out var discount);

                    return CreateProductItem(
                        product,
                        discount);
                })
                .ToList();

            ProductsCollectionView.ItemsSource =
                _products;

            ProductCountLabel.Text =
                $"{_products.Count} ürün bulundu";

            _isLoaded = true;
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);
            ProductErrorBorder.IsVisible = true;
        }
        finally
        {
            ProductLoadingIndicator.IsRunning = false;
            ProductLoadingIndicator.IsVisible = false;
        }
    }

    private void OnSearchTextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        var searchText =
            e.NewTextValue?.Trim();

        if (string.IsNullOrWhiteSpace(searchText))
        {
            ProductsCollectionView.ItemsSource =
                _products;

            ProductCountLabel.Text =
                $"{_products.Count} ürün bulundu";

            return;
        }

        var filteredProducts = _products
            .Where(product =>
                product.Name.Contains(
                    searchText,
                    StringComparison.CurrentCultureIgnoreCase))
            .ToList();

        ProductsCollectionView.ItemsSource =
            filteredProducts;

        ProductCountLabel.Text =
            $"{filteredProducts.Count} ürün bulundu";
    }

    private static HomeProductItem CreateProductItem(
        Product product,
        Discount? discount)
    {
        return new HomeProductItem
        {
            Id = product.Id,
            Name = product.Name,
            Unit = product.Unit,
            ImageName = product.ImageName,
            Price = product.Price,
            DiscountRate = discount?.DiscountRate ?? 0
        };
    }

    private async void OnBackTapped(
        object sender,
        TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
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

            await Task.Delay(900);
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);

            await DisplayAlert(
                "Bir Hata Oluştu",
                "Ürün sepete eklenemedi. Lütfen tekrar dene.",
                "Tamam");
        }
        finally
        {
            button.Text = "+ Ekle";
            button.IsEnabled = true;
        }
    }
}