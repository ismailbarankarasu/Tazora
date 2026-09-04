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
    private List<Category> _categories = [];
    private int _selectedCategoryId; // 0 = Tümü
    private bool _isLoaded;

    public ProductListPage(DatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("categoryId", out var categoryIdValue) &&
            int.TryParse(categoryIdValue?.ToString(), out var categoryId))
        {
            _selectedCategoryId = categoryId;
        }
        else
        {
            _selectedCategoryId = 0; // parametresiz açılırsa: Tümü
        }

        _isLoaded = false;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isLoaded)
        {
            await LoadCategoriesAsync();
            await LoadProductsAsync();
        }
    }
    private async Task LoadCategoriesAsync()
    {
        _categories = await _databaseService.GetCategoriesAsync();
        BuildCategoryChips();
    }
    private void BuildCategoryChips()
    {
        var chips = new List<CategoryChipItem>
        {
            new() { Id = 0, Name = "Tümü", IsSelected = _selectedCategoryId == 0 }
        };

        chips.AddRange(_categories.Select(category => new CategoryChipItem
        {
            Id = category.Id,
            Name = category.Name,
            IsSelected = category.Id == _selectedCategoryId
        }));

        BindableLayout.SetItemsSource(CategoryChipContainer, chips);
    }
    private async void OnCategoryChipTapped(object sender, TappedEventArgs e)
    {
        if (sender is not Border border || border.GestureRecognizers.FirstOrDefault()
                is not TapGestureRecognizer { CommandParameter: int categoryId })
        {
            return;
        }

        if (categoryId == _selectedCategoryId)
            return;

        _selectedCategoryId = categoryId;
        BuildCategoryChips();

        CategoryTitleLabel.Text = categoryId == 0
            ? "Tüm Ürünler"
            : _categories.First(c => c.Id == categoryId).Name;

        await LoadProductsAsync();
    }
    private async Task LoadProductsAsync()
    {
        try
        {
            ProductErrorBorder.IsVisible = false;
            ProductLoadingIndicator.IsVisible = true;
            ProductLoadingIndicator.IsRunning = true;

            var products = _selectedCategoryId == 0
                ? await _databaseService.GetProductsAsync()
                : await _databaseService.GetProductsByCategoryAsync(_selectedCategoryId);

            var discounts = await _databaseService.GetActiveDiscountsAsync();

            var discountsByProductId = discounts
                .Where(discount => discount.ProductId.HasValue)
                .GroupBy(discount => discount.ProductId!.Value)
                .ToDictionary(
                    group => group.Key,
                    group => group.OrderByDescending(item => item.DiscountRate).First());

            _products = products
                .Select(product =>
                {
                    discountsByProductId.TryGetValue(product.Id, out var discount);
                    return CreateProductItem(product, discount);
                })
                .ToList();

            ProductsCollectionView.ItemsSource = _products;
            ProductCountLabel.Text = $"{_products.Count} ürün bulundu";

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

    private async void OnProductSelectionChanged(
    object sender,
    SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault()
            is not HomeProductItem selectedProduct)
        {
            return;
        }

        ProductsCollectionView.SelectedItem = null;

        await Shell.Current.GoToAsync(
            $"{nameof(ProductDetailPage)}" +
            $"?productId={selectedProduct.Id}");
    }
}