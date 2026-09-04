using Tazora.Models;
using Tazora.Services;

namespace Tazora.Controls;

public partial class AiChatWidget : ContentView
{
    private readonly AiService _aiService;
    private readonly DatabaseService _databaseService;

    public AiChatWidget()
    {
        InitializeComponent();

        var services = IPlatformApplication.Current!.Services;
        _aiService = services.GetRequiredService<AiService>();
        _databaseService = services.GetRequiredService<DatabaseService>();
    }

    private void OnOpenChatTapped(object sender, TappedEventArgs e)
    {
        ChatOverlay.IsVisible = true;
        ChatPopup.IsVisible = true;
    }

    private void OnCloseChatTapped(object sender, TappedEventArgs e)
    {
        ChatOverlay.IsVisible = false;
        ChatPopup.IsVisible = false;
    }

    private async void OnSendPromptClicked(object sender, EventArgs e)
    {
        var prompt = TxtUserPrompt.Text?.Trim();
        if (string.IsNullOrEmpty(prompt))
            return;

        AddUserMessage(prompt);
        TxtUserPrompt.Text = string.Empty;

        var typingBorder = AddAssistantMessage("Düşünüyorum...");

        var result = await _aiService.GetRecipeAndIngredientsAsync(prompt);

        ChatMessagesContainer.Remove(typingBorder);
        AddAssistantMessage(result.AssistantMessage);

        if (result.MatchedProducts.Count > 0)
            AddProductCardsToChat(result.MatchedProducts);

        await ScrollToBottomAsync();
    }

    private void AddUserMessage(string message)
    {
        var border = new Border
        {
            BackgroundColor = (Color)Application.Current!.Resources["TazoraPrimary"],
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
            Stroke = Colors.Transparent,
            Padding = new Thickness(14),
            HorizontalOptions = LayoutOptions.End,
            MaximumWidthRequest = 280,
            Content = new Label
            {
                Text = message,
                FontFamily = "InterRegular",
                FontSize = 13,
                TextColor = Colors.White
            }
        };

        ChatMessagesContainer.Add(border);
    }

    private Border AddAssistantMessage(string message)
    {
        var border = new Border
        {
            BackgroundColor = (Color)Application.Current!.Resources["TazoraPrimarySoft"],
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
            Stroke = Colors.Transparent,
            Padding = new Thickness(14),
            HorizontalOptions = LayoutOptions.Start,
            MaximumWidthRequest = 280,
            Content = new Label
            {
                Text = message,
                FontFamily = "InterRegular",
                FontSize = 13,
                TextColor = (Color)Application.Current!.Resources["TazoraTextPrimary"]
            }
        };

        ChatMessagesContainer.Add(border);
        return border;
    }

    private void AddProductCardsToChat(List<Product> products)
    {
        var layout = new VerticalStackLayout { Spacing = 8, HorizontalOptions = LayoutOptions.Start };

        foreach (var product in products)
        {
            var card = new Border
            {
                BackgroundColor = (Color)Application.Current!.Resources["TazoraSurface"],
                Stroke = (Color)Application.Current!.Resources["TazoraDivider"],
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                Padding = new Thickness(10),
                WidthRequest = 260
            };

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                },
                ColumnSpacing = 8
            };

            var img = new Image
            {
                Source = product.ImageName,
                WidthRequest = 40,
                HeightRequest = 40,
                Aspect = Aspect.AspectFill
            };

            var textLayout = new VerticalStackLayout { VerticalOptions = LayoutOptions.Center };
            textLayout.Add(new Label
            {
                Text = product.Name,
                FontFamily = "InterBold",
                FontSize = 12,
                TextColor = (Color)Application.Current!.Resources["TazoraTextPrimary"]
            });
            textLayout.Add(new Label
            {
                Text = $"{product.Price:N2} TL",
                FontFamily = "InterRegular",
                FontSize = 11,
                TextColor = (Color)Application.Current!.Resources["TazoraPrimary"]
            });

            var btnAdd = new Button
            {
                Text = "+ Ekle",
                BackgroundColor = (Color)Application.Current!.Resources["TazoraPrimary"],
                TextColor = Colors.White,
                FontSize = 11,
                CornerRadius = 8,
                HeightRequest = 32,
                Padding = new Thickness(10, 0)
            };

            btnAdd.Clicked += async (_, _) =>
            {
                await _databaseService.AddProductToBasketAsync(product.Id);
                btnAdd.Text = "Eklendi";
                btnAdd.IsEnabled = false;
            };

            grid.Add(img, 0, 0);
            grid.Add(textLayout, 1, 0);
            grid.Add(btnAdd, 2, 0);

            card.Content = grid;
            layout.Add(card);
        }

        ChatMessagesContainer.Add(layout);
    }

    private async Task ScrollToBottomAsync()
    {
        await Task.Delay(50);
        await ChatScrollView.ScrollToAsync(0, ChatMessagesContainer.Height, animated: true);
    }
}