namespace AppVidaMinisterio
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            if (MauiProgram.MauiAppInstance?.Services is not IServiceProvider serviceProvider)
                throw new InvalidOperationException("Servicios no inicializados");

            // Si usara new AppShell() directamente, no se inyectarían
            // automáticamente sus dependencias (como MainViewModel), lo que rompería el flujo de DI.
            var appShell = serviceProvider.GetRequiredService<AppShell>();

            return new Window(appShell);
        }
    }
}