using Microsoft.Extensions.Logging;
using Tazora.Pages;
using Tazora.Services;

namespace Tazora
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<AppSession>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<CategoriesPage>();
            builder.Services.AddTransient<BasketPage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<ProductListPage>();
            builder
                .UseMauiApp<App>()
                    .ConfigureFonts(fonts =>
                         {
                             fonts.AddFont(
                                 "Inter_18pt-Regular.ttf",
                                 "InterRegular");

                             fonts.AddFont(
                                 "Inter_18pt-Bold.ttf",
                                 "InterBold");

                             fonts.AddFont(
                                 "MaterialSymbolsOutlined-Regular.ttf",
                                 "MaterialSymbols");
                         });
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
