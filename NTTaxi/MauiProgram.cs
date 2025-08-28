using Microsoft.Extensions.Logging;
using NTTaxi.Libraries.GoogleSheetServers;
using NTTaxi.Libraries.GoogleSheetServers.Interfaces;
using NTTaxi.Libraries.Services;
using NTTaxi.Libraries.Services.Interfaces;
using NTTaxi.Libraries.Workers;
using NTTaxi.Libraries.Workers.Interfaces;

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

            // Registering services (Dispose sau khi call)
            // Post data to Google Sheet
            builder.Services.AddScoped<IAliGgSheetServer, AliGgSheetServer>();
            builder.Services.AddScoped<IVetcGgSheetServer, VetcGgSheetServer>();
            // Get data from HTTP API
            builder.Services.AddScoped<IAliService, AliService>();
            builder.Services.AddScoped<IVETCService, VETCService>();

            // Testing Skysoft
            builder.Services.AddScoped<ISkysoftService, SkysoftService> ();

            // Registering workers (Singleton luôn sống theo ứng dụng)
            builder.Services.AddSingleton<IAliWorker, AliWorker>();
            builder.Services.AddSingleton<IVetcWorker, VetcWorker>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
