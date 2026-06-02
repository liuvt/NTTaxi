using Microsoft.Extensions.Logging;
using NTTaxi.Libraries.Models.Cameras;
using NTTaxi.Libraries.Services.Interfaces;
using System.Net;
using System.Text;
using System.Text.Json;

namespace NTTaxi.Libraries.Services;

public class CameraService : ICameraService
{
    private readonly HttpClient _client;
    private readonly CookieContainer _cookieContainer;
    private readonly ILogger<CameraService> _logger;

    private const string EndpointLogin = "api/v1/users/login";
    private static readonly Uri BaseUri = new("https://api.midvietnam.net/");

    // Constructor không cần tham số
    public CameraService(ILogger<CameraService> logger)
    {
        _logger = logger;
        _cookieContainer = new CookieContainer();

        var handler = new HttpClientHandler
        {
            CookieContainer = _cookieContainer,
            AllowAutoRedirect = true,
            MaxConnectionsPerServer = 3
        };

        _client = new HttpClient(handler)
        {
            BaseAddress = BaseUri,
            Timeout = Timeout.InfiniteTimeSpan
        };
    }

    #region Authentication
    public async Task<bool> GetAuthenticationAsync(CameraLoginRequest user, CancellationToken cancellationToken = default)
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(user, jsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _client.PostAsync(EndpointLogin, content, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Login thất bại. HTTP {(int)response.StatusCode} {response.StatusCode}. Body: {body}");
            }

            var apiResponse = JsonSerializer.Deserialize<CameraLoginApiResponse>(body, jsonOptions);

            if (apiResponse == null)
                throw new Exception("Không deserialize được dữ liệu trả về từ API.");

            if (!apiResponse.Result)
                throw new Exception($"API báo lỗi: {apiResponse.Message}");

            var tokenData = apiResponse.Data.FirstOrDefault();
            if (tokenData == null)
                throw new Exception("API không trả về token trong data.");

            var authFile = new CameraAuthenticationFile
            {
                User = new CameraLoginRequest
                {
                    Username = user.Username,
                    Password = user.Password,
                    DeviceToken = user.DeviceToken
                },
                Token = new CameraTokenFile
                {
                    Token = tokenData.Token,
                    RefreshToken = tokenData.RefreshToken
                }
            };

            var fileContent = JsonSerializer.Serialize(authFile, jsonOptions);
            await File.WriteAllTextAsync(
                "CameraAuthentication.json",
                fileContent,
                new UTF8Encoding(false),
                cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đăng nhập camera API");
            throw new Exception($"Chi tiết: {ex.Message}", ex);
        }
    }
    #endregion

}
