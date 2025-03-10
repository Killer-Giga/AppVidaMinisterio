using AppVidaMinisterio.ViewModels;

namespace AppVidaMinisterio
{
    public partial class AppShell : Shell
    {
        public AppShell(MainViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is MainViewModel viewModel)
            {
                viewModel.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == nameof(viewModel.IsLoading))
                        UpdateLoadingState(viewModel.IsLoading);
                };
            }
        }

        private async void UpdateLoadingState(bool isLoading)
        {
            while (BindingContext is MainViewModel viewModel && viewModel.IsLoading) // Se ejecuta mientras IsLoading sea true
            {
                await Task.WhenAll(
                    tesorosImage.FadeTo(0.5, 500),
                    tesorosImage.ScaleTo(1.2, 500),
                    mejoresMaestrosImage.FadeTo(0.5, 500),
                    mejoresMaestrosImage.ScaleTo(1.2, 500),
                    vidaCristianaImage.FadeTo(0.5, 500),
                    vidaCristianaImage.ScaleTo(1.2, 500)
                );

                await Task.WhenAll(
                    tesorosImage.FadeTo(.8, 500),
                    tesorosImage.ScaleTo(.8, 500),
                    mejoresMaestrosImage.FadeTo(.8, 500),
                    mejoresMaestrosImage.ScaleTo(.8, 500),
                    vidaCristianaImage.FadeTo(.8, 500),
                    vidaCristianaImage.ScaleTo(.8, 500)
                );
            }
        }
    }
}