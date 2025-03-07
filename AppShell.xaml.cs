using AppVidaMinisterio.ViewModels;
using AppVidaMinisterio.Views;

namespace AppVidaMinisterio
{
    public partial class AppShell : Shell
    {
        public AppShell(MainViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}