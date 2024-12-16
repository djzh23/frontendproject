using CommunityToolkit.Maui.Views;
using ppm_fe.ViewModels.Pages;
using System.Diagnostics;

namespace ppm_fe.Views.Page;

public partial class AllWorksPage : ContentPage
{
    private readonly AllWorksPageViewModel _viewModel;

    public AllWorksPage(AllWorksPageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel?.OnDisappearing();
        Debug.WriteLine("----------------------------> WorksPage OnDisappearing called");
        (_viewModel as IDisposable)?.Dispose();
    }

    // Ensure this method is defined in your WorksPage class
    private void OnExpanderTapped(object sender, EventArgs e)
    {

        if (_viewModel != null && !_viewModel.IsLoading) // Only toggle if not loading
        {
            _viewModel.IsExpanderOpen = !_viewModel.IsExpanderOpen;
        }
    }

    private async void OnExpanderPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Expander.IsExpanded) && sender is Expander expander)
        {
            var arrow = FindArrowImage(expander);
            if (arrow != null)
            {
                await AnimateArrow(expander, arrow);
            }
        }
    }
    public Image? FindArrowImage(Expander expander)
    {
        return (expander.Header as Grid)?.Children.OfType<Image>().FirstOrDefault();
    }
    private async Task AnimateArrow(Expander expander, Image arrowImage)
    {
        uint duration = 300; // Slightly longer duration for smoother effect
        double targetRotation = expander.IsExpanded ? 90 : 0;

        // Use ViewExtensions for more control over the animation
        await arrowImage.RotateTo(targetRotation, duration, Easing.SpringOut);

        // Optional: Add a subtle scale animation for extra smoothness
        await Task.WhenAll(
            arrowImage.RotateTo(targetRotation, duration, Easing.SpringOut),
            arrowImage.ScaleTo(1.1, duration / 2, Easing.SpringOut),
            arrowImage.ScaleTo(1, duration / 2, Easing.SpringIn)
        );
        //#if __IOS__ || __ANDROID__ || WINDOWS
        //            await Task.WhenAll(
        //                arrowImage.RotateTo(targetRotation, duration, Easing.SpringOut),
        //                arrowImage.ScaleTo(1.1, duration / 2, Easing.SpringOut),
        //                arrowImage.ScaleTo(1, duration / 2, Easing.SpringIn)
        //            );
        //#else
        //    // Provide an alternative implementation or do nothing
        //    await Task.CompletedTask;
        //#endif
    }

    private void OnHelperEntryUnfocused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry && BindingContext is AllWorksPageViewModel viewModel)
        {
            string oldHelper = entry.BindingContext as string;
            string newHelper = entry.Text;

            if (string.IsNullOrWhiteSpace(newHelper))
            {
                // If the new helper is empty, remove it
                viewModel.RemoveHelperCommand.Execute(oldHelper);
            }
            else if (oldHelper != newHelper)
            {
                // Update the helper only if it has changed
                viewModel.UpdateHelperCommand.Execute((oldHelper, newHelper));
            }
        }
    }

    private void OnNumberEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry)
        {
            // Entferne führende Nullen
            if (e.NewTextValue.StartsWith("0") && e.NewTextValue.Length > 1)
            {
                entry.Text = int.Parse(e.NewTextValue).ToString();
            }

            // Erlaube nur Zahlen
            if (!string.IsNullOrEmpty(e.NewTextValue) && !int.TryParse(e.NewTextValue, out _))
            {
                entry.Text = e.OldTextValue;
            }
        }
    }

    private void OnNumberEntryUnfocused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry)
        {
            // Wenn leer, setze auf "0"
            if (string.IsNullOrEmpty(entry.Text))
            {
                entry.Text = "0";
            }
            // Entferne führende Nullen
            else if (entry.Text.StartsWith("0") && entry.Text.Length > 1)
            {
                entry.Text = int.Parse(entry.Text).ToString();
            }
        }
    }


    //AllWorksPageViewModel _viewModel;

    //public AllWorksPage(AllWorksPageViewModel viewModel)
    //{
    //    InitializeComponent();
    //    this.BindingContext = viewModel;
    //    _viewModel = viewModel;
    //}

    //protected override async void OnAppearing()
    //{
    //    base.OnAppearing();
    //    await _viewModel.InitializeAsync();
    //}
    //protected override void OnDisappearing()
    //{
    //    base.OnDisappearing();
    //    _viewModel?.OnDisappearing();
    //    (_viewModel as IDisposable)?.Dispose();
    //}

    //// Ensure this method is defined in your AllUserWorksPage class
    //private void OnExpanderTapped(object sender, EventArgs e)
    //{
    //    if (_viewModel != null && !_viewModel.IsLoading) // Only toggle if not loading
    //    {
    //        _viewModel.IsExpanderOpen = !_viewModel.IsExpanderOpen;
    //    }
    //}

    //private async void OnExpanderPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    //{
    //    if (e.PropertyName == nameof(Expander.IsExpanded) && sender is Expander expander)
    //    {
    //        var arrow = FindArrowImage(expander);
    //        if (arrow != null)
    //        {
    //            await AnimateArrow(expander, arrow);
    //        }
    //    }
    //}

    //public Image? FindArrowImage(Expander expander)
    //{
    //    return (expander.Header as Grid)?.Children.OfType<Image>().FirstOrDefault();
    //}

    //private async Task AnimateArrow(Expander expander, Image arrowImage)
    //{
    //    uint duration = 300; // Slightly longer duration for smoother effect
    //    double targetRotation = expander.IsExpanded ? 90 : 0;

    //    // Use ViewExtensions for more control over the animation
    //    await arrowImage.RotateTo(targetRotation, duration, Easing.SpringOut);

    //    // Optional: Add a subtle scale animation for extra smoothness
    //    await Task.WhenAll(
    //        arrowImage.RotateTo(targetRotation, duration, Easing.SpringOut),
    //        arrowImage.ScaleTo(1.1, duration / 2, Easing.SpringOut),
    //        arrowImage.ScaleTo(1, duration / 2, Easing.SpringIn)
    //    );
    //    //#if __IOS__ || __ANDROID__ || WINDOWS
    //    //            await Task.WhenAll(
    //    //                arrowImage.RotateTo(targetRotation, duration, Easing.SpringOut),
    //    //                arrowImage.ScaleTo(1.1, duration / 2, Easing.SpringOut),
    //    //                arrowImage.ScaleTo(1, duration / 2, Easing.SpringIn)
    //    //            );
    //    //#else
    //    //    // Provide an alternative implementation or do nothing
    //    //    await Task.CompletedTask;
    //    //#endif
    //}

    //private void OnHelperEntryUnfocused(object sender, FocusEventArgs e)
    //{
    //    if (sender is Entry entry && BindingContext is AllWorksPageViewModel viewModel)
    //    {
    //        string oldHelper = entry.BindingContext as string;
    //        string newHelper = entry.Text;

    //        if (string.IsNullOrWhiteSpace(newHelper))
    //        {
    //            // If the new helper is empty, remove it
    //            viewModel.RemoveHelperCommand.Execute(oldHelper);
    //        }
    //        else if (oldHelper != newHelper)
    //        {
    //            // Update the helper only if it has changed
    //            viewModel.UpdateHelperCommand.Execute((oldHelper, newHelper));
    //        }
    //    }
    //}
}