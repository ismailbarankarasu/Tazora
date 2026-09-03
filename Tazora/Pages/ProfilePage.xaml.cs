using Microsoft.Extensions.DependencyInjection;
using Tazora.Helpers;
using Tazora.Services;
using Tazora.ViewModels;

namespace Tazora.Pages;

public partial class ProfilePage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private readonly AppSession _appSession;
    private readonly IServiceProvider _serviceProvider;

    public ProfilePage(
        DatabaseService databaseService,
        AppSession appSession,
        IServiceProvider serviceProvider)
    {
        InitializeComponent();

        _databaseService = databaseService;
        _appSession = appSession;
        _serviceProvider = serviceProvider;

        CreateMenuItems();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        LoadUserInformation();
        await LoadProfileStatisticsAsync();
    }

    private void LoadUserInformation()
    {
        var user = _appSession.CurrentUser;

        if (user is null)
        {
            ProfileNameLabel.Text = "Kullanıcı";
            ProfileContactLabel.Text = "Oturum bulunamadı";
            return;
        }

        ProfileNameLabel.Text = user.FullName;

        ProfileContactLabel.Text =
            string.IsNullOrWhiteSpace(user.PhoneNumber)
                ? user.Email
                : $"{user.PhoneNumber} • {user.Email}";
    }

    private async Task LoadProfileStatisticsAsync()
    {
        var user = _appSession.CurrentUser;

        if (user is null)
            return;

        try
        {
            var orders =
                await _databaseService
                    .GetOrdersByUserAsync(user.Id);

            var basketItems =
                await _databaseService
                    .GetBasketItemsAsync();

            var discountCount =
                await _databaseService
                    .GetActiveDiscountCountAsync();

            OrderCountLabel.Text =
                orders.Count.ToString();

            BasketCountLabel.Text =
                basketItems
                    .Sum(item => item.Quantity)
                    .ToString();

            DiscountCountLabel.Text =
                discountCount.ToString();
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);
        }
    }

    private void CreateMenuItems()
    {
        var menuItems = new List<ProfileMenuItem>
        {
            new()
            {
                Key = "orders",
                Title = "Siparişlerim",
                Subtitle = "Geçmiş ve mevcut siparişlerini görüntüle",
                Icon = IconFont.Receipt
            },
            new()
            {
                Key = "addresses",
                Title = "Adreslerim",
                Subtitle = "Teslimat adreslerini yönet",
                Icon = IconFont.Location
            },
            new()
            {
                Key = "payments",
                Title = "Ödeme Yöntemlerim",
                Subtitle = "Kayıtlı ödeme seçeneklerini görüntüle",
                Icon = IconFont.CreditCard
            },
            new()
            {
                Key = "favorites",
                Title = "Favorilerim",
                Subtitle = "Beğendiğin ürünleri görüntüle",
                Icon = IconFont.Favorite
            },
            new()
            {
                Key = "settings",
                Title = "Ayarlar",
                Subtitle = "Uygulama tercihlerini düzenle",
                Icon = IconFont.Settings
            },
            new()
            {
                Key = "statistics",
                Title = "İstatistikler",
                Subtitle = "Ürün, sepet, indirim ve sipariş özetini gör",
                Icon = IconFont.Categories
            },
            new()
            {
                Key = "help",
                Title = "Yardım",
                Subtitle = "Destek ve sık sorulan sorular",
                Icon = IconFont.Help
            }
        };

        BindableLayout.SetItemsSource(
            ProfileMenuContainer,
            menuItems);
    }

    private async void OnMenuItemTapped(
        object sender,
        TappedEventArgs e)
    {
        var menuKey =
            e.Parameter?.ToString();

        if (menuKey == "orders")
        {
            await Shell.Current.GoToAsync(
                nameof(OrdersPage));

            return;
        }
        if (menuKey == "statistics")
        {
            await Shell.Current.GoToAsync(
                nameof(StatisticsPage));

            return;
        }
        await DisplayAlert(
            "Tazora",
            "Bu bölüm case kapsamında görsel olarak hazırlanmıştır.",
            "Tamam");
    }

    private async void OnEditProfileClicked(
        object sender,
        EventArgs e)
    {
        await DisplayAlert(
            "Profili Düzenle",
            "Profil düzenleme ekranı case kapsamında değildir.",
            "Tamam");
    }

    private async void OnLogoutClicked(
        object sender,
        EventArgs e)
    {
        var shouldLogout = await DisplayAlert(
            "Çıkış Yap",
            "Hesabından çıkmak istediğine emin misin?",
            "Çıkış Yap",
            "Vazgeç");

        if (!shouldLogout)
            return;

        _appSession.Clear();

        var loginPage =
            _serviceProvider.GetRequiredService<LoginPage>();

        var currentWindow =
            Application.Current?
                .Windows
                .FirstOrDefault();

        if (currentWindow is not null)
        {
            currentWindow.Page =
                new NavigationPage(loginPage);
        }
    }
}