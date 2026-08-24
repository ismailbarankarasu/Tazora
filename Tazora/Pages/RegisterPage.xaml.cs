using System.Net.Mail;
using Tazora.Helpers;
using Tazora.Services;

namespace Tazora.Pages;

public partial class RegisterPage : ContentPage
{
    private readonly DatabaseService _databaseService;

    private bool _isPasswordVisible;
    private bool _isRegistering;

    public RegisterPage(DatabaseService databaseService)
    {
        InitializeComponent();

        _databaseService = databaseService;
    }

    private async void OnRegisterClicked(
        object sender,
        EventArgs e)
    {
        if (_isRegistering)
            return;

        HideValidationMessages();

        if (!ValidateForm())
            return;

        try
        {
            SetLoadingState(true);

            await _databaseService.RegisterUserAsync(
                FullNameEntry.Text!,
                EmailEntry.Text!,
                PhoneEntry.Text,
                PasswordEntry.Text!);

            await DisplayAlert(
                "Kayıt Başarılı",
                "Hesabın başarıyla oluşturuldu.",
                "Tamam");

            ClearForm();
        }
        catch (InvalidOperationException exception)
        {
            EmailErrorLabel.Text = exception.Message;
            EmailErrorLabel.IsVisible = true;
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);

            await DisplayAlert(
                "Bir Hata Oluştu",
                "Kayıt işlemi tamamlanamadı. Lütfen tekrar dene.",
                "Tamam");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private bool ValidateForm()
    {
        var isValid = true;

        var fullName = FullNameEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(fullName))
        {
            FullNameErrorLabel.Text =
                "Ad soyad alanı zorunludur.";

            FullNameErrorLabel.IsVisible = true;
            isValid = false;
        }
        else if (fullName.Length < 3)
        {
            FullNameErrorLabel.Text =
                "Ad soyad en az 3 karakter olmalıdır.";

            FullNameErrorLabel.IsVisible = true;
            isValid = false;
        }

        var email = EmailEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(email) ||
            !MailAddress.TryCreate(email, out _))
        {
            EmailErrorLabel.Text =
                "Geçerli bir e-posta adresi gir.";

            EmailErrorLabel.IsVisible = true;
            isValid = false;
        }

        if (!IsPhoneNumberValid(PhoneEntry.Text))
        {
            PhoneErrorLabel.Text =
                "Geçerli bir telefon numarası gir.";

            PhoneErrorLabel.IsVisible = true;
            isValid = false;
        }

        var password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(password))
        {
            PasswordErrorLabel.Text =
                "Şifre alanı zorunludur.";

            PasswordErrorLabel.IsVisible = true;
            isValid = false;
        }
        else if (password.Length < 8)
        {
            PasswordErrorLabel.Text =
                "Şifre en az 8 karakter olmalıdır.";

            PasswordErrorLabel.IsVisible = true;
            isValid = false;
        }

        if (!TermsCheckBox.IsChecked)
        {
            TermsErrorLabel.IsVisible = true;
            isValid = false;
        }

        return isValid;
    }

    private static bool IsPhoneNumberValid(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        var digits = new string(
            phoneNumber
                .Where(char.IsDigit)
                .ToArray());

        return digits.Length is 10 or 11;
    }

    private void HideValidationMessages()
    {
        FullNameErrorLabel.IsVisible = false;
        EmailErrorLabel.IsVisible = false;
        PhoneErrorLabel.IsVisible = false;
        PasswordErrorLabel.IsVisible = false;
        TermsErrorLabel.IsVisible = false;
    }

    private void SetLoadingState(bool isLoading)
    {
        _isRegistering = isLoading;

        RegisterButton.IsEnabled = !isLoading;
        RegisterButton.Text = isLoading
            ? "Hesap Oluşturuluyor..."
            : "Kayıt Ol";

        RegisterActivityIndicator.IsVisible = isLoading;
        RegisterActivityIndicator.IsRunning = isLoading;

        FullNameEntry.IsEnabled = !isLoading;
        EmailEntry.IsEnabled = !isLoading;
        PhoneEntry.IsEnabled = !isLoading;
        PasswordEntry.IsEnabled = !isLoading;
        TermsCheckBox.IsEnabled = !isLoading;
    }

    private void ClearForm()
    {
        FullNameEntry.Text = string.Empty;
        EmailEntry.Text = string.Empty;
        PhoneEntry.Text = string.Empty;
        PasswordEntry.Text = string.Empty;
        TermsCheckBox.IsChecked = false;

        HideValidationMessages();
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

    private async void OnBackTapped(
        object sender,
        TappedEventArgs e)
    {
        if (Navigation.NavigationStack.Count > 1)
        {
            await Navigation.PopAsync();
        }
    }
}