using ppm_fe.ViewModels.Pages;

namespace ppm_fe.Views.Page;

public partial class ProfilePage : ContentPage
{
    public ProfilePage(ProfilePageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ((ProfilePageViewModel)BindingContext).RefreshProfileCommand.Execute(null);
    }
}