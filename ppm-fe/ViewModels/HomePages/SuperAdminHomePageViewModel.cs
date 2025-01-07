using ppm_fe.Services;

namespace ppm_fe.ViewModels.HomePages
{
    public partial class SuperAdminHomePageViewModel : BaseViewModel
    {
        public SuperAdminHomePageViewModel(IConnectivityService connectivityService)
        {
            ConnectivityService = connectivityService;
        }
    }
}
