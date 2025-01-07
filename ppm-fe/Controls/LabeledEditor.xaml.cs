namespace ppm_fe.Controls;

public partial class LabeledEditor : ContentView
{
    public static readonly BindableProperty MyLabelProperty =
        BindableProperty.Create(nameof(MyLabel), typeof(string), typeof(LabeledEditor), default(string));

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(LabeledEditor), default(string), BindingMode.TwoWay);

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(LabeledEditor), default(string));

    public string MyLabel
    {
        get => (string)GetValue(MyLabelProperty);
        set => SetValue(MyLabelProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }
    public LabeledEditor()
	{
		InitializeComponent();
	}
}