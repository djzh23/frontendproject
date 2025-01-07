namespace ppm_fe.Messages
{
    public class HomePageMessage
    {
        public bool RefreshCache { get; }

        public HomePageMessage(bool refreshCache = true)
        {
            RefreshCache = refreshCache;
        }
    }
}
