using Tazora.Models;
using Tazora.Services;

namespace Tazora.Pages;

public partial class CategoriesPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private List<Category> _categories = [];
    private bool _isLoaded;

    public CategoriesPage(DatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isLoaded)
        {
            await LoadCategoriesAsync();
        }
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            CategoryErrorBorder.IsVisible = false;
            CategoryLoadingIndicator.IsVisible = true;
            CategoryLoadingIndicator.IsRunning = true;

            _categories =
                await _databaseService.GetCategoriesAsync();

            CategoriesCollectionView.ItemsSource =
                _categories;

            CategoryCountLabel.Text =
                $"{_categories.Count} kategori bulundu";

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

    private void OnSearchTextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        var searchText =
            e.NewTextValue?.Trim();

        if (string.IsNullOrWhiteSpace(searchText))
        {
            CategoriesCollectionView.ItemsSource =
                _categories;

            CategoryCountLabel.Text =
                $"{_categories.Count} kategori bulundu";

            return;
        }

        var filteredCategories = _categories
            .Where(category =>
                category.Name.Contains(
                    searchText,
                    StringComparison.CurrentCultureIgnoreCase))
            .ToList();

        CategoriesCollectionView.ItemsSource =
            filteredCategories;

        CategoryCountLabel.Text =
            $"{filteredCategories.Count} kategori bulundu";
    }

    private async void OnCategorySelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault()
            is not Category selectedCategory)
        {
            return;
        }

        CategoriesCollectionView.SelectedItem = null;

        var categoryName =
            Uri.EscapeDataString(selectedCategory.Name);

        await Shell.Current.GoToAsync(
            $"{nameof(ProductListPage)}" +
            $"?categoryId={selectedCategory.Id}" +
            $"&categoryName={categoryName}");
    }
}