using Microsoft.Extensions.DependencyInjection;
using Tazora.Pages;

namespace Tazora;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(ProductListPage), typeof(ProductListPage));
        Routing.RegisterRoute(nameof(OrdersPage), typeof(OrdersPage));
        Routing.RegisterRoute(nameof(OrderDetailPage), typeof(OrderDetailPage));
        Routing.RegisterRoute(nameof(ProductDetailPage), typeof(ProductDetailPage));
        Routing.RegisterRoute(nameof(DiscountsPage), typeof(DiscountsPage));
        Routing.RegisterRoute(nameof(StatisticsPage), typeof(StatisticsPage));
        HomeShellContent.Content =
            serviceProvider.GetRequiredService<HomePage>();

        CategoriesShellContent.Content =
            serviceProvider.GetRequiredService<CategoriesPage>();

        BasketShellContent.Content =
            serviceProvider.GetRequiredService<BasketPage>();

        ProfileShellContent.Content =
            serviceProvider.GetRequiredService<ProfilePage>();
    }
}