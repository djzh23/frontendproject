using ppm_fe.ViewModels.Pages;

namespace ppm_fe.Views.Pages;

public partial class AllAdminBillingPage : ContentPage
{
    private readonly AllAdminBillingPageViewModel _viewModel;



    public AllAdminBillingPage(AllAdminBillingPageViewModel viewModel)
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