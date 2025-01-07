namespace ppm_fe.Extensions
{
    // Used for changing the color of the Forget Password Link in Ui, used in LoginPage
    public static class ViewExtensions
    {
        public static Task<bool> ColorTo(this VisualElement self, Color fromColor, Color toColor, Action<Color> callback, uint length = 250)
        {
            Func<double, Color> transform = (t) =>
                new Color(
                    (float)(fromColor.Red + t * (toColor.Red - fromColor.Red)),
                    (float)(fromColor.Green + t * (toColor.Green - fromColor.Green)),
                    (float)(fromColor.Blue + t * (toColor.Blue - fromColor.Blue)),
                    (float)(fromColor.Alpha + t * (toColor.Alpha - fromColor.Alpha)));

            return ColorAnimation(self, "ColorTo", transform, callback, length);
        }

        public static Task<bool> ColorAnimation(VisualElement element, string name, Func<double, Color> transform, Action<Color> callback, uint length)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            element.Animate(name, transform, callback, 16, length, Easing.Linear, (v, c) => taskCompletionSource.SetResult(c));

            return taskCompletionSource.Task;
        }
    }
}
