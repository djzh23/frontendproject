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
        int count = 0;

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

        private async void OnExpanderPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Expander.IsExpanded) && sender is Expander expander)
            {
                var arrow = FindArrowImage(expander);
                if (arrow != null)
                {
                    await AnimateArrow(expander, arrow);
                }
            }
        }

        public Image? FindArrowImage(Expander expander)
        {
            return (expander.Header as Grid)?.Children.OfType<Image>().FirstOrDefault();
        }

        private async Task AnimateArrow(Expander expander, Image arrowImage)
        {
            uint duration = 300; // Slightly longer duration for smoother effect
            double targetRotation = expander.IsExpanded ? 90 : 0;

            // Use ViewExtensions for more control over the animation
            await arrowImage.RotateTo(targetRotation, duration, Easing.SpringOut);

            // Optional: Add a subtle scale animation for extra smoothness
            await Task.WhenAll(
                arrowImage.RotateTo(targetRotation, duration, Easing.SpringOut),
                arrowImage.ScaleTo(1.1, duration / 2, Easing.SpringOut),
                arrowImage.ScaleTo(1, duration / 2, Easing.SpringIn)
            );
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;
        }
    }
}