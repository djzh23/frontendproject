using ppm_fe.Models;
using Newtonsoft.Json;

namespace ppm_fe.Services
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }

        public T? Data { get; set; }

        [JsonProperty("pagination")]
        public PaginationInfo? Pagination { get; set; }

    }

    public class PaginationInfo
    {
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("per_page")]
        public int PerPage { get; set; }

        [JsonProperty("current_page")]
        public int CurrentPage { get; set; }

        [JsonProperty("last_page")]
        public int LastPage { get; set; }

        [JsonProperty("from")]
        public int? From { get; set; }

        [JsonProperty("to")]
        public int? To { get; set; }

        [JsonProperty("first_page_url")]
        public string? FirstPageUrl { get; set; }

        [JsonProperty("last_page_url")]
        public string? LastPageUrl { get; set; }

        [JsonProperty("next_page_url")]
        public string? NextPageUrl { get; set; }

        [JsonProperty("prev_page_url")]
        public string? PrevPageUrl { get; set; }

        [JsonProperty("path")]
        public string? Path { get; set; }
    }

    public class LoginResponse
    {
        [JsonProperty("user")]
        public User? User { get; set; }

        [JsonProperty("token")]
        public string? Token { get; set; }

        [JsonProperty("role")]
        public string? RoleText { get; set; }

    }

    public class ForgetPasswordResponse
    {
        [JsonProperty("message")]
        public string? Message { get; set; }
    }

    public class UserSummary
    {
        public int Id { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? Email { get; set; }
        public RoleSummary? Role { get; set; }
        public int role_id { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public bool Approved { get; set; }
    }

    public class RoleSummary
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
