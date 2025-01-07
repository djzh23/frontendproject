using Android.App;
using Android.Content.PM;
using Android.OS;
using ppm_fe.Helpers;
using ppm_fe.Models;

namespace ppm_fe
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        public override void OnBackPressed()
        {
            var appNavigation = new AppNavigation();
            appNavigation.HandleBackButtonAsync().ConfigureAwait(false);
        }
    }
}
