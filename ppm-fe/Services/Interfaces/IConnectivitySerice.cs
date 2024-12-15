namespace ppm_fe.Services
{
    public interface IConnectivityService
    {
        bool IsConnected { get; }
        event EventHandler<ConnectivityChangedEventArgs> ConnectivityChanged;
    }
}
