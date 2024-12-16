using ppm_fe.ViewModels.Pages;

namespace ppm_fe.Views.Page;

public partial class CreateWorkPage : ContentPage
{
    public CreateWorkPage(CreateWorkPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}