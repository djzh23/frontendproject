using ppm_fe.ViewModels.HomePages;

namespace ppm_fe.Views.HomePages;

public partial class FestHomePage : ContentPage
{
    private readonly FestHomePageViewModel _viewModel;

    public FestHomePage(FestHomePageViewModel viewModel)
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