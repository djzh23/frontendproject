using System.Collections.ObjectModel;

namespace ppm_fe.Constants
{
    // Constants used across the app
    public static class Properties
    {
        public static readonly string TokenKey = "token";
        public static readonly string HasAuthKey = "hasAuth";
        public static readonly string UserDataKey = "user_data";

        public static readonly ObservableCollection<string> Months = new ObservableCollection<string>
        {
            "Januar", "Februar", "März", "April", "Mai", "Juni", "Juli", "August", "September", "Oktober", "November", "Dezember"
        };

        public static readonly ObservableCollection<string> Teams = new ObservableCollection<string>
        {
            "FF1", "FF2", "FF3", "FF4", "FF5"
        };

        public const string UnexpectedError = "Ein unerwarteter Fehler ist aufgetreten";
    }
}
