using ppm_fe.Services;
using ppm_fe.ViewModels.Dashboards;
using Microsoft.Maui.Platform;

namespace ppm_fe.Views.Dashboards;

public partial class AdminDashboardPage : ContentPage
{
    private readonly AdminDashboardPageViewModel _viewModel;

    public AdminDashboardPage(AdminDashboardPageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.RefreshDataAsync();
        BackButton.Clicked += OnBackButtonClicked;
        MyWebView.Navigated += OnWebViewNavigated;
    }

    private void OnWebViewNavigated(object sender, WebNavigatedEventArgs e)
    {
        _viewModel.CanGoBack = MyWebView.CanGoBack;
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        if (MyWebView.CanGoBack)
        {
            MyWebView.GoBack();
        }
    }
    private async void OnRefreshButtonClicked(object sender, EventArgs e)
    {
       await _viewModel.RefreshDataAsync();
    }
}