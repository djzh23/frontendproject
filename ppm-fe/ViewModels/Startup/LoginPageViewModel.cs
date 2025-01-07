using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using ppm_fe.Helpers;
using ppm_fe.Models;
using ppm_fe.Services;
using ppm_fe.Views.Startup;

namespace ppm_fe.ViewModels
{
    public partial class LoginPageViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;

        public LoginPageViewModel(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            LoginCommand = new AsyncRelayCommand(Login);
            ForgotPasswordCommand = new AsyncRelayCommand(ForgotPassword);

            // Subscribe to the message to clear fields field when user is logged out
            WeakReferenceMessenger.Default.Register<MessageHelper>(this, (recipient, message) =>
            {
                if (message.Value == "clear")
                {
                    Email = string.Empty; // Clear the field value
                    Password = string.Empty; // Clear the field value
                }
            });
        }

        #region properties
        [ObservableProperty]
        private string? _email;

        [ObservableProperty]
        private string? _password;
        #endregion

        #region Commands
        public IAsyncRelayCommand LoginCommand { get; }
        public IAsyncRelayCommand ForgotPasswordCommand { get; }
        #endregion

        #region tasks
        private async Task Login()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await DisplayAlertAsync("Fehler", "Bitte geben Sie Email und Passwort ein");
                return;
            }

            try
            {
                // show Loading Page
                await Shell.Current.GoToAsync($"//{nameof(LoadingPage)}");

                IsLoading = true;

                var loginResult = await _authService.Login(Email, Password);
                if (loginResult.Success)
                {
                    // Login succeeded, now retrieve user details
                    User userDetails_ = _authService.GetLoggedIntUser();

                    // Remove existing user details and save new details
                    if (Preferences.ContainsKey(nameof(App.UserDetails)))
                    {
                        Preferences.Remove(nameof(App.UserDetails));
                    }

                    string userDetailStr = JsonConvert.SerializeObject(userDetails_);
                    Preferences.Set(nameof(App.UserDetails), userDetailStr);
                    App.UserDetails = userDetails_;

                    await AppNavigation.AddFlyoutMenusDetails();
                }
                else
                {
                    // Hide the loading page
                    await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
                    await DisplayAlertAsync("Fehler", $"{loginResult.Message}");
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(Login), ex.Message, new { Email }, ex.StackTrace);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ForgotPassword()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                await DisplayAlertAsync("Fehler", "Bitte E-Mail in das Feld eingeben");
                return;
            }

            IsLoading = true;
            try
            {
                var response = await _authService.ForgetPassword(Email);
                if (response != null)
                {
                    IsLoading = false;
                    var window = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0] : null;
                    if (window?.Page != null)
                    {
                        string alertTitle = response.Success ? "Erfolg" : "Fehler";
                        await DisplayAlertAsync(alertTitle, response.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(ForgotPassword), ex.Message, new { Email }, ex.StackTrace);
                await DisplayAlertAsync("Fehler", Constants.Properties.UnexpectedError);
            }
            finally
            {
                IsLoading = false;
            }
        }
        #endregion
    }
}