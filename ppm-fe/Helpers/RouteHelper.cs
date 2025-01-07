namespace ppm_fe.Helpers
{
    // Generate dynamic API based-route for Local testing or using the hosted Backend
    public static class RouteHelper
    {
        public static readonly bool LOCAL_BACKEND = false;

        private static readonly string _localBaseUrl = DeviceInfo.Platform == DevicePlatform.Android
        ? (DeviceInfo.DeviceType == DeviceType.Virtual ? "http://10.0.2.2:8000/api" : "http://YOUR_IP_ADRESS:8000/api")
        : "http://127.0.0.1:8000/api";


        private static readonly string _baseUrl = DeviceInfo.Platform == DevicePlatform.Android
        ? (DeviceInfo.DeviceType == DeviceType.Virtual ? "http://64.225.93.52/api" : "http://64.225.93.52/api")
        : "http://64.225.93.52/api";

        public static string BaseUrl = LOCAL_BACKEND ? _localBaseUrl : _baseUrl;

        public static string GetFullUrl(string endpoint)
        {
            return $"{BaseUrl}/{endpoint.TrimStart('/')}";
        }
    }   
}
