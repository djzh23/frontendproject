using ppm_fe.Controls;
using ppm_fe.Views.Dashboards;

namespace ppm_fe.Models
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


            var superAdminDashboardInfo = Shell.Current.Items.Where(f => f.Route == nameof(SuperAdminDashboardPage)).FirstOrDefault();
            if (superAdminDashboardInfo != null) Shell.Current.Items.Remove(superAdminDashboardInfo);


            if (App.UserDetails?.Role_ID == (int)UserRole.SuperAdmin)
            {
                var flyoutItem = new FlyoutItem()
                {
                    Title = "Home Page",
                    Route = nameof(SuperAdminDashboardPage),
                    FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems,

                    Items =
                            {
                                new ShellContent
                                {
                                    Icon = Icons.Home,
                                    Title = "Startseite",
                                    ContentTemplate = new DataTemplate(typeof(SuperAdminDashboardPage)),
                                    Style = Application.Current?.Resources != null
                                            ? (Style)Application.Current.Resources["ShellContentStyle"]
                                            : null,
                                }
                            }
                };
                if (!Shell.Current.Items.Contains(flyoutItem))
                {
                    Shell.Current.Items.Add(flyoutItem);
                    if (DeviceInfo.Platform == DevicePlatform.WinUI)
                    {
                        Shell.Current.Dispatcher.Dispatch(async () =>
                        {
                            await Shell.Current.GoToAsync($"//{nameof(SuperAdminDashboardPage)}");
                        });
                    }
                    else
                    {
                        await Shell.Current.GoToAsync($"//{nameof(SuperAdminDashboardPage)}");
                    }
                }

            }

        }
    }
}