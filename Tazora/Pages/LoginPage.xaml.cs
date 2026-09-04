using System.Net.Mail;
using Microsoft.Extensions.DependencyInjection;
using Tazora.Helpers;
using Tazora.Services;

namespace Tazora.Pages;

public partial class LoginPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private readonly IServiceProvider _serviceProvider;
    private readonly AppSession _appSession;
    private bool _isPasswordVisible;
    private bool _isLoggingIn;

    public LoginPage(
        DatabaseService databaseService,
        IServiceProvider serviceProvider,
        AppSession appSession)
    {
        InitializeComponent();

        _databaseService = databaseService;
        _serviceProvider = serviceProvider;
        _appSession = appSession;
    }

    private async void OnLoginClicked(
        object sender,
        EventArgs e)
    {
        await LoginAsync();
    }

    private async void OnPasswordCompleted(
        object sender,
        EventArgs e)
    {
        await LoginAsync();
    }

    private async Task LoginAsync()
    {
        if (_isLoggingIn)
            return;

        HideValidationMessages();

        if (!ValidateForm())
            return;

        try
        {
            SetLoadingState(true);

            var user = await _databaseService.LoginAsync(
                EmailEntry.Text!,
                PasswordEntry.Text!);

            if (user is null)
            {
                LoginErrorLabel.Text =
                    "E-posta veya şifre hatalı.";

                LoginErrorBorder.IsVisible = true;
                return;
            }
            _appSession.Start(user);

            var appShell =
                _serviceProvider.GetRequiredService<AppShell>();

            var currentWindow =
                Application.Current?.Windows.FirstOrDefault();

            if (currentWindow is null)
            {
                throw new InvalidOperationException(
                    "Uygulama penceresi bulunamadı.");
            }

            currentWindow.Page = appShell;

            // Ana sayfayı oluşturduğumuzda buradan yönlendireceğiz.
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"GIRIS HATA DETAYI: {exception.Message}");
            System.Diagnostics.Debug.WriteLine(exception);

            LoginErrorLabel.Text =
                "Giriş işlemi tamamlanamadı. Tekrar dene.";

            LoginErrorBorder.IsVisible = true;
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private bool ValidateForm()
    {
        var isValid = true;
        var email = EmailEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(email) ||
            !MailAddress.TryCreate(email, out _))
        {
            EmailErrorLabel.IsVisible = true;
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            PasswordErrorLabel.IsVisible = true;
            isValid = false;
        }

        return isValid;
    }

    private void HideValidationMessages()
    {
        EmailErrorLabel.IsVisible = false;
        PasswordErrorLabel.IsVisible = false;
        LoginErrorBorder.IsVisible = false;
    }

    private void SetLoadingState(bool isLoading)
    {
        _isLoggingIn = isLoading;

        EmailEntry.IsEnabled = !isLoading;
        PasswordEntry.IsEnabled = !isLoading;
        LoginButton.IsEnabled = !isLoading;

        LoginButton.Text = isLoading
            ? "Giriş Yapılıyor..."
            : "Giriş Yap";

        LoginActivityIndicator.IsVisible = isLoading;
        LoginActivityIndicator.IsRunning = isLoading;
    }

    private void OnPasswordVisibilityTapped(
        object sender,
        TappedEventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;

        PasswordEntry.IsPassword = !_isPasswordVisible;

        PasswordVisibilityIcon.Text = _isPasswordVisible
            ? IconFont.Visibility
            : IconFont.VisibilityOff;
    }

    private async void OnGoToRegisterTapped(
        object sender,
        TappedEventArgs e)
    {
        var registerPage =
            _serviceProvider.GetRequiredService<RegisterPage>();

        await Navigation.PushAsync(registerPage);
    }
}