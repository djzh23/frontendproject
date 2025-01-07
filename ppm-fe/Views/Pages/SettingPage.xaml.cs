using CommunityToolkit.Maui.Views;
using ppm_fe.ViewModels.Pages;
using System.Runtime.Versioning;

namespace ppm_fe.Views.Page
{
    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("ios")]
    [SupportedOSPlatform("android")]
    public partial class SettingPage : ContentPage
    {

        public SettingPage(SettingsPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            ThemeManager.SelectedThemeChanged += ThemeManager_SelectedThemeChanged;
        }

        private void ThemeManager_SelectedThemeChanged(object? sender, EventArgs e)
        {
            SelectedTheme = ThemeManager.SelectedTheme;
        }

        private string _selectedTheme = ThemeManager.SelectedTheme;
        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (_selectedTheme != value)
                {

                    _selectedTheme = value;
                    OnPropertyChanged();
                }
            }
        }

        private void OnChangeThemeClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string themeName)
            {
                if (ThemeManager.ThemeNames.Contains(themeName))
                {
                    ThemeManager.SetTheme(themeName);
                }
            }
        }

    }
}