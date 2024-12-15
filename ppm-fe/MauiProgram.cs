using CommunityToolkit.Maui;
using Microcharts.Maui;
using Microsoft.Extensions.Logging;
using ppm_fe.Services;
using ppm_fe.Services.Interfaces;
using ppm_fe.ViewModels;
using ppm_fe.ViewModels.Dashboards;
using ppm_fe.ViewModels.Pages;
using ppm_fe.ViewModels.Startup;
using ppm_fe.Views.Dashboards;
using ppm_fe.Views.Page;
using ppm_fe.Views.Startup;
using SkiaSharp.Views.Maui.Controls.Hosting;
using UraniumUI;

namespace ppm_fe
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMicrocharts()
                .UseSkiaSharp()
                .UseUraniumUI()
                .UseUraniumUIMaterial()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
                            
#if DEBUG

    		builder.Logging.AddDebug();

#endif



            builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<IBillingService, BillingService>();
            builder.Services.AddSingleton<ILocalPathService, LocalPathService>();
            builder.Services.AddSingleton<ICacheService, CacheService>();


            //Views
            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<RegisterPage>();
            builder.Services.AddSingleton<LoadingPage>();
            builder.Services.AddSingleton<SuperAdminDashboardPage>();
            builder.Services.AddSingleton<AllUsersPage>();
            builder.Services.AddSingleton<BillingPage>();
            builder.Services.AddSingleton<ProfilePage>();
            builder.Services.AddSingleton<SettingPage>();


            //View Models
            builder.Services.AddTransient<AppShellViewModel>(); 

            builder.Services.AddSingleton<LoginPageViewModel>();
            builder.Services.AddSingleton<RegisterPageViewModel>();
            builder.Services.AddSingleton<LoadingPageViewModel>();
            builder.Services.AddSingleton<SuperAdminDashboardPageViewModel>();
            builder.Services.AddSingleton<AllUsersPageViewModel>();
            builder.Services.AddSingleton<BillingPageViewModel>();
            builder.Services.AddSingleton<ProfilePageViewModel>();
            builder.Services.AddSingleton<SettingsPageViewModel>();

            // Register AppShell
            builder.Services.AddTransient<AppShell>();

            return builder.Build();
        }
    }
}
