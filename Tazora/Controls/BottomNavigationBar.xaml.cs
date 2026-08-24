namespace Tazora.Controls;

public partial class BottomNavigationBar : ContentView
{
    public BottomNavigationBar()
    {
        InitializeComponent();
    }

    private async void OnHomeTapped(
        object sender,
        TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//home");
    }

    private async void OnCategoriesTapped(
        object sender,
        TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//categories");
    }

    private async void OnBasketTapped(
        object sender,
        TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//basket");
    }

    private async void OnProfileTapped(
        object sender,
        TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//profile");
    }
}