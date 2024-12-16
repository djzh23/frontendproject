using ppm_fe.ViewModels.Pages;
using System.Diagnostics;

namespace ppm_fe.Views.Page;

public partial class AllUsersWorksPage : ContentPage
{
    private AllUsersWorksPageViewModel _viewModel;

    public AllUsersWorksPage(AllUsersWorksPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        //await _viewModel.GetLastAllWorks();
        await _viewModel.InitializeAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel?.OnDisappearing();
        Debug.WriteLine("----------------------------> AllUsersWorksPage OnDisappearing called");
        (_viewModel as IDisposable)?.Dispose();
    }
}