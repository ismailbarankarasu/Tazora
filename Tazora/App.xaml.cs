using Microsoft.Extensions.DependencyInjection;
using Tazora.Pages;

namespace Tazora;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        UserAppTheme = AppTheme.Light;
    }

    protected override Window CreateWindow(
        IActivationState? activationState)
    {
        var loginPage =
            _serviceProvider.GetRequiredService<LoginPage>();

        return new Window(
            new NavigationPage(loginPage));
    }
}