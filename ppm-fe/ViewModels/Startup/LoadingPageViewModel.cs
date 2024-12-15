using ppm_fe.Models;
using ppm_fe.Views.Startup;
using Newtonsoft.Json;

namespace ppm_fe.ViewModels.Startup
{
    public class LoadingPageViewModel : BaseViewModel
    {
        public LoadingPageViewModel()
        {
            CheckUserLoginDetails();
        }
        private async void CheckUserLoginDetails()
        {
            // Retrieve the stored user details as a string
            string userDetailsStr = Preferences.Get(nameof(App.UserDetails), "");
            string theme_ = Preferences.Get("Theme", "");

            // Check if the user details string is null or empty
            if (string.IsNullOrWhiteSpace(userDetailsStr))
            {
                // Navigate to the login page if user details are not found
                Shell.Current.Dispatcher.Dispatch(async () =>
                {
                    await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
                });
                return;
            }

            try
            {
                // Deserialize the user details string to a UserBasicInfo object
                var userInfo = JsonConvert.DeserializeObject<User>(userDetailsStr);

                // Check if the deserialization was successful and the object is not null
                if (userInfo == null || userInfo.Role_ID == null || userInfo.Approved == null)
                {
                    Shell.Current.Dispatcher.Dispatch(async () =>
                    {
                        await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
                    });
                    return;
                }

                // Assign the deserialized user info to the App.UserDetails property
                else
                {
                    // Store the theme in Preferences
                    Preferences.Set("Theme", theme_);
                    App.UserDetails = userInfo;
                }
                App.UserDetails = userInfo;

                // Add flyout menu details
                await AppNavigation.AddFlyoutMenusDetails();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");

                var window = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0] : null;
                if (window?.Page != null)
                {
                    await window.Page.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                }
                else
                {
                    // Handle the case where there's no current window or page
                    Console.WriteLine("No current window or page found. Cannot display alert.");
                }
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
                }
            }
        }
    }
}
