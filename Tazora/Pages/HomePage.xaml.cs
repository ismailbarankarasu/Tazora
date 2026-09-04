using Tazora.Models;
using Tazora.Services;
using Tazora.ViewModels;

namespace Tazora.Pages;

public partial class HomePage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private readonly AppSession _appSession;
    private bool _isLoaded;

    public HomePage(
        DatabaseService databaseService,
        AppSession appSession)
    {
        InitializeComponent();

        _databaseService = databaseService;
        _appSession = appSession;

        SetGreeting();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isLoaded)
            await LoadHomeDataAsync();
    }

    private void SetGreeting()
    {
        var fullName =
            _appSession.CurrentUser?.FullName?.Trim();

        if (string.IsNullOrWhiteSpace(fullName))
        {
            GreetingLabel.Text = "Merhaba 👋";
            return;
        }

        var firstName =
            fullName
                .Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();

        GreetingLabel.Text =
            $"Merhaba, {firstName} 👋";
    }

    private async Task LoadHomeDataAsync()
    {
        try
        {
            CategoryErrorBorder.IsVisible = false;

            CategoryLoadingIndicator.IsVisible = true;
            CategoryLoadingIndicator.IsRunning = true;

            var categories =
                await _databaseService.GetCategoriesAsync();

            var products =
                await _databaseService.GetProductsAsync();

            var popularProducts =
                await _databaseService
                    .GetPopularProductsAsync();

            var discounts =
                await _databaseService
                    .GetActiveDiscountsAsync();

            BindableLayout.SetItemsSource(
                CategoryContainer,
                categories);

            var discountsByProductId =
                discounts
                    .Where(discount =>
                        discount.ProductId.HasValue)
                    .GroupBy(discount =>
                        discount.ProductId!.Value)
                    .ToDictionary(
                        group => group.Key,
                        group => group
                            .OrderByDescending(
                                item => item.DiscountRate)
                            .First());

            var discountedProducts =
                products
                    .Where(product =>
                        discountsByProductId
                            .ContainsKey(product.Id))
                    .Select(product =>
                        CreateProductItem(
                            product,
                            discountsByProductId[product.Id]))
                    .ToList();

            var popularItems =
                popularProducts
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

            BindableLayout.SetItemsSource(
                DiscountedProductContainer,
                discountedProducts);

            PopularProductContainer.ItemsSource = popularItems;

            _isLoaded = true;
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);

            CategoryErrorBorder.IsVisible = true;
        }
        finally
        {
            CategoryLoadingIndicator.IsRunning = false;
            CategoryLoadingIndicator.IsVisible = false;
        }
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
            DiscountRate =
                discount?.DiscountRate ?? 0
        };
    }

    private async void OnCategoryTapped(
        object sender,
        TappedEventArgs e)
    {
        if (e.Parameter is not Category selectedCategory)
            return;

        var categoryName =
            Uri.EscapeDataString(
                selectedCategory.Name);

        await Shell.Current.GoToAsync(
            $"{nameof(ProductListPage)}" +
            $"?categoryId={selectedCategory.Id}" +
            $"&categoryName={categoryName}");
    }

    private async void OnProductTapped(
        object sender,
        TappedEventArgs e)
    {
        if (e.Parameter is not HomeProductItem product)
            return;

        await Shell.Current.GoToAsync(
            $"{nameof(ProductDetailPage)}" +
            $"?productId={product.Id}");
    }

    private async void OnSearchTapped(
        object sender,
        TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(CategoriesPage));
    }

    private async void OnAddDiscountedProductClicked(
        object sender,
        EventArgs e)
    {
        if (sender is not Button button ||
            button.BindingContext is not HomeProductItem product)
        {
            return;
        }

        try
        {
            button.IsEnabled = false;
            button.Text = "Ekleniyor...";

            await _databaseService
                .AddProductToBasketAsync(
                    product.Id,
                    1);

            button.Text = "✓ Eklendi";

            await Task.Delay(700);
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
            button.Text = "+ Ekle";
            button.IsEnabled = true;
        }
    }

    private async void OnAddPopularProductClicked(
    object sender,
    TappedEventArgs e)
    {
        if (sender is not Border border ||
            border.BindingContext is not HomeProductItem product)
        {
            return;
        }

        try
        {
            border.IsEnabled = false;

            await _databaseService
                .AddProductToBasketAsync(
                    product.Id,
                    1);

            await DisplayAlert(
                "Sepete Eklendi",
                $"{product.Name} sepete eklendi.",
                "Tamam");
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
            border.IsEnabled = true;
        }
    }
    private async void OnDiscountsTapped(
        object sender,
        TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(DiscountsPage));
    }
}