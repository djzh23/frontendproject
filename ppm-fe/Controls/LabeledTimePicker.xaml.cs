using System.Windows.Input;

namespace ppm_fe.Controls;

public partial class LabeledTimePicker : ContentView
{
   
    public static readonly BindableProperty TimeProperty =
        BindableProperty.Create(nameof(Time), typeof(TimeSpan), typeof(LabeledTimePicker), default(TimeSpan), BindingMode.TwoWay, propertyChanged: OnTimePropertyChanged);

    public static readonly BindableProperty TimeChangedCommandProperty =
        BindableProperty.Create(nameof(TimeChangedCommand), typeof(ICommand), typeof(LabeledTimePicker), null);
   
    public static readonly BindableProperty LabelTextProperty =
        BindableProperty.Create(nameof(LabelText), typeof(string), typeof(LabeledTimePicker), default(string));
    public string LabelText
    {
        get => (string)GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }
    public TimeSpan Time
    {
        get => (TimeSpan)GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
    }

    public ICommand TimeChangedCommand
    {
        get => (ICommand)GetValue(TimeChangedCommandProperty);
        set => SetValue(TimeChangedCommandProperty, value);
    }

    public LabeledTimePicker()
    {
        InitializeComponent();
    }

    private static void OnTimePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LabeledTimePicker control)
        {
            control.TimeChangedCommand?.Execute(newValue);
        }
    }
}