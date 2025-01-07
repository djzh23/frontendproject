using System.Diagnostics;
using System.Text.Json;

namespace ppm_fe.Helpers
{
    public class LogInfo
    {
        public string? ClassName { get; set; }
        public string? FunctionName { get; set; }
        public string? Properties { get; set; }
        public object? Inputs { get; set; }
        public int? UserId { get; set; }
        public string? StackTrace { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public static class LoggerHelper
    {
        public static void LogError(string className, string functionName, string errorMessage, object? inputs, string? stackTrace)
        {
            var logInfo = new LogInfo
            {
                ClassName = className,
                FunctionName = functionName,
                Properties = errorMessage,
                Inputs = inputs,
                UserId = App.CurrentUserId,
                StackTrace = stackTrace,
                Timestamp = DateTime.UtcNow
            };

            string jsonLog = JsonSerializer.Serialize(logInfo, new JsonSerializerOptions { WriteIndented = true });

            Debug.WriteLine(jsonLog);
        }
    }
}