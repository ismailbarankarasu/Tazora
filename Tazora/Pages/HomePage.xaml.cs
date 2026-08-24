using Tazora.Services;

namespace Tazora.Pages;

public partial class HomePage : ContentPage
{
    public HomePage(AppSession appSession)
    {
        InitializeComponent();

        if (appSession.CurrentUser is not null)
        {
            Title = appSession.CurrentUser.FullName;
        }
    }
}