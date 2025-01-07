using ppm_fe.ViewModels.Pages;

namespace ppm_fe.Views.Page;

public partial class StatisticsPage : ContentPage
{
    public StatisticsPage(StatisticsPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}