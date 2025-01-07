using ppm_fe.ViewModels.HomePages;

namespace ppm_fe.Views.HomePages;

public partial class SuperAdminHomePage : ContentPage
{
	public SuperAdminHomePage(SuperAdminHomePageViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}