using System.Windows.Input;

namespace ppm_fe.Controls;

public partial class CustomButton : ContentView
{
    public static readonly BindableProperty ButtonTextProperty =
            BindableProperty.Create(nameof(ButtonText), typeof(string), typeof(CustomButton), string.Empty);
    public string ButtonText
    {
        get => (string)GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }


    public static readonly BindableProperty ButtonTextColor =
            BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(CustomButton), Colors.White);
    public Color TextColor
    {
        get => (Color)GetValue(ButtonTextColor);
        set => SetValue(ButtonTextColor, value);
    }


    public static readonly BindableProperty EnabledColorProperty =
        BindableProperty.Create(nameof(EnabledColor), typeof(Color), typeof(CustomButton), Colors.Blue);
    public Color EnabledColor
    {
        get => (Color)GetValue(EnabledColorProperty);
        set => SetValue(EnabledColorProperty, value);
    }


    public static readonly BindableProperty DisabledColorProperty =
        BindableProperty.Create(nameof(DisabledColor), typeof(Color), typeof(CustomButton), Colors.Gray);
    public Color DisabledColor
    {
        get => (Color)GetValue(DisabledColorProperty);
        set => SetValue(DisabledColorProperty, value);
    }


    public static readonly BindableProperty ButtonCommandProperty =
        BindableProperty.Create(nameof(ButtonCommand), typeof(ICommand), typeof(CustomButton));
    public ICommand ButtonCommand
    {
        get => (ICommand)GetValue(ButtonCommandProperty);
        set => SetValue(ButtonCommandProperty, value);
    }

    public static readonly BindableProperty ButtonCommandParameterProperty =
    BindableProperty.Create(
        nameof(ButtonCommandParameter),
        typeof(object), // Allow any type
        typeof(CustomButton),
        default(object)
    );

    public object ButtonCommandParameter
    {
        get => (object)GetValue(ButtonCommandParameterProperty);
        set => SetValue(ButtonCommandParameterProperty, value);
    }


    public CustomButton()
    {
        InitializeComponent();
        this.PropertyChanged += CustomButton_PropertyChanged;
    }

    private void CustomButton_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IsEnabled))
        {
            InnerButton.BackgroundColor = IsEnabled ? EnabledColor : DisabledColor;
        }

        if (e.PropertyName == nameof(IsVisible))
        {
            InnerButton.IsVisible = IsVisible;
        }
    }
}