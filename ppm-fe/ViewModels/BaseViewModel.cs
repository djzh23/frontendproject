using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ppm_fe.Messages;
using ppm_fe.Services;
using ppm_fe.Services.Interfaces;

namespace ppm_fe.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        protected static bool _isUserCacheInitialized;
        private static readonly object _lock = new();

        // Static constructor to run once for all instances
        static BaseViewModel()
        {
        }

        #region properties
        [ObservableProperty]
        protected bool _isConnected;

        [ObservableProperty]
        protected bool _isLoading;

        private IConnectivityService? _connectivityService;
        protected IConnectivityService? ConnectivityService
        {
            get => _connectivityService;
            set
            {
                if (_connectivityService != value)
                {
                    if (_connectivityService != null)
                    {
                        _connectivityService.ConnectivityChanged -= OnConnectivityChanged;
                    }
                    _connectivityService = value;
                    if (_connectivityService != null)
                    {
                        IsConnected = _connectivityService.IsConnected;
                        _connectivityService.ConnectivityChanged += OnConnectivityChanged;
                    }
                }
            }
        }

        private static ICacheService? _cacheService;
        protected static ICacheService CacheService
        {
            get
            {
                EnsureInitialized();
                return _cacheService!;
            }
        }
        #endregion

        #region tasks
        private static void EnsureInitialized()
        {
            if (!_isUserCacheInitialized)
            {
                lock (_lock)
                {
                    if (!_isUserCacheInitialized)
                    {
                        if (_cacheService != null && App.UserDetails != null)
                        {
                            // Send the message to add User to cache
                            WeakReferenceMessenger.Default.Send(new UserMessage(App.UserDetails));
                        }
                        _isUserCacheInitialized = true;
                    }
                }
            }
        }

        public static void Initialize(ICacheService cacheService)
        {
            if (cacheService == null)
                throw new ArgumentNullException(nameof(cacheService));

            lock (_lock)
            {
                _cacheService = cacheService;
                if (App.UserDetails != null)
                {
                    // Send the message to add User to cache
                    WeakReferenceMessenger.Default.Send(new UserMessage(App.UserDetails));
                }
            }
        }

        private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            IsConnected = e.NetworkAccess == NetworkAccess.Internet;
            HandleConnectivityChanged();
        }

        protected virtual void HandleConnectivityChanged()
        {
            // Override in derived view models to handle connectivity changes
        }


        protected static async Task DisplayAlertAsync(string title, string? message)
        {
            // Get the current window (assuming a single-window app for now)
            var windows = Application.Current?.Windows;
            var currentWindow = windows != null && windows.Count > 0 ? windows[0] : null;

            if (currentWindow?.Page != null)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await currentWindow.Page.DisplayAlert(title, message, "OK");
                });
            }
        }

        protected static async Task<bool> DisplayAlertWithActionAsync(string title, string? message)
        {
            // Get the current window (assuming a single-window app for now)
            var windows = Application.Current?.Windows;
            var currentWindow = windows != null && windows.Count > 0 ? windows[0] : null;

            if (currentWindow?.Page != null)
            {
                return await currentWindow.Page.DisplayAlert(title, message, "Weiter", "Abbrechen");
            }
            else
            {
                // Handle the case where there's no current window or page
                return false;
            }
        }
        #endregion
    }
}
