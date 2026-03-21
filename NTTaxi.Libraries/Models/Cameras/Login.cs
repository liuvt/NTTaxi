using System.Text.Json.Serialization;

namespace NTTaxi.Libraries.Models.Cameras;

public class Login
{

}

public class LoginDto
{

}


// Dữ liệu trả vế sau khi đăng nhập thành công
public class LoginResponse
{
    [JsonPropertyName("result")]
    public bool Result { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("data")]
    public List<LoginResponseData> Data { get; set; } = new();

    [JsonPropertyName("options")]
    public Dictionary<string, object> Options { get; set; } = new();
}

public class LoginResponseData
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonPropertyName("parentId")]
    public int ParentId { get; set; }

    [JsonPropertyName("userId")]
    public int UserId { get; set; }

    [JsonPropertyName("role")]
    public int Role { get; set; }

    [JsonPropertyName("level")]
    public int Level { get; set; }

    [JsonPropertyName("customerId")]
    public int CustomerId { get; set; }

    [JsonPropertyName("isMain")]
    public int IsMain { get; set; }
}