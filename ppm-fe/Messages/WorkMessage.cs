namespace ppm_fe.Messages
{
    public class WorkMessage
    {
        public bool RefreshCache { get; }

        public WorkMessage(bool refreshCache = true)
        {
            RefreshCache = refreshCache;
        }
    }
}
