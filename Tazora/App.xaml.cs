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
    }

    protected override Window CreateWindow(
        IActivationState? activationState)
    {
        var registerPage =
            _serviceProvider.GetRequiredService<RegisterPage>();

        return new Window(
            new NavigationPage(registerPage));
    }
}