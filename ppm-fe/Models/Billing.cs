using Newtonsoft.Json;

namespace ppm_fe.Models
{
    public class Billing
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("date")]
        public string? Date { get; set; }

        [JsonProperty("month")]
        public string? Month { get; set; }

        [JsonProperty("billing_number")]
        public int BillingNumber { get; set; }

        [JsonProperty("billing_details")]
        public List<BillingDetail> BillingDetails = new List<BillingDetail> { };

        [JsonProperty("somme_all")]
        public double SommeAll { get; set; }

        [JsonProperty("pdf_file")]
        public string? PdfFileBilling { get; set; }
    }

    public class BillingDetail
    {
        public DateTime DateWorkDay { get; set; }
        public double NumberOfHours { get; set; }
        public double WorkDay { get; set; }
        public double Stundenlohn { get; set; }
    }

    public class BillingsInfoProfile : Billing
    {
        public string? Firstname { get; set; } 
        public string? Lastname { get; set; } 

        public string? UserAddress { get; set; }
        public string? UserPostalCode { get; set; }

        public string? Iban { get; set; }
        public string? Bic { get; set; }
        public string? BankName { get; set; }
        public string? Steueridentifikationsnummer { get; set; }
        public string? Role { get; set; }
    }
}
