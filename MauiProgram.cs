using AppVidaMinisterio.ViewModels;
using AppVidaMinisterio.Views;
using Microsoft.Extensions.Logging;
using QuestPDF.Infrastructure;

namespace AppVidaMinisterio
{
    public static class MauiProgram
    {
        public static MauiApp? MauiAppInstance { get; private set; }

        public static MauiApp CreateMauiApp()
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Inyección de dependencias
            builder.Services.AddSingleton<MainViewModel>();

            // Registra las páginas
            builder.Services.AddTransient<PrincipalView>();
            builder.Services.AddSingleton<AppShell>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            MauiAppInstance = builder.Build();
            return MauiAppInstance;
        }
    }
}
