using ppm_fe.Extensions;
using ppm_fe.ViewModels;

namespace ppm_fe.Views.Startup;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginPageViewModel viewModel)
    {
        InitializeComponent();
        this.BindingContext = viewModel;
        Shell.SetNavBarIsVisible(this, false);
    }


    private async void OnRegisterButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(RegisterPage)}");
    }

    private async void OnForgotPasswordTapped(object sender, EventArgs e)
    {
        // Change color
        await AnimateColorChange(Colors.Red); // Or any color you prefer

        // Execute your command
        if (BindingContext is LoginPageViewModel viewModel)
        {
            viewModel.ForgotPasswordCommand.Execute(null);
        }

        // Change color back
        await AnimateColorChange(Colors.Cyan);
    }

    private async Task AnimateColorChange(Color targetColor)
    {
        await ForgotPasswordLabel.ColorTo(ForgotPasswordLabel.TextColor, targetColor, c => ForgotPasswordLabel.TextColor = c, 250);
    }
}