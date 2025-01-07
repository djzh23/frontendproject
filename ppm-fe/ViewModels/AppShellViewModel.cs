using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ppm_fe.Helpers;
using ppm_fe.Messages;
using ppm_fe.Services;
using ppm_fe.Views.Startup;
using Themes = ppm_fe.Resources.Themes;

namespace ppm_fe.ViewModels
{
    public partial class AppShellViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;

        public AppShellViewModel(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            SignOutCommand = new AsyncRelayCommand(SignOutAsync);
        }

        public ImageSource? Icon { get; set; }
        public DataTemplate? ContentTemplate { get; set; }

        public IAsyncRelayCommand SignOutCommand { get; }

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

            // Clear Cache
            CacheService.ClearCache();
            _isUserCacheInitialized = false;

            Shell.Current.FlyoutIsPresented = false;
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
            Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;

            // Send the message to remove User from cache
            WeakReferenceMessenger.Default.Send(new UserMessage(null));


            WeakReferenceMessenger.Default.Send(new MessageHelper("clear"));

            Application.Current?.Dispatcher.Dispatch(() =>
            {
                Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;
            });
        }
    }
}
