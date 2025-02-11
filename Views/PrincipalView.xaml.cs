using AppVidaMinisterio.ViewModels;

namespace AppVidaMinisterio.Views;

public partial class PrincipalView : ContentPage
{
	public PrincipalView()
	{
        _ = InitializeAsync();
    }

	private async Task InitializeAsync()
	{
        var viewModel = new MainViewModel();
        await viewModel.InitializeAsync();
        BindingContext = viewModel;

        InitializeComponent();
    }
}