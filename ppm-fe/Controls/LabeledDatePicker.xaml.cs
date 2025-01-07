namespace ppm_fe.Controls;

public partial class LabeledDatePicker : ContentView
{
    public static readonly BindableProperty MyLabelProperty =
        BindableProperty.Create(nameof(MyLabel), typeof(string), typeof(LabeledDatePicker), string.Empty);

    public static readonly BindableProperty DateProperty =
        BindableProperty.Create(nameof(Date), typeof(DateTime), typeof(LabeledDatePicker), DateTime.Now, BindingMode.TwoWay);

    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(nameof(FontSize), typeof(double), typeof(LabeledDatePicker), 14.0);

    public static readonly BindableProperty MaximumDateProperty = BindableProperty.Create(
        nameof(MaximumDate),
        typeof(DateTime),
        typeof(LabeledDatePicker),
        DateTime.MaxValue);

    public string MyLabel
    {
        get => (string)GetValue(MyLabelProperty);
        set => SetValue(MyLabelProperty, value);
    }

    public DateTime Date
    {
        get => (DateTime)GetValue(DateProperty);
        set => SetValue(DateProperty, value);
    }

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public DateTime MaximumDate
    {
        get => (DateTime)GetValue(MaximumDateProperty);
        set => SetValue(MaximumDateProperty, value);
    }

    public LabeledDatePicker()
    {
        InitializeComponent();
    }
}