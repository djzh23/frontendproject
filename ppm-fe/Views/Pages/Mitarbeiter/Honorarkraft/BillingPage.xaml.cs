using ppm_fe.ViewModels.Pages;
using System.Diagnostics;

namespace ppm_fe.Views.Page;

public partial class BillingPage : ContentPage
{
    private readonly BillingPageViewModel _viewModel;

    private bool _isButtonEnabled = true;
    public bool IsButtonEnabled
    {
        get => _isButtonEnabled;
        set
        {
            _isButtonEnabled = value;
            OnPropertyChanged();
        }
    }

    public BillingPage(BillingPageViewModel viewModel)
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

    private void OnToggleButtonClicked(object sender, EventArgs e)
    {
        IsButtonEnabled = !IsButtonEnabled;
    }
}