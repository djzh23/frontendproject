using System.Collections;

namespace ppm_fe.Controls;

public partial class LabeledPicker : ContentView
{
    public static readonly BindableProperty MyLabelProperty =
        BindableProperty.Create(nameof(MyLabel), typeof(string), typeof(LabeledPicker), string.Empty);

    public static readonly BindableProperty ItemsSourceProperty =
    BindableProperty.Create(nameof(ItemsSource), typeof(IList), typeof(LabeledPicker), null);

    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(LabeledPicker), null, BindingMode.TwoWay);

    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(nameof(FontSize), typeof(double), typeof(LabeledPicker), 14.0);

    public string MyLabel
    {
        get => (string)GetValue(MyLabelProperty);
        set => SetValue(MyLabelProperty, value);
    }

    public IEnumerable ItemsSource
    {
        get => (IList)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public LabeledPicker()
    {
        InitializeComponent();
    }
}