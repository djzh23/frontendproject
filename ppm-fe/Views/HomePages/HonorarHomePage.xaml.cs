using ppm_fe.ViewModels.HomePages;

namespace ppm_fe.Views.HomePages;

public partial class HonorarHomePage : ContentPage
{
    private readonly HonorarHomePageViewModel _viewModel;

    public HonorarHomePage(HonorarHomePageViewModel viewModel)
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