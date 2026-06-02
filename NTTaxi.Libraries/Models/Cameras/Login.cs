using System.Text.Json.Serialization;

namespace NTTaxi.Libraries.Models.Cameras;

public class CameraLogin
{
    
}

public class CameraLoginRequest
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("device_token")]
    public string DeviceToken { get; set; } = "false";
}

//Trả về khi login thành công, có thể dùng để lấy token và refresh token
public class CameraLoginApiResponse
{
    [JsonPropertyName("result")]
    public bool Result { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("data")]
    public List<CameraLoginResponseData> Data { get; set; } = new();

    [JsonPropertyName("options")]
    public Dictionary<string, object> Options { get; set; } = new();
}

public class CameraLoginResponseData
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

// Ghi ra file json
public class CameraAuthenticationFile
{
    public CameraLoginRequest User { get; set; } = new();
    public CameraTokenFile Token { get; set; } = new();
}

public class CameraTokenFile
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; } = string.Empty;
}