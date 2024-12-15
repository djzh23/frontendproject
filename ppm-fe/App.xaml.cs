namespace ppm_fe;
using Microsoft.Maui.Platform;
using ppm_fe.Models;

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


        // Lade das gespeicherte Theme oder setze "Light" als Standard
        //var theme = Preferences.Get("Theme", "Light");
        var a = UserDetails;
        //LoadTheme(theme);

        // Update the SelectedTheme property after logging back in

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

        // Registrieren für Theme-Änderungsnachrichten
        //WeakReferenceMessenger.Default.Register<ThemeChangedMessage>(this, (r, m) =>
        //{
        //    LoadTheme(m.Value);
        //});
    }


    protected override void OnStart()
    {
        // Handle when your app starts
        base.OnStart();
        ThemeManager.Initialize();
    }
}
