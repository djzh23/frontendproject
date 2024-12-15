using System.Windows.Input;

namespace ppm_fe.Controls;

public partial class EditableLabel : ContentView
{
    public static readonly BindableProperty TextProperty =
            BindableProperty.Create(nameof(Text), typeof(string), typeof(EditableLabel), default(string), BindingMode.TwoWay);

    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(EditableLabel), default(string));

    public static readonly BindableProperty IsEditingProperty =
        BindableProperty.Create(nameof(IsEditing), typeof(bool), typeof(EditableLabel), false, BindingMode.TwoWay);

    public static readonly BindableProperty EditCompletedCommandProperty =
        BindableProperty.Create(nameof(EditCompletedCommand), typeof(ICommand), typeof(EditableLabel));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public bool IsEditing
    {
        get => (bool)GetValue(IsEditingProperty);
        set => SetValue(IsEditingProperty, value);
    }

    public ICommand EditCompletedCommand
    {
        get => (ICommand)GetValue(EditCompletedCommandProperty);
        set => SetValue(EditCompletedCommandProperty, value);
    }

    public EditableLabel()
    {
        InitializeComponent();
    }

    private void OnLabelTapped(object sender, EventArgs e)
    {
        IsEditing = true;
        UpdateVisibility();
        EditEntry.Focus();
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(IsEditing))
        {
            UpdateVisibility();
            if (!IsEditing)
            {
                EditCompletedCommand?.Execute(Text);
            }
        }
    }

    private void UpdateVisibility()
    {
        DisplayLabel.IsVisible = !IsEditing;
        EditEntry.IsVisible = IsEditing;
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler != null)
        {
            EditEntry.Completed += OnEntryCompleted;
            EditEntry.Unfocused += OnEntryUnfocused;
        }
        else
        {
            EditEntry.Completed -= OnEntryCompleted;
            EditEntry.Unfocused -= OnEntryUnfocused;
        }
    }

    private void OnEntryCompleted(object sender, EventArgs e)
    {
        FinishEditing();
    }

    private void OnEntryUnfocused(object sender, FocusEventArgs e)
    {
        FinishEditing();
    }

    private void FinishEditing()
    {
        IsEditing = false;
    }
}
