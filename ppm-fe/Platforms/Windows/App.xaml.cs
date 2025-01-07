using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ppm_fe.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();


        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);

            var navigationViewStyle = new Microsoft.UI.Xaml.Style(typeof(NavigationView));

            // NavigationView-Settings
            navigationViewStyle.Setters.Add(new Microsoft.UI.Xaml.Setter(NavigationView.PaneDisplayModeProperty, NavigationViewPaneDisplayMode.Left));
            navigationViewStyle.Setters.Add(new Microsoft.UI.Xaml.Setter(NavigationView.IsPaneOpenProperty, true));
            navigationViewStyle.Setters.Add(new Microsoft.UI.Xaml.Setter(NavigationView.OpenPaneLengthProperty, 250.0));
            navigationViewStyle.Setters.Add(new Microsoft.UI.Xaml.Setter(NavigationView.CompactPaneLengthProperty, 0.0));
            navigationViewStyle.Setters.Add(new Microsoft.UI.Xaml.Setter(NavigationView.IsPaneToggleButtonVisibleProperty, false));

            // SplitView Style
            var splitViewStyle = new Microsoft.UI.Xaml.Style(typeof(SplitView));
            splitViewStyle.Setters.Add(new Microsoft.UI.Xaml.Setter(SplitView.DisplayModeProperty, SplitViewDisplayMode.CompactInline));
            splitViewStyle.Setters.Add(new Microsoft.UI.Xaml.Setter(SplitView.CompactPaneLengthProperty, 0.0));
            splitViewStyle.Setters.Add(new Microsoft.UI.Xaml.Setter(SplitView.OpenPaneLengthProperty, 250.0));

            Microsoft.UI.Xaml.Application.Current.Resources.MergedDictionaries.Add(
                new Microsoft.UI.Xaml.ResourceDictionary
                {
                    { typeof(NavigationView), navigationViewStyle },
                    { typeof(SplitView), splitViewStyle }
                }
            );
        }
    }

}
