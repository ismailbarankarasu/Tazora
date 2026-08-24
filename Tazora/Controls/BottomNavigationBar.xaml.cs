namespace Tazora.Controls;

public partial class BottomNavigationBar : ContentView
{
    public static readonly BindableProperty ActiveTabProperty =
        BindableProperty.Create(
            nameof(ActiveTab),
            typeof(string),
            typeof(BottomNavigationBar),
            string.Empty,
            propertyChanged: OnActiveTabChanged);

    public string ActiveTab
    {
        get => (string)GetValue(ActiveTabProperty);
        set => SetValue(ActiveTabProperty, value);
    }

    public BottomNavigationBar()
    {
        InitializeComponent();
        UpdateActiveTab();
    }

    private static void OnActiveTabChanged(
        BindableObject bindable,
        object oldValue,
        object newValue)
    {
        if (bindable is BottomNavigationBar navigationBar)
        {
            navigationBar.UpdateActiveTab();
        }
    }

    private void UpdateActiveTab()
    {
        SetTabState(
            HomeIconLabel,
            HomeTextLabel,
            ActiveTab == "Home");

        SetTabState(
            CategoriesIconLabel,
            CategoriesTextLabel,
            ActiveTab == "Categories");

        SetTabState(
            BasketIconLabel,
            BasketTextLabel,
            ActiveTab == "Basket");

        SetTabState(
            ProfileIconLabel,
            ProfileTextLabel,
            ActiveTab == "Profile");
    }

    private static void SetTabState(
        Label iconLabel,
        Label textLabel,
        bool isActive)
    {
        var colorResource = isActive
            ? "TazoraPrimary"
            : "TazoraTextSecondary";

        iconLabel.SetDynamicResource(
            Label.TextColorProperty,
            colorResource);

        textLabel.SetDynamicResource(
            Label.TextColorProperty,
            colorResource);

        textLabel.FontFamily = isActive
            ? "InterBold"
            : "InterRegular";
    }

    private async void OnHomeTapped(
        object sender,
        TappedEventArgs e)
    {
        if (ActiveTab == "Home")
            return;

        await Shell.Current.GoToAsync("//home");
    }

    private async void OnCategoriesTapped(
        object sender,
        TappedEventArgs e)
    {
        if (ActiveTab == "Categories")
            return;

        await Shell.Current.GoToAsync("//categories");
    }

    private async void OnBasketTapped(
        object sender,
        TappedEventArgs e)
    {
        if (ActiveTab == "Basket")
            return;

        await Shell.Current.GoToAsync("//basket");
    }

    private async void OnProfileTapped(
        object sender,
        TappedEventArgs e)
    {
        if (ActiveTab == "Profile")
            return;

        await Shell.Current.GoToAsync("//profile");
    }
}