using System.Text.Json.Serialization;

namespace ppm_fe.Models
{
    public class User
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("firstname")]
        public string? Firstname { get; set; }
        [JsonPropertyName("lastname")]
        public string? Lastname { get; set; }

        [JsonPropertyName("password")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? Password { get; set; }
        [JsonPropertyName("email")]
        public string? Email { get; set; }
        [JsonPropertyName("steueridentifikationsnummer")]
        public string? Steueridentifikationsnummer { get; set; }
        [JsonPropertyName("street")]
        public string? Street { get; set; }

        [JsonPropertyName("number")]
        public string? Number { get; set; }
        [JsonPropertyName("pzl")]
        public string? Pzl { get; set; }
        [JsonPropertyName("city")]
        public string? City { get; set; }
        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("bank_name")]
        public string? bank_name { get; set; }
        [JsonPropertyName("iban")]
        public string? Iban { get; set; }
        [JsonPropertyName("bic")]

        public string? Bic { get; set; }

        [JsonPropertyName("role_id")]
        public int? Role_ID { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool? Approved { get; set; }

        public string RoleText { get; set; }
    }

    public enum UserRole
    {
        SuperAdmin = 1,
        Admin = 2,
        FestMitarbeiter = 3,
        Honorerkraft = 4,
        NoRole = 5
    }
}
