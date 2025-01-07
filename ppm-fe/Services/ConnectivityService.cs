namespace ppm_fe.Services
{
    // This service monitors and provides the connectivity status when a user's device loses its connection
    public class ConnectivityService : IConnectivityService
    {
        // Property that stores the connectivity status
        public bool IsConnected => Connectivity.NetworkAccess == NetworkAccess.Internet;

        // Event raised when the connectivity state changes on the device
        public event EventHandler<ConnectivityChangedEventArgs>? ConnectivityChanged;

        // Initializes a new instance of the ConnectivityService class and viewmodels to the user's device connectivity change events
        public ConnectivityService()
        {
            Connectivity.ConnectivityChanged += OnConnectivityChanged;
        }

        // Handles the user's device connectivity change event and raises the ConnectivityChanged event for viewmodels
        private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            ConnectivityChanged?.Invoke(this, e);
        }
    }
}
