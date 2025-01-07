using Newtonsoft.Json;

namespace ppm_fe.Models
{
    public class Work
    {
        [JsonProperty("lastUpdated")]
        public DateTime LastUpdated { get; set; }


        [JsonProperty("creator_id")]
        public int? CreatorId { get; set; }


        [JsonProperty("creator_name")]
        public string? CreatorName { get; set; }


        [JsonProperty("status")]
        public string? Status { get; set; }


        [JsonProperty("date")]
        public string? Date { get; set; }


        [JsonProperty("team")]
        public string? Team { get; set; }


        [JsonProperty("ort")]
        public string? Ort { get; set; }


        [JsonProperty("vorort")]
        public bool Vorort { get; set; }


        [JsonProperty("list_of_helpers")]
        public List<string> ListOfHelpers { get; set; } = new List<string>();


        [JsonProperty("plan")]
        public string? Plan { get; set; }


        [JsonProperty("start_work")]
        public string? StartWork { get; set; }


        [JsonProperty("id")]
        public int Id { get; set; }


        [JsonProperty("reflection")]
        public string? Reflection { get; set; }


        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }


        [JsonProperty("defect")]
        public string Defect { get; set; } = string.Empty;


        [JsonProperty("parent_contact")]
        public string ParentContact { get; set; } = string.Empty;


        [JsonProperty("wellbeing_of_children")]
        public string WellbeingOfChildren { get; set; } = string.Empty;


        [JsonProperty("notes")]
        public string Notes { get; set; } = string.Empty;


        [JsonProperty("wishes")]
        public string Wishes { get; set; } = string.Empty;


        [JsonProperty("end_work")]
        public string? EndWork { get; set; }


        [JsonProperty("kids_data")]
        public List<WorkKidsData>? KidsData { get; set; }


        [JsonProperty("pdf_file")]
        public string? PdfFile { get; set; }


        public Work(DateTime date, string? status, string? team, string? ort, bool vorort, string? plan, TimeSpan startWork, List<string> listofhelpers)
        {
            Date = date.ToString("yyyy-MM-dd");  // Convert date to the expected format
            Status = status;
            Team = team;
            Ort = ort;
            Vorort = vorort;
            Plan = plan;
            StartWork = startWork.ToString(@"hh\:mm");
            ListOfHelpers = listofhelpers;
        }

        // Parameterless constructor for deserialization
        public Work() { }
    }

    public class WorkKidsData
    {
        [JsonProperty("work_id")]
        public int WorkId { get; set; }


        [JsonProperty("age_range")]
        public string? AgeRange { get; set; }


        [JsonProperty("age_group_id")]
        public int AgeGroupId { get; set; }


        [JsonProperty("boys")]
        public int NumberBoys { get; set; }


        [JsonProperty("girls")]
        public int NumberGirls { get; set; }
    }
}
