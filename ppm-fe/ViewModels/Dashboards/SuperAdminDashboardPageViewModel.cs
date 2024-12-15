using ppm_fe.Services;

namespace ppm_fe.ViewModels.Dashboards
{
    public partial class SuperAdminDashboardPageViewModel : BaseViewModel
    {
        public SuperAdminDashboardPageViewModel(IConnectivityService connectivityService)
        {
            ConnectivityService = connectivityService;
        }
    }
}
