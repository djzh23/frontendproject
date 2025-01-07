using ppm_fe.ViewModels.HomePages;

namespace ppm_fe.Views.HomePages;

public partial class AdminHomePage : ContentPage
{
    private readonly AdminHomePageViewModel _viewModel;

    public AdminHomePage(AdminHomePageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.RefreshDataAsync();
    }
}