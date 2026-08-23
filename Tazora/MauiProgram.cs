using Microsoft.Extensions.Logging;
using Tazora.Services;

namespace Tazora
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.Services.AddSingleton<DatabaseService>();
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
