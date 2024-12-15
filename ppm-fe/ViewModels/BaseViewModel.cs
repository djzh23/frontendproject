using CommunityToolkit.Mvvm.ComponentModel;
using ppm_fe.Services;
using System.Diagnostics;

namespace ppm_fe.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        protected bool _isBusy;

        [ObservableProperty]
        private string? _title;

        private IConnectivityService _connectivityService;
        protected IConnectivityService ConnectivityService
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

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public BaseViewModel()
        {
            // Empty constructor
        }

        private void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            IsConnected = e.NetworkAccess == NetworkAccess.Internet;
            HandleConnectivityChanged();
        }

        protected virtual void HandleConnectivityChanged()
        {
            // Override in derived view models to handle connectivity changes
        }

        static protected async Task DisplayAlertAsync(string title, string message)
        {
            // Get the current window (assuming a single-window app for now)
            var windows = Application.Current?.Windows;
            var currentWindow = windows != null && windows.Count > 0 ? windows[0] : null;

            if (currentWindow?.Page != null)
            {
                await currentWindow.Page.DisplayAlert(title, message, "OK");
            }
            else
            {
                // Handle the case where there's no current window or page
                Debug.WriteLine("No current window or page found. Cannot display alert.");
            }
        }
    }
}
