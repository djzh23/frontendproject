using CommunityToolkit.Maui;
using Microcharts.Maui;
using Microsoft.Extensions.Logging;
using ppm_fe.Services;
using ppm_fe.ViewModels;
using ppm_fe.ViewModels.Dashboards;
using ppm_fe.ViewModels.Startup;
using ppm_fe.Views.Dashboards;
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

            //Views
            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<RegisterPage>();
            builder.Services.AddSingleton<LoadingPage>();
            builder.Services.AddSingleton<SuperAdminDashboardPage>();
            

            //View Models
            builder.Services.AddTransient<AppShellViewModel>(); 

            builder.Services.AddSingleton<LoginPageViewModel>();
            builder.Services.AddSingleton<RegisterPageViewModel>();
            builder.Services.AddSingleton<LoadingPageViewModel>();
            builder.Services.AddSingleton<SuperAdminDashboardPageViewModel>();
           
            // Register AppShell
            builder.Services.AddTransient<AppShell>();

            return builder.Build();
        }
    }
}
