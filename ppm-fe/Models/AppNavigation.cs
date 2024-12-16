using ppm_fe.Controls;
using ppm_fe.Views.Dashboards;
using ppm_fe.Views.Page;

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

            var AdminDashboardInfo = Shell.Current.Items.Where(f => f.Route == nameof(AdminDashboardPage)).FirstOrDefault();
            if (AdminDashboardInfo != null) Shell.Current.Items.Remove(AdminDashboardInfo);


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
                                },
                                new ShellContent
                                {
                                    Icon = Icons.Users,
                                    Title = "All Users Page",
                                    ContentTemplate = new DataTemplate(typeof(AllUsersPage)),
                                    Style = Application.Current?.Resources != null
                                            ? (Style)Application.Current.Resources["ShellContentStyle"]
                                            : null,
                                },
                                new ShellContent
                                {
                                    Icon = Icons.CreateBilling,
                                    Title = "Rechnungen",
                                    ContentTemplate = new DataTemplate(typeof(BillingPage)),
                                },
                                new ShellContent
                                {
                                    Icon = Icons.ProfilRed,
                                    Title = "Profile",
                                    ContentTemplate = new DataTemplate(typeof(ProfilePage)),
                                },
                                new ShellContent
                                {
                                    Icon = Icons.AboutUs,
                                    Title = "Einstellung",
                                    ContentTemplate = new DataTemplate(typeof(SettingPage)),
                                    Style = Application.Current ?.Resources != null ?(Style) Application.Current.Resources["ShellContentStyle"] : null,
                                },
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

            if (App.UserDetails?.Role_ID == (int)UserRole.Admin)
            {
                var flyoutItem = new FlyoutItem()
                {
                    Title = "Home Page",
                    Route = nameof(AdminDashboardPage),
                    FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems,
                    Items =
                    {
                                new ShellContent
                                {
                                    Icon = Icons.Home,
                                    Title = "Startseite",
                                    ContentTemplate = new DataTemplate(typeof(AdminDashboardPage)),
                                },
                                new ShellContent
                                {
                                    Icon = Icons.CreateWork,
                                    Title = "Einsatzserstellung",
                                    ContentTemplate = new DataTemplate(typeof(CreateWorkPage)),
                                },
                                new ShellContent
                                {
                                    Icon = Icons.AllWorks,
                                    Title = "Alle Meine Einsätze",
                                    ContentTemplate = new DataTemplate(typeof(AllWorksPage)),
                                },
                                new ShellContent
                                {
                                    Icon = Icons.Users,
                                    Title = "Alle Einsätze",
                                    ContentTemplate = new DataTemplate(typeof(AllUsersWorksPage)),
                                },
                                new ShellContent
                                {
                                    Icon = Icons.CreateBilling,
                                    Title = "Rechnungen",
                                    ContentTemplate = new DataTemplate(typeof(BillingPage)),
                                },
                                new ShellContent
                                {
                                    Icon = Icons.Statistic,
                                    Title = "Statistiken",
                                    ContentTemplate = new DataTemplate(typeof(StatisticsPage)),
                                },
                                new ShellContent
                                {
                                    Icon = Icons.ProfilRed,
                                    Title = "Profile",
                                    ContentTemplate = new DataTemplate(typeof(ProfilePage)),
                                },
                                new ShellContent
                                {
                                    Icon = Icons.AboutUs,
                                    Title = "Einstellung",
                                    ContentTemplate = new DataTemplate(typeof(SettingPage)),
                                    Style = Application.Current ?.Resources != null ?(Style) Application.Current.Resources["ShellContentStyle"] : null,
                                },
                   }
                };

                if (!Shell.Current.Items.Contains(flyoutItem))
                {
                    Shell.Current.Items.Add(flyoutItem);
                    if (DeviceInfo.Platform == DevicePlatform.WinUI)
                    {
                        Shell.Current.Dispatcher.Dispatch(async () =>
                        {
                            await Shell.Current.GoToAsync($"//{nameof(AdminDashboardPage)}");
                        });
                    }
                    else
                    {
                        await Shell.Current.GoToAsync($"//{nameof(AdminDashboardPage)}");
                    }
                }
            }



        }

        public async Task HandleBackButtonAsync()
        {
            bool confirmExit = await Application.Current.MainPage.DisplayAlert(
                "App schließen",
                "Möchten Sie die Anwendung wirklich schließen?",
                "Ok",
                "Abbrechen");

            if (confirmExit)
            {
                //System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
                CloseApp();
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