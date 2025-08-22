using Microsoft.Extensions.Logging;
using NTTaxi.Libraries.Services;
using NTTaxi.Libraries.Services.Interfaces;
using NTTaxi.Libraries.GoogleSheetServers;
using NTTaxi.Libraries.GoogleSheetServers.Interfaces;

namespace NTTaxi
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            // Registering services
            builder.Services.AddScoped<IAliGgSheetServer, AliGgSheetServer>();
            builder.Services.AddScoped<IAliService, AliService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
