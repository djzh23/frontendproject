namespace ppm_fe.Controls;

public partial class LabeledEntry : ContentView
{
    public static readonly BindableProperty MyLabelProperty =
       BindableProperty.Create(nameof(MyLabel), typeof(string), typeof(LabeledEntry), default(string));

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(LabeledEntry), default(string), BindingMode.TwoWay);

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(LabeledEntry), default(string));

    public static readonly BindableProperty KeyboardProperty =
        BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(LabeledEntry), Keyboard.Default);

    public static readonly BindableProperty FontSizeProperty =
    BindableProperty.Create(nameof(FontSize), typeof(double), typeof(LabeledEditor), default(double));
 
    public static readonly BindableProperty FontSizeLabelProperty =
    BindableProperty.Create(nameof(FontSizeLabel), typeof(double), typeof(LabeledEditor), default(double));

    public static readonly BindableProperty IsPasswordProperty =
            BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(LabeledEntry), false, propertyChanged: OnIsPasswordChanged);

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


    public Keyboard Keyboard
    {
        get => (Keyboard)GetValue(KeyboardProperty);
        set => SetValue(KeyboardProperty, value);
    }

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public double FontSizeLabel
    {
        get => (double)GetValue(FontSizeLabelProperty);
        set => SetValue(FontSizeLabelProperty, value);
    }

    public bool IsPassword
    {
        get => (bool)GetValue(IsPasswordProperty);
        set => SetValue(IsPasswordProperty, value);
    }


    private static void OnIsPasswordChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (LabeledEntry)bindable;
        bool isPassword = (bool)newValue;

        // Set the Keyboard to Password if IsPassword is true
        control.Keyboard = isPassword ? Keyboard.Create(KeyboardFlags.None) : Keyboard.Default;
    }

    public LabeledEntry()
    {
        InitializeComponent();
    }
}