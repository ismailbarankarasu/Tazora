using Tazora.Models;
using Tazora.Services;

namespace Tazora.Pages;

public partial class ProductDetailPage
    : ContentPage, IQueryAttributable
{
    private readonly DatabaseService _databaseService;

    private Product? _product;
    private Discount? _discount;
    private int _productId;
    private int _quantity = 1;
    private bool _isLoaded;
    private bool _isAdding;

    public ProductDetailPage(
        DatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;
    }

    public void ApplyQueryAttributes(
        IDictionary<string, object> query)
    {
        if (query.TryGetValue(
                "productId",
                out var productIdValue) &&
            int.TryParse(
                productIdValue?.ToString(),
                out var productId))
        {
            _productId = productId;
            _isLoaded = false;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isLoaded && _productId > 0)
        {
            await LoadProductAsync();
        }
    }

    private async Task LoadProductAsync()
    {
        try
        {
            ProductLoadingIndicator.IsVisible = true;
            ProductLoadingIndicator.IsRunning = true;

            _product =
                await _databaseService
                    .GetProductByIdAsync(_productId);

            if (_product is null)
            {
                await DisplayAlert(
                    "Ürün Bulunamadı",
                    "Bu ürün artık görüntülenemiyor.",
                    "Tamam");

                await Shell.Current.GoToAsync("..");
                return;
            }

            _discount =
                await _databaseService
                    .GetDiscountByProductIdAsync(_product.Id);

            ProductNameLabel.Text =
                _product.Name;

            ProductUnitLabel.Text =
                _product.Unit;

            ProductDescriptionLabel.Text =
                string.IsNullOrWhiteSpace(_product.Description)
                    ? "Bu ürün için açıklama bulunmuyor."
                    : _product.Description;

            var hasDiscount =
                _discount is not null &&
                _discount.DiscountRate > 0;

            DiscountBadge.IsVisible = hasDiscount;
            OriginalPriceLabel.IsVisible = hasDiscount;

            if (hasDiscount)
            {
                DiscountLabel.Text =
                    $"%{_discount!.DiscountRate}";

                OriginalPriceLabel.Text =
                    $"{_product.Price:N2} TL";
            }

            _quantity = 1;
            UpdateQuantityArea();

            _isLoaded = true;
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);

            await DisplayAlert(
                "Bir Hata Oluştu",
                "Ürün bilgileri yüklenemedi.",
                "Tamam");
        }
        finally
        {
            ProductLoadingIndicator.IsRunning = false;
            ProductLoadingIndicator.IsVisible = false;
        }
    }

    private decimal GetUnitPrice()
    {
        if (_product is null)
            return 0;

        if (_discount is null ||
            _discount.DiscountRate <= 0)
        {
            return _product.Price;
        }

        return Math.Round(
            _product.Price *
            (100 - _discount.DiscountRate) / 100,
            2);
    }

    private void UpdateQuantityArea()
    {
        var unitPrice = GetUnitPrice();
        var total = unitPrice * _quantity;

        QuantityLabel.Text =
            _quantity.ToString();

        ProductPriceLabel.Text =
            $"{unitPrice:N2} TL";

        AddToBasketButton.Text =
            $"Sepete Ekle • {total:N2} TL";
    }

    private void OnIncreaseClicked(
        object sender,
        EventArgs e)
    {
        if (_quantity >= 10)
            return;

        _quantity++;
        UpdateQuantityArea();
    }

    private void OnDecreaseClicked(
        object sender,
        EventArgs e)
    {
        if (_quantity <= 1)
            return;

        _quantity--;
        UpdateQuantityArea();
    }

    private async void OnAddToBasketClicked(
        object sender,
        EventArgs e)
    {
        if (_product is null || _isAdding)
            return;

        try
        {
            _isAdding = true;
            AddToBasketButton.IsEnabled = false;
            AddToBasketButton.Text = "Ekleniyor...";

            var basketQuantity =
                await _databaseService
                    .AddProductToBasketAsync(
                        _product.Id,
                        _quantity);

            AddToBasketButton.Text =
                $"Sepette: {basketQuantity}";

            await Task.Delay(900);

            _quantity = 1;
            UpdateQuantityArea();
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);

            await DisplayAlert(
                "Bir Hata Oluştu",
                "Ürün sepete eklenemedi.",
                "Tamam");
        }
        finally
        {
            _isAdding = false;
            AddToBasketButton.IsEnabled = true;
            UpdateQuantityArea();
        }
    }

    private async void OnBackTapped(
        object sender,
        TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}