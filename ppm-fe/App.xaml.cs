using Microsoft.Maui.Platform;
using ppm_fe.Models;
using ppm_fe.Services.Interfaces;
using ppm_fe.ViewModels;


namespace ppm_fe;
public partial class App : Application
{
    private static User? _userDetails;
    private static readonly object _lock = new();

    public static User? UserDetails
    {
        get
        {
            lock (_lock)
            {
                return _userDetails;
            }
        }
        set
        {
            lock (_lock)
            {
                _userDetails = value;
            }
        }
    }

    public static int? CurrentUserId { get; set; }


    [Obsolete]
    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();

        // Borderless Entry für plattformübergreifende UI
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(nameof(Entry), (handler, view) =>
        {
            if (view is Entry)
            {
#if __ANDROID__
                handler.PlatformView.SetBackgroundColor(Colors.Transparent.ToPlatform());
#elif __IOS__
                //handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
            }
        });

        MainPage = serviceProvider.GetRequiredService<AppShell>();

        // Initialize BaseViewModel with ICacheService
        var cacheService = serviceProvider.GetRequiredService<ICacheService>();
        BaseViewModel.Initialize(cacheService);
    }


    protected override void OnStart()
    {
        // Handle when the app starts
        base.OnStart();
        ThemeManager.Initialize();
    }
}
