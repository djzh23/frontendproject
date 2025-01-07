using System.ComponentModel;

namespace ppm_fe.Controls
{
    public class LoadingController : INotifyPropertyChanged
    {
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        private string? _loadingMessage;
        public string? LoadingMessage
        {
            get => _loadingMessage;
            set
            {
                _loadingMessage = value;
                OnPropertyChanged(nameof(LoadingMessage));
            }
        }

        public void StartLoading(string message = "Wird geladen...")
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                IsLoading = true;
                LoadingMessage = message;
            });
        }

        public void StopLoading()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                IsLoading = false;
                LoadingMessage = string.Empty;
            });
        }

        // INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
