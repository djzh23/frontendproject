using ppm_fe.Constants;
using ppm_fe.Controls;
using ppm_fe.Models;
using ppm_fe.Views.HomePages;
using ppm_fe.Views.Page;
using ppm_fe.Views.Pages;

namespace ppm_fe.Helpers
{
    public partial class AppNavigation
    {
        public async static Task AddFlyoutMenusDetails()
        {

            if (Shell.Current == null)
            {
                return;
            }

            Shell.Current.FlyoutHeader = new FlyoutHeaderControl();


            var superAdminDashboardInfo = Shell.Current.Items.Where(f => f.Route == nameof(SuperAdminHomePage)).FirstOrDefault();
            if (superAdminDashboardInfo != null) Shell.Current.Items.Remove(superAdminDashboardInfo);

            var AdminHomePafeInfo = Shell.Current.Items.Where(f => f.Route == nameof(AdminHomePage)).FirstOrDefault();
            if (AdminHomePafeInfo != null) Shell.Current.Items.Remove(AdminHomePafeInfo);

            var HonorarHomePageInfo = Shell.Current.Items.Where(f => f.Route == nameof(HonorarHomePage)).FirstOrDefault();
            if (HonorarHomePageInfo != null) Shell.Current.Items.Remove(HonorarHomePageInfo);

            var FestHomePageInfo = Shell.Current.Items.Where(f => f.Route == nameof(FestHomePage)).FirstOrDefault();
            if (FestHomePageInfo != null) Shell.Current.Items.Remove(FestHomePageInfo);



            // Role-based displaying of the Navigations in Flyout 

            if(App.UserDetails?.Role_ID == (int)UserRole.SuperAdmin)
            {
                await Navigation.NavigationForSuperAdminAsync();
            }

            if (App.UserDetails?.Role_ID == (int)UserRole.Admin)
            {
                await Navigation.NavigationForProjektKoordinatorAsync();
            }

            if (App.UserDetails?.Role_ID == (int)UserRole.Honorarkraft)
            {
                await Navigation.NavigationForHonorarkraftAsync();
            }

            if (App.UserDetails?.Role_ID == (int)UserRole.FestMitarbeiter)
            {
                await Navigation.NavigationForMitarbeiterAsync();
            }
        }

        public async Task HandleBackButtonAsync()
        {
            if (Application.Current?.MainPage != null)
            {
                bool confirmExit = await Application.Current.MainPage.DisplayAlert(
                    "App schließen",
                    "Möchten Sie die Anwendung wirklich schließen?",
                    "Ok",
                    "Abbrechen");

                if (confirmExit)
                {
                    CloseApp();
                }
            }
        }

        public void CloseApp()
        {
#if ANDROID
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
#elif WINDOWS
            System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
#else
            System.Diagnostics.Debug.WriteLine("App-Schließen wird nicht unterstützt.");
#endif
        }
    }
}