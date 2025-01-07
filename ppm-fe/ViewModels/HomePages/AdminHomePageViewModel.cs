using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using ppm_fe.Helpers;
using ppm_fe.Services;

namespace ppm_fe.ViewModels.HomePages;

public partial class AdminHomePageViewModel : BaseViewModel
{
    public AdminHomePageViewModel(IConnectivityService connectivityService)
    {
        ConnectivityService = connectivityService;
        RefreshCommand = new Command(async () => await RefreshDataAsync());
    }

    #region properties
    [ObservableProperty]
    private int _worksCount;

    [ObservableProperty]
    private int _incompleteWorksCount;

    [ObservableProperty]
    private bool _isRefreshing;
    #endregion

    #region commands
    public ICommand RefreshCommand { get; }
    #endregion

    #region tasks
    public async Task RefreshDataAsync()
    {
        try
        {
            IsRefreshing = true;
            Dictionary<string, int> dashboard = await CacheService.GetAdminHomePageCounts();
            WorksCount = dashboard["totalWorks"];
            IncompleteWorksCount = dashboard["incompleteWorks"];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error refreshing data: {ex.Message}");
            LoggerHelper.LogError(this.GetType().Name, nameof(RefreshDataAsync), $"Error refreshing data: {ex.Message}", new { }, null);
        }
        finally
        {
            IsRefreshing = false;
        }
    }
    #endregion
}
