namespace ppm_fe.Controls;

public partial class LabeledSwitch : ContentView
{
    public static readonly BindableProperty MyLabelProperty =
        BindableProperty.Create(nameof(MyLabel), typeof(string), typeof(LabeledSwitch), string.Empty);

    public static readonly BindableProperty IsToggledProperty =
        BindableProperty.Create(nameof(IsToggled), typeof(bool), typeof(LabeledSwitch), false, BindingMode.TwoWay);

    public string MyLabel
    {
        get => (string)GetValue(MyLabelProperty);
        set => SetValue(MyLabelProperty, value);
    }

    public bool IsToggled
    {
        get => (bool)GetValue(IsToggledProperty);
        set => SetValue(IsToggledProperty, value);
    }

    public LabeledSwitch()
    {
        InitializeComponent();
    }
}