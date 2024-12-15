namespace ppm_fe.Helpers
{
    public static class RouteHelper
    {
        //private static readonly string _baseUrl = DeviceInfo.Platform == DevicePlatform.Android
        //? (DeviceInfo.DeviceType == DeviceType.Virtual ? "http://10.0.2.2:8000/api" : "http://192.168.178.36:8000/api")
        //: "http://localhost:8000/api";

        private static readonly string _baseUrl = DeviceInfo.Platform == DevicePlatform.Android
        ? (DeviceInfo.DeviceType == DeviceType.Virtual ? "http://64.225.93.52/api" : "http://64.225.93.52/api")
        : "http://64.225.93.52/api";

        public static string BaseUrl => _baseUrl;

        public static string GetFullUrl(string endpoint)
        {
            return $"{BaseUrl}/{endpoint.TrimStart('/')}";
        }
    }
}
