using System.Text.RegularExpressions;

namespace ppm_fe.Helpers
{
    public static class InputCleaner
    {
        public static string RemoveWhitespace(string input)
        {
            return string.IsNullOrEmpty(input) ? input : Regex.Replace(input, @"\s", "");
        }

        public static string CleanIBAN(string iban)
        {
            return RemoveWhitespace(iban).ToUpper();
        }

        public static string CleanBIC(string bic)
        {
            return RemoveWhitespace(bic).ToUpper();
        }

        public static string CleanEmail(string email)
        {
            return RemoveWhitespace(email).ToLower();
        }

        public static string SanitizeInput(string input)
        {
            // Remove potentially dangerous characters
            return Regex.Replace(input, @"[<>&'\""]", "");
        }
    }
}
