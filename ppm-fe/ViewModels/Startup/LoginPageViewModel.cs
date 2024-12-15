using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ppm_fe.Helpers;
using ppm_fe.Models;
using ppm_fe.Services;
using ppm_fe.Views.Startup;
using Newtonsoft.Json;

namespace ppm_fe.ViewModels
{
    public partial class LoginPageViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        [ObservableProperty]
        private string? _email;

        [ObservableProperty]
        private string? _password;

        #region Commands

        public IAsyncRelayCommand LoginCommand { get; }
        public IAsyncRelayCommand ForgotPasswordCommand { get; }

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

                IsBusy = true;
                IsRefreshing = true;

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
                IsRefreshing = false;
                IsBusy = false;
            }
        }

        private async Task ForgotPassword()
        {
            if (Email is null)
            {
                await DisplayAlertAsync("Fehler", "Bitte E-Mail in das Feld eingeben");
                return;
            }

            IsRefreshing = true;
            try
            {
                if (!string.IsNullOrWhiteSpace(Email))
                {
                    bool forgetPasswordSuccess = await _authService.ForgetPassword(Email);
                    if (forgetPasswordSuccess)
                    {
                        IsRefreshing = false;
                        var window = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0] : null;
                        if (window?.Page != null)
                        {
                            await DisplayAlertAsync("Konfirmation", "Link zum Zurücksetzen des Passworts erfolgreich an die E-Mail gesendet!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(ForgotPassword), ex.Message, new { Email }, ex.StackTrace);
            }
            finally
            {
                IsRefreshing = false;
                IsBusy = false;
            }
        }

        #endregion
    }
}