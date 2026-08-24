using Microsoft.Extensions.DependencyInjection;
using Tazora.Pages;

namespace Tazora;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(ProductListPage), typeof(ProductListPage));

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