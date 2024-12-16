using ppm_fe.Helpers;
using ppm_fe.Services;
using ppm_fe.Services.Interfaces;
using System.Diagnostics;
using System.Windows.Input;

namespace ppm_fe.ViewModels.Dashboards;

public partial class AdminDashboardPageViewModel : BaseViewModel
{
    private readonly ICacheService _cacheService;

    private int _worksCount;
    public int WorksCount
    {
        get => _worksCount;
        set
        {
            _worksCount = value;
            OnPropertyChanged();
        }
    }


    private int _standingWorksCount;
    public int StandingWorksCount
    {
        get => _standingWorksCount;
        set
        {
            _standingWorksCount = value;
            OnPropertyChanged();
        }
    }


    private int _worksInProgressCount;
    public int WorksInProgressCount
    {
        get => _worksInProgressCount;
        set
        {
            _worksInProgressCount = value;
            OnPropertyChanged();
        }
    }


    private int _billingsCount;
    public int BillingsCount
    {
        get => _billingsCount;
        set
        {
            _billingsCount = value;
            OnPropertyChanged();
        }
    }


    private string _statusMessage;
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }
    }


    private bool _canGoBack;
    public bool CanGoBack
    {
        get => _canGoBack;
        set
        {
            if (_canGoBack != value)
            {
                _canGoBack = value;
                OnPropertyChanged();
            }
        }
    }
    

    private bool _isRefreshing;
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            _isRefreshing = value;
            OnPropertyChanged();
        }
    }

    public ICommand RefreshCommand { get; }

    public AdminDashboardPageViewModel(ICacheService cacheService, IConnectivityService connectivityService)
    {
        _cacheService = cacheService;
        ConnectivityService = connectivityService;

        RefreshCommand = new Command(async () => await RefreshDataAsync());
    }

    public async Task RefreshDataAsync()
    {
        try
        {
            IsRefreshing = true;
            Dictionary<string, int> dashboard = await _cacheService.GetAdminDashboardStats();
            WorksCount = dashboard["totalWorks"];
            StandingWorksCount = dashboard["standingWorks"];
            BillingsCount = dashboard["totalBillings"];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error refreshing data: {ex.Message}");
            LoggerHelper.LogError(this.GetType().Name, nameof(RefreshDataAsync), $"Error refreshing data: {ex.Message}", new { UserId = App.UserDetails.Id }, null);
        }
        finally
        {
            IsRefreshing = false;
        }
    }
}
