using ppm_fe.Services;

namespace ppm_fe.ViewModels.Pages
{
    public partial class SettingsPageViewModel : BaseViewModel
    {
        public SettingsPageViewModel(IConnectivityService connectivityService)
        {
            ConnectivityService = connectivityService;
        }
    }
}
