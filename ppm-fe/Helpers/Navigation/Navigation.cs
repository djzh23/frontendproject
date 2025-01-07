using ppm_fe.Constants;
using ppm_fe.Views.HomePages;
using ppm_fe.Views.Page;
using ppm_fe.Views.Pages;

namespace ppm_fe.Helpers
{
    public static class Navigation
    {
        public static async Task NavigationForSuperAdminAsync()
        {
            var flyoutItem = new FlyoutItem()
            {
                Title = "Home Page",
                Route = nameof(SuperAdminHomePage),
                FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems,

                Items =
                            {
                                new ShellContent
                                {
                                    Icon = Icons.Home,
                                    Title = "Startseite",
                                    ContentTemplate = new DataTemplate(typeof(SuperAdminHomePage)),
                                    Style = Application.Current?.Resources != null
                                            ? (Style)Application.Current.Resources["ShellContentStyle"]
                                            : null,
                                },
                                new ShellContent
                                {
                                    Icon = Icons.Users,
                                    Title = "Alle Benutzer",
                                    ContentTemplate = new DataTemplate(typeof(AllUsersPage)),
                                    Style = Application.Current?.Resources != null
                                            ? (Style)Application.Current.Resources["ShellContentStyle"]
                                            : null,
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
                        await Shell.Current.GoToAsync($"//{nameof(SuperAdminHomePage)}");
                    });
                }
                else
                {
                    await Shell.Current.GoToAsync($"//{nameof(SuperAdminHomePage)}");
                }
            }

        }

        public static async Task NavigationForProjektKoordinatorAsync() 
        {
            var flyoutItem = new FlyoutItem()
            {
                Title = "Home Page",
                Route = nameof(AdminHomePage),
                FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems,
                Items =
                    {
                                new ShellContent
                                {
                                    Icon = Icons.Home,
                                    Title = "Startseite",
                                    ContentTemplate = new DataTemplate(typeof(AdminHomePage)),
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
                                    Title = "Meine Einsätze",
                                    ContentTemplate = new DataTemplate(typeof(AllWorksPage)),
                                },
                                new ShellContent
                                {
                                    Icon = Icons.Users,
                                    Title = "Alle Einsätze",
                                    ContentTemplate = new DataTemplate(typeof(AllAdminWorksPage)),
                                },
                                new ShellContent
                                {
                                    Icon = Icons.CreateBilling,
                                    Title = "Alle Rechnungen",
                                    ContentTemplate = new DataTemplate(typeof(AllAdminBillingPage)),
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
                        await Shell.Current.GoToAsync($"//{nameof(AdminHomePage)}");
                    });
                }
                else
                {
                    await Shell.Current.GoToAsync($"//{nameof(AdminHomePage)}");
                }
            }
        }

        public static async Task NavigationForHonorarkraftAsync()
        {
            var flyoutItem = new FlyoutItem()
            {
                Title = "Home Page",
                Route = nameof(HonorarHomePage),
                FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems,
                Items =
                    {
                                new ShellContent
                                {
                                    Icon = Icons.Home,
                                    Title = "Startseite",
                                    ContentTemplate = new DataTemplate(typeof(HonorarHomePage)),
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
                                    Icon = Icons.CreateBilling,
                                    Title = "Meine Rechnungen",
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
                        await Shell.Current.GoToAsync($"//{nameof(HonorarHomePage)}");
                    });
                }
                else
                {
                    await Shell.Current.GoToAsync($"//{nameof(HonorarHomePage)}");
                }
            }
        }

        public static async Task NavigationForMitarbeiterAsync()
        {
            var flyoutItem = new FlyoutItem()
            {
                Title = "Home Page",
                Route = nameof(FestHomePage),
                FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems,
                Items =
                    {
                                new ShellContent
                                {
                                    Icon = Icons.Home,
                                    Title = "Startseite",
                                    ContentTemplate = new DataTemplate(typeof(FestHomePage)),
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
                                    Title = "Meine Einsätze",
                                    ContentTemplate = new DataTemplate(typeof(AllWorksPage)),
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
                        await Shell.Current.GoToAsync($"//{nameof(FestHomePage)}");
                    });
                }
                else
                {
                    await Shell.Current.GoToAsync($"//{nameof(FestHomePage)}");
                }
            }
        }
    }
}
