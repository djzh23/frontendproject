namespace ppm_fe.Controls;

    public partial class LabeledEntryFrame : ContentView
    {
        public static readonly BindableProperty IconSourceProperty =
                BindableProperty.Create(nameof(IconSource), typeof(string), typeof(LabeledEntryFrame), default(string));

        public static readonly BindableProperty EntryTextProperty =
            BindableProperty.Create(nameof(EntryText), typeof(string), typeof(LabeledEntryFrame), default(string), BindingMode.TwoWay);

        public static readonly BindableProperty PlaceholderTextProperty =
            BindableProperty.Create(nameof(PlaceholderText), typeof(string), typeof(LabeledEntryFrame), default(string));

        public static readonly BindableProperty KeyboardTypeProperty =
            BindableProperty.Create(nameof(KeyboardType), typeof(Keyboard), typeof(LabeledEntryFrame), Keyboard.Default);

        public static readonly BindableProperty IsPasswordProperty =
            BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(LabeledEntryFrame), false, propertyChanged: OnIsPasswordChanged);

    public string IconSource
        {
            get => (string)GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        public string EntryText
        {
            get => (string)GetValue(EntryTextProperty);
            set => SetValue(EntryTextProperty, value);
        }

        public string PlaceholderText
        {
            get => (string)GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        public Keyboard KeyboardType
        {
            get => (Keyboard)GetValue(KeyboardTypeProperty);
            set => SetValue(KeyboardTypeProperty, value);
        }

        public bool IsPassword
        {
            get => (bool)GetValue(IsPasswordProperty);
            set => SetValue(IsPasswordProperty, value);
        }

    private static void OnIsPasswordChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (LabeledEntryFrame)bindable;
        bool isPassword = (bool)newValue;

        // Set the KeyboardType to Password if IsPassword is true
        control.KeyboardType = isPassword ? Keyboard.Create(KeyboardFlags.None) : Keyboard.Default;
    }


    public LabeledEntryFrame()
	    {
		    InitializeComponent();
	    }
    }