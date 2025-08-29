using Google.Apis.Sheets.v4.Data;
using NTTaxi.Libraries.Extensions;
using NTTaxi.Libraries.Services.Interfaces;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace NTTaxi.Libraries.Services
{
    public class SkysoftService : ISkysoftService
    {
        private readonly HttpClient httpClient;
        private readonly CookieContainer cookieContainer;
        private readonly string _url = "https://tracking.skysoft.vn/ServletServer";

        public SkysoftService()
        {
            cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                UseCookies = true,
                AutomaticDecompression = DecompressionMethods.None // bỏ gzip/deflate
            };

            httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows 2000)");
        }
        /// <summary>
        /// Lấy cookie từ server (JSESSIONID + tokenID) và set thẳng vào cookieContainer
        /// </summary>
        public async Task<string> GetCookieAsync()
        {
            var requestTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var body = $"__DDTP__=$var.Function$..getProfile.$Var.Class$..icom.skysoft.gps.server.CommonBean.$Var.PrimaryKey$..skysoft.$requestTime$..{requestTime}";
            var content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await httpClient.PostAsync(_url, content);

            // Lấy tất cả Set-Cookie từ header và add vào CookieContainer
            if (response.Headers.TryGetValues("Set-Cookie", out var setCookies))
            {
                foreach (var cookieStr in setCookies)
                {
                    var cookie = ParseCookie(cookieStr, new Uri(_url));
                    if (cookie != null)
                    {
                        cookieContainer.Add(cookie);
                    }
                }
            }

            // Debug: in tất cả cookie
            var cookies = cookieContainer.GetCookies(new Uri(_url));
            foreach (Cookie c in cookies)
            {
                Console.WriteLine($"Cookie: {c.Name}={c.Value}; Domain={c.Domain}; Path={c.Path}");
            }

            return cookies["JSESSIONID"]?.Value;
        }

        /// <summary>
        /// Parse Set-Cookie string thành System.Net.Cookie
        /// </summary>
        private Cookie ParseCookie(string cookieStr, Uri uri)
        {
            try
            {
                var parts = cookieStr.Split(';', 2);
                var keyValue = parts[0].Split('=', 2);
                if (keyValue.Length != 2) return null;

                return new Cookie(keyValue[0].Trim(), Uri.UnescapeDataString(keyValue[1].Trim()), "/", uri.Host);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Tạo DDTP login request từ username/password
        /// </summary>
        private static byte[] CreateLoginDDTP(string username, string password, string sessionKey)
        {
            var ddtp = new DDTP(true); // compress
            ddtp.SetString("$Var.Function$", "login");
            ddtp.SetString("$Var.Class$", "com.skysoft.gps.server.CommonBean");
            ddtp.SetString("username", username);
            ddtp.SetString("password", password);
            ddtp.SetString("deviceID", "VNET");           // phải là device hợp lệ, ví dụ "M2" hoặc "X5"
            ddtp.SetString("requestSource", "CSharpClient"); // thường "CSharpClient"
            ddtp.SetString("os", "Windows");
            ddtp.SetString("serverIP", "");   // bỏ trống hoặc theo config server
            ddtp.SetString("serverPort", ""); // bỏ trống hoặc theo config server
            ddtp.SetString("$Var.SessionKey$", sessionKey);


            using var ms = new System.IO.MemoryStream();
            ddtp.Store(ms, true);
            return ms.ToArray();
        }

        /// <summary>
        /// Gửi request DDTP và nhận về byte[] response
        /// </summary>
        private async Task<byte[]> SendDDTPAsync(byte[] body)
        {
            var content = new ByteArrayContent(body);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var response = await httpClient.PostAsync(_url, content);
            response.EnsureSuccessStatusCode();

            // Lấy tất cả Set-Cookie từ header và add vào CookieContainer
            if (response.Headers.TryGetValues("Set-Cookie", out var setCookies))
            {
                foreach (var cookieStr in setCookies)
                {
                    var cookie = ParseCookie(cookieStr, new Uri(_url));
                    if (cookie != null)
                    {
                        cookieContainer.Add(cookie);
                    }
                }
            }

            return await response.Content.ReadAsByteArrayAsync();
        }

        /// <summary>
        /// Login và trả về DDTP object từ response
        /// </summary>
        public async Task<DDTP> LoginAsync(string username, string password, string sessionKey)
        {
            var loginBody = CreateLoginDDTP(username, password,sessionKey);
            var responseBytes = await SendDDTPAsync(loginBody);

            using var ms = new System.IO.MemoryStream(responseBytes);
            var responseDDTP = new DDTP(ms);

            return responseDDTP;
        }

        public async Task<string> AuthenticationAsync()
        {
            var ss = await GetCookieAsync();

            string username = "namthangbl";
            string password = "ntbl@2501";

            DDTP loginResponse = await LoginAsync(username, password, ss);

            string returnValue = loginResponse.GetString("$Var.Return$");
            Console.WriteLine("Return: " + returnValue);

            var cookies = cookieContainer.GetCookies(new Uri(_url));
            string jsessionid = cookies["JSESSIONID"]?.Value;
            string tokenID = cookies["tokenID"]?.Value; // nếu server trả tokenID
            Console.WriteLine($"JSESSIONID={jsessionid}; tokenID={tokenID}");

            return returnValue; // hoặc return tokenID tùy API cần
        }
    }

}
