namespace ppm_fe.Behaviors
{
    public class RefreshPageBehavior : Behavior<Page>
    {
        protected override void OnAttachedTo(Page bindable)
        {
            base.OnAttachedTo(bindable);

            // Force the page to refresh
            bindable.PropertyChanged += (sender, e) => { };
        }

        protected override void OnDetachingFrom(Page bindable)
        {
            base.OnDetachingFrom(bindable);
        }
    }
}
