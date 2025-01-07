namespace ppm_fe.Messages
{
    public class BillingMessage
    {
        public bool RefreshCache { get; }

        public BillingMessage(bool refreshCache = true)
        {
            RefreshCache = refreshCache;
        }
    }
}
