using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace Tazora.Pages
{
    public partial class SpinWheelPage : ContentPage
    {
        private bool _isSpinning = false;
        private readonly Random _random = new();

        // 8 Parçalı Çarka Uygun İndirim Ödülleri
        private readonly (string Description, string Code)[] _rewards = new[]
        {
            ("Sepette %20 İndirim!", "TAZORA20"),
            ("850 TL Üzeri 250 TL İndirim!", "FIRSAT250"),
            ("Tüm Siparişlerde Ücretsiz Kargo!", "BEDAVAKARGO"),
            ("Sepette %15 İndirim!", "TAZORA15"),
            ("500 TL Üzeri 100 TL İndirim!", "INDIRIM100"),
            ("Sepette %10 İndirim!", "TAZORA10"),
            ("Sürpriz Hediye Ürün!", "SURPRIZHEDIYE"),
            ("Sepette %25 İndirim!", "MEGA25")
        };

        public SpinWheelPage()
        {
            InitializeComponent();
        }

        private async void OnSpinClicked(object sender, EventArgs e)
        {
            if (_isSpinning) return;

            _isSpinning = true;
            SpinButton.IsEnabled = false;
            RewardCard.IsVisible = false;

            int selectedIndex = _random.Next(0, _rewards.Length);
            var selectedReward = _rewards[selectedIndex];


            double sliceAngle = 360.0 / 8.0; // 45°
            double targetAngle = 1800 + (selectedIndex * sliceAngle);

            WheelImage.Rotation = 0;
            await WheelImage.RelRotateTo(targetAngle, 4000, Easing.CubicOut);

            RewardTitleLabel.Text = selectedReward.Description;
            CouponCodeLabel.Text = selectedReward.Code;
            RewardCard.IsVisible = true;

            _isSpinning = false;
            SpinButton.IsEnabled = true;
        }

        private async void OnCopyCouponTapped(object sender, EventArgs e)
        {
            await Clipboard.SetTextAsync(CouponCodeLabel.Text);
            await DisplayAlert("Başarılı", "Kupon kodu panoya kopyalandı!", "Tamam");
        }

        private async void OnStartShoppingClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }

        private async void OnBackTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}