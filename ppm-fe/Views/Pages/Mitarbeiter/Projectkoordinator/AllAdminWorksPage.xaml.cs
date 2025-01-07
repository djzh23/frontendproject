using ppm_fe.ViewModels.Pages;

namespace ppm_fe.Views.Page;

public partial class AllAdminWorksPage : ContentPage
{
    private AllAdminWorksPageViewModel _viewModel;

    public AllAdminWorksPage(AllAdminWorksPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel?.OnDisappearing();
        (_viewModel as IDisposable)?.Dispose();
    }
}