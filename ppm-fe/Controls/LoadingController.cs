using System.ComponentModel;

namespace ppm_fe.Controls
{
    public class LoadingController : INotifyPropertyChanged
    {
        private bool _isLoading;
        private string? _loadingMessage;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public string? LoadingMessage
        {
            get => _loadingMessage;
            set
            {
                _loadingMessage = value;
                OnPropertyChanged(nameof(LoadingMessage));
            }
        }

        public void StartLoading(string message = "Loading...")
        {
            IsLoading = true;
            LoadingMessage = message;
        }

        public void StopLoading()
        {
            IsLoading = false;
            LoadingMessage = string.Empty;
        }

        // INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
