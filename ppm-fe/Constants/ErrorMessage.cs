namespace ppm_fe.Constants
{
    public class ErrorMessage
    {
        public const string UnexpectedError = "Ein unerwarteter Fehler ist aufgetreten";
        public const string NotAuthorizedError = "Benutzer ist nicht berechtigt, diese Aktion auszuführen";
  
        public static string NotFoundErrorHelper(string? action = null)
        {
            if(action == "works")
            {
                return "Die Daten konnten nicht abgerufen werden";
            }
            else if(action == "billings")
            {
                return "Die Daten konnten nicht gelöscht werden";
            }
            else
            {
                return "Die Daten konnten nicht gefunden werden";
            }
        }
    }
}
