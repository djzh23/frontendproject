using ppm_fe.ViewModels.Dashboards;

namespace ppm_fe.Views.Dashboards;

public partial class SuperAdminDashboardPage : ContentPage
{
	public SuperAdminDashboardPage(SuperAdminDashboardPageViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}