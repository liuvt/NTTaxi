using System.Text.Json.Serialization;

namespace NTTaxi.Libraries.Models.Gsms
{
    public class GsmUser
    {
        [JsonPropertyName("email")] //ltvan.ntbl@namthanggroup.vn
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("password")] //@2AqDm8{
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("platform")] //website
        public string Platform { get; set; } = string.Empty;
    }

    public class GsmAuthResponse
    {
        [JsonPropertyName("data")]
        public GsmAuthData? Data { get; set; }
    }

    public class GsmAuthData
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("phone")]
        public string Phone { get; set; } = string.Empty;

        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("aud")]
        public string Audience { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("has_password")]
        public bool HasPassword { get; set; }

        [JsonPropertyName("partner_id")]
        public Guid PartnerId { get; set; }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
