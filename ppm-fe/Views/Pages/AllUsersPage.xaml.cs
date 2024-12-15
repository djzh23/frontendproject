using ppm_fe.ViewModels.Pages;

namespace ppm_fe.Views.Page;

public partial class AllUsersPage : ContentPage
{
    public AllUsersPage(AllUsersPageViewModel viewModel)
    {
        InitializeComponent();
        this.BindingContext = viewModel;
    }
}