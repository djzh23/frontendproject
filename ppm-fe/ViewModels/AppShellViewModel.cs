using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ppm_fe.Helpers;
using ppm_fe.Services;
using ppm_fe.Services.Interfaces;
using ppm_fe.Views.Startup;
using Themes = ppm_fe.Resources.Themes;

namespace ppm_fe.ViewModels
{
    public partial class AppShellViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly ICacheService _cacheService;

        public ImageSource? Icon { get; set; }
        public DataTemplate? ContentTemplate { get; set; }
        public IAsyncRelayCommand SignOutCommand { get; }

        public AppShellViewModel(IAuthService authService, ICacheService cacheService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            SignOutCommand = new AsyncRelayCommand(SignOutAsync);
            // for testing - logout manualy
            //SignOutCommand.Execute(null);
        }

        private async Task SignOutAsync()
        {
            if (Preferences.ContainsKey(nameof(App.UserDetails)))
            {
                await _authService.Logout();

                Preferences.Remove(nameof(App.UserDetails));
                Preferences.Remove("theme");

                App.UserDetails = null;
                Preferences.Clear();

                ThemeManager.SetTheme(nameof(Themes.Default));
            }

            _cacheService.ClearCache();

            Shell.Current.FlyoutIsPresented = false;
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
            Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;

            // Send the message to clear fields
            WeakReferenceMessenger.Default.Send(new MessageHelper("clear"));

            // If needed, re-enable the Flyout after navigation
            Device.BeginInvokeOnMainThread(() =>
            {
                Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;
            });
        }
    }
}
