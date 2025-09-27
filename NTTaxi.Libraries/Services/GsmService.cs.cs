using NTTaxi.Libraries.Models.Gsms;
using NTTaxi.Libraries.Services.Interfaces;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace NTTaxi.Libraries.Services
{
    public class GsmService : IGsmService
    {
        private readonly HttpClient _httpClient;
        private readonly CookieContainer _cookieContainer;
        private readonly string _baseUrl = "https://admin-api.vn.gsm-api.net";
        private readonly string _baseUrlOriginal = "https://admin-customer.xanhsm.com";
        private readonly string _endpointLogin = "/auth/v1/public/admin/login";
        private readonly string _endpointTransactions = "/wallet/v1/public/api/admin/transaction?";
        private readonly string _endpointOrders = "/order-statistic/v2/public/list-orders";
        private readonly string _endpointDriver = "/account/v1/public/admin/supplier/export?country_code=VNM";
        private readonly string _endpointDriverStatus = "/account/v1/public/partner/supplier/status";

        public GsmService()
        {
            _cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = _cookieContainer,
                AllowAutoRedirect = true, // phải bật để theo redirect tự động
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
            };

            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Referrer = new Uri(_baseUrlOriginal + "/");
            _httpClient.DefaultRequestHeaders.Add("origin", _baseUrlOriginal);
        }

        public async Task<GsmAuthData> GetAuthTokenAsync(GsmUser _user)
        {
            try
            {
                var loginPayload = new
                {
                    email = _user.Email,
                    password = _user.Password,
                    platform = "website"
                };


                var json = JsonSerializer.Serialize(loginPayload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_baseUrl + _endpointLogin, content);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Login failed: " + response.StatusCode);
                    return null;
                }

                var responseText = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<GsmAuthResponse>(responseText);

                return loginResponse?.Data ?? new GsmAuthData();
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi kết nối đến server! " + ex.Message);
            }
           
        }
    }
}
