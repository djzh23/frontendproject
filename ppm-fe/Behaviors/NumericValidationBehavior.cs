namespace ppm_fe.Behaviors
{
    public class NumericValidationBehavior : Behavior<Entry>
    {
        protected override void OnAttachedTo(Entry entry)
        {
            entry.TextChanged += OnEntryTextChanged;
            entry.Unfocused += OnEntryUnfocused;
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            entry.TextChanged -= OnEntryTextChanged;
            entry.Unfocused -= OnEntryUnfocused;
            base.OnDetachingFrom(entry);
        }

        private void OnEntryTextChanged(object? sender, TextChangedEventArgs args)
        {
            if (sender is Entry entry)
            {
                
                if (!string.IsNullOrEmpty(args.NewTextValue))
                {
                    if (args.NewTextValue.StartsWith("0") && args.NewTextValue.Length > 1)
                    {
                        entry.Text = int.Parse(args.NewTextValue).ToString();
                        return;
                    }

                    if (!int.TryParse(args.NewTextValue, out _))
                    {
                        entry.Text = args.OldTextValue;
                    }
                }
            }
        }
        private void OnEntryUnfocused(object? sender, FocusEventArgs e)
        {
            if (sender is Entry entry)
            {
                if (string.IsNullOrEmpty(entry.Text))
                {
                    entry.Text = "0";
                }
                else if (entry.Text.StartsWith("0") && entry.Text.Length > 1)
                {
                    entry.Text = int.Parse(entry.Text).ToString();
                }
            }
        }
    }
}
