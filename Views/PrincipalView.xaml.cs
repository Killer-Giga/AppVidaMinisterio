using AppVidaMinisterio.ViewModels;
using AppVidaMinisterio.Messages;
using CommunityToolkit.Mvvm.Messaging;

namespace AppVidaMinisterio.Views;

public partial class PrincipalView : ContentPage
{
    public PrincipalView(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        InitializeAsync(viewModel);
    }

    private async void InitializeAsync(MainViewModel viewModel)
    {
        await viewModel.InitializeAsync();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        WeakReferenceMessenger.Default.Register<AnimationMessage>(this, async (r, m) =>
        {
            if (m.AnimationType == "ButtonNext")
            {
                await AnimateButton(ButtonNext);
                await AnimateGrid();
                
            }
            else if (m.AnimationType == "ButtonPrevious")
            {
                await AnimateButton(ButtonPrevious);
                await AnimateGrid();
            }
        });
    }

    protected override void OnDisappearing()
    {
        WeakReferenceMessenger.Default.Unregister<AnimationMessage>(this);
        base.OnDisappearing();
    }

    private async Task AnimateButton(VisualElement element)
    {
        await element.ScaleTo(0.9, 100);
        await element.ScaleTo(1, 100);
    }

    private async Task AnimateGrid()
    {
        var fadeOutTasks = MyGrid.Children
        .Select(child =>
        {
            if (child != borderSkip && child is VisualElement ve)
                return ve.FadeTo(0, 200);
            else if (child == borderSkip)
                return labelSemana.FadeTo(0, 200);
            else
                return Task.CompletedTask;
        })
        .ToList();

        await Task.WhenAll(fadeOutTasks);

        if (BindingContext is MainViewModel vm)
        {
            vm.UpdateWeekAfterAnimation(); // Método que actualiza SemanaActual
        }

        var fadeInTasks = MyGrid.Children
        .Select(child =>
        {
            if (child != borderSkip && child is VisualElement ve)
                return ve.FadeTo(1, 200);
            else if (child == borderSkip)
                return labelSemana.FadeTo(1, 200);
            else
                return Task.CompletedTask;
        })
        .ToList();

        await Task.WhenAll(fadeInTasks);
    }
}