using System.Reflection;
using System.Runtime.Versioning;


using Themes = ppm_fe.Resources.Themes;

namespace ppm_fe
{
    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("ios")]
    [SupportedOSPlatform("android")]
    public static class ThemeManager
    {
        private const string ThemeKey = "Theme";
        private const string PreviousThemeKey ="PerviousTheme";
        private static readonly Dictionary<string, ResourceDictionary> _themesMap = new();

        private static string[]? _themeNames;
        public static string[] ThemeNames => _themeNames ??= _themesMap.Keys.ToArray();
        public static string SelectedTheme { get; private set; } = nameof(Themes.Default);

        public static event EventHandler? SelectedThemeChanged;


        static ThemeManager()
        {
            // Load all theme dictionaries
            _themesMap[nameof(Themes.Default)] = LoadResourceDictionary("ppm_fe.Resources.Themes.Default.xaml");
            _themesMap[nameof(Themes.Dark)] = LoadResourceDictionary("ppm_fe.Resources.Themes.Dark.xaml");
            _themesMap[nameof(Themes.Blue)] = LoadResourceDictionary("ppm_fe.Resources.Themes.Blue.xaml");
            _themesMap[nameof(Themes.Red)] = LoadResourceDictionary("ppm_fe.Resources.Themes.Red.xaml");

            Application.Current!.RequestedThemeChanged += Current_RequestedThemeChanged;
        }

        public static void Initialize()
        {

            string? selectedTheme = Preferences.Default.Get<string?>(ThemeKey, null);
            string? prevSelectedTheme = Preferences.Default.Get<string?>(PreviousThemeKey, null);
            if (PreviousThemeKey != null)
            {
                Preferences.Default.Set(PreviousThemeKey, prevSelectedTheme);
            }

            if (selectedTheme is null && Application.Current != null && Application.Current.UserAppTheme == AppTheme.Dark)
            {
                selectedTheme = nameof(Themes.Dark);
            }

            // Set the theme
            SetTheme(selectedTheme ?? nameof(Themes.Default));
        }
        private static ResourceDictionary LoadResourceDictionary(string resourcePath)
        {
            var assembly = typeof(ThemeManager).GetTypeInfo().Assembly;
            using (var stream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream == null)
                    throw new InvalidOperationException($"Resource {resourcePath} not found.");

                using (var reader = new StreamReader(stream))
                {
                    var xaml = reader.ReadToEnd();
                    return new ResourceDictionary().LoadFromXaml(xaml);
                }
            }
        }

        private static void Current_RequestedThemeChanged_(object? sender, AppThemeChangedEventArgs e)
        {
            SetTheme(e.RequestedTheme == AppTheme.Dark ? nameof(Themes.Dark) : nameof(Themes.Default));
        }
        private static void Current_RequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
        {
            if(e.RequestedTheme == AppTheme.Dark)
            {
                if (SelectedTheme != nameof(Themes.Dark))
                {
                    Preferences.Default.Set(PreviousThemeKey, SelectedTheme);
                }
                SetTheme(nameof(Themes.Dark));
            }
            else
            {
                var previousTheme = Preferences.Default.Get<string>(PreviousThemeKey, nameof(Themes.Default));
                SetTheme(previousTheme);
            }
        }

        public static void SetTheme(string themeName)
        {
            if (SelectedTheme == themeName)
                return;

            var mergedDicts = Application.Current?.Resources.MergedDictionaries;
            
            // Remove the current theme
            var currentTheme = mergedDicts?.FirstOrDefault(d => d.Source?.OriginalString.Contains("/Themes/") == true);
            if (currentTheme != null)
                mergedDicts?.Remove(currentTheme);

            if(themeName == "")
                themeName = nameof(Themes.Default);

            // Add the new theme
            mergedDicts?.Add(_themesMap[themeName]);

            SelectedTheme = themeName;
            SelectedThemeChanged?.Invoke(null, EventArgs.Empty);
            Preferences.Default.Set<string>(ThemeKey, themeName);
        }
    }
}
