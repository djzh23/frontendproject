using ppm_fe.ViewModels.Startup;

namespace ppm_fe.Views.Startup;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterPageViewModel viewModel)
    {
        InitializeComponent();
        this.BindingContext = viewModel;
        Shell.SetNavBarIsVisible(this, false);
    }

    private async void OnBackButtonClick(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
    }
}