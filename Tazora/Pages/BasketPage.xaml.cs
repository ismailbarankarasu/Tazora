using Tazora.Models;
using Tazora.Services;
using Tazora.ViewModels;

namespace Tazora.Pages;

public partial class BasketPage : ContentPage
{
    private const decimal FreeDeliveryLimit = 300m;
    private const decimal DeliveryFee = 19.90m;

    private readonly DatabaseService _databaseService;
    private readonly AppSession _appSession;
    private List<BasketDisplayItem> _basketItems = [];
    private bool _isUpdating;

    public BasketPage(DatabaseService databaseService, AppSession appSession)
    {
        InitializeComponent();
        _databaseService = databaseService;
        _appSession = appSession;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadBasketAsync();
    }

    private async Task LoadBasketAsync()
    {
        try
        {
            BasketLoadingIndicator.IsVisible = true;
            BasketLoadingIndicator.IsRunning = true;

            var basketItems =
                await _databaseService.GetBasketItemsAsync();

            var products =
                await _databaseService.GetProductsAsync();

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

            _basketItems = basketItems
                .Where(item =>
                    productsById.ContainsKey(item.ProductId))
                .Select(item =>
                {
                    var product =
                        productsById[item.ProductId];

                    discountsByProductId.TryGetValue(
                        product.Id,
                        out var discount);

                    return new BasketDisplayItem
                    {
                        ProductId = product.Id,
                        Name = product.Name,
                        Unit = product.Unit,
                        ImageName = product.ImageName,
                        Price = product.Price,
                        DiscountRate =
                            discount?.DiscountRate ?? 0,
                        Quantity = item.Quantity
                    };
                })
                .ToList();

            BasketCollectionView.ItemsSource =
                _basketItems;

            UpdateSummary();
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);

            await DisplayAlert(
                "Bir Hata Oluştu",
                "Sepet bilgileri yüklenemedi.",
                "Tamam");
        }
        finally
        {
            BasketLoadingIndicator.IsRunning = false;
            BasketLoadingIndicator.IsVisible = false;
        }
    }

    private void UpdateSummary()
    {
        var totalQuantity =
            _basketItems.Sum(item => item.Quantity);

        var subtotal =
            _basketItems.Sum(item => item.LineTotal);

        var deliveryFee =
            subtotal == 0 || subtotal >= FreeDeliveryLimit
                ? 0
                : DeliveryFee;

        var total =
            subtotal + deliveryFee;

        BasketItemCountLabel.Text =
            $"{totalQuantity} ürün";

        BasketTotalLabel.Text =
            $"{total:N2} TL";

        CheckoutButton.IsEnabled =
            _basketItems.Count > 0;

        FreeDeliveryProgressBar.Progress =
            subtotal <= 0
                ? 0
                : (double)Math.Min(
                    subtotal / FreeDeliveryLimit,
                    1);

        if (subtotal <= 0)
        {
            FreeDeliveryLabel.Text =
                "Ücretsiz teslimat için sepetine ürün ekle";
        }
        else if (subtotal >= FreeDeliveryLimit)
        {
            FreeDeliveryLabel.Text =
                "Ücretsiz teslimat kazandın!";
        }
        else
        {
            var remaining =
                FreeDeliveryLimit - subtotal;

            FreeDeliveryLabel.Text =
                $"Ücretsiz teslimat için {remaining:N2} TL daha ekle";
        }
    }

    private async void OnIncreaseClicked(
        object sender,
        EventArgs e)
    {
        if (_isUpdating ||
            sender is not Button button ||
            button.CommandParameter is not int productId)
        {
            return;
        }

        var item = _basketItems
            .FirstOrDefault(item =>
                item.ProductId == productId);

        if (item is null)
            return;

        await UpdateQuantityAsync(
            productId,
            item.Quantity + 1);
    }

    private async void OnDecreaseClicked(
        object sender,
        EventArgs e)
    {
        if (_isUpdating ||
            sender is not Button button ||
            button.CommandParameter is not int productId)
        {
            return;
        }

        var item = _basketItems
            .FirstOrDefault(item =>
                item.ProductId == productId);

        if (item is null)
            return;

        await UpdateQuantityAsync(
            productId,
            item.Quantity - 1);
    }

    private async Task UpdateQuantityAsync(
        int productId,
        int quantity)
    {
        try
        {
            _isUpdating = true;

            await _databaseService
                .UpdateBasketQuantityAsync(
                    productId,
                    quantity);

            await LoadBasketAsync();
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private async void OnRemoveClicked(
        object sender,
        EventArgs e)
    {
        if (_isUpdating ||
            sender is not Button button ||
            button.CommandParameter is not int productId)
        {
            return;
        }

        try
        {
            _isUpdating = true;

            await _databaseService
                .RemoveProductFromBasketAsync(productId);

            await LoadBasketAsync();
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private async void OnClearBasketTapped(
        object sender,
        TappedEventArgs e)
    {
        if (_basketItems.Count == 0)
            return;

        var shouldClear = await DisplayAlert(
            "Sepeti Temizle",
            "Sepetteki tüm ürünler silinsin mi?",
            "Temizle",
            "Vazgeç");

        if (!shouldClear)
            return;

        await _databaseService.ClearBasketAsync();
        await LoadBasketAsync();
    }

    private async void OnCheckoutClicked(
        object sender,
        EventArgs e)
    {
        if (_isUpdating || _basketItems.Count == 0)
            return;

        var currentUser =
            _appSession.CurrentUser;

        if (currentUser is null)
        {
            await DisplayAlert(
                "Oturum Gerekli",
                "Sipariş oluşturmak için giriş yapmalısın.",
                "Tamam");

            return;
        }

        var shouldCreateOrder = await DisplayAlert(
            "Siparişi Tamamla",
            "Siparişini oluşturmak istiyor musun?",
            "Sipariş Ver",
            "Vazgeç");

        if (!shouldCreateOrder)
            return;

        try
        {
            _isUpdating = true;
            CheckoutButton.IsEnabled = false;
            CheckoutButton.Text = "Hazırlanıyor...";

            var orderId =
                await _databaseService
                    .CreateOrderFromBasketAsync(
                        currentUser.Id);

            await DisplayAlert(
                "Sipariş Oluşturuldu",
                $"#{orderId} numaralı siparişin hazırlanıyor.",
                "Tamam");

            await LoadBasketAsync();
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);

            await DisplayAlert(
                "Bir Hata Oluştu",
                "Sipariş oluşturulamadı. Lütfen tekrar dene.",
                "Tamam");
        }
        finally
        {
            _isUpdating = false;
            CheckoutButton.Text = "Siparişi Tamamla";
            CheckoutButton.IsEnabled =
                _basketItems.Count > 0;
        }
    }
}