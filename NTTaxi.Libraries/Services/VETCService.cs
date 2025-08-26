using NTTaxi.Libraries.Models.Vetcs;
using NTTaxi.Libraries.Services.Interfaces;
using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Tesseract;

namespace NTTaxi.Libraries.Services
{
    public class VETCService : IVETCService
    {
        private readonly HttpClient httpClient;
        private readonly CookieContainer cookieContainer;

        public VETCService()
        {
            cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                UseCookies = true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

        }

        private async Task<string> GetVerificationTokenAsync()
        {
            var url = "https://customer.vetc.com.vn/Account/Login?ReturnUrl=%2FCorporate%2FSearchmodifyinformation";

            // Gọi GET -> CookieContainer sẽ tự giữ ASP.NET_SessionId
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();

            // Regex lấy __RequestVerificationToken
            var match = Regex.Match(html,
                @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)""");
            if (!match.Success)
                throw new Exception("Notfound: __RequestVerificationToken");

            return match.Groups[1].Value;
        }

        public async Task<string> GetAuthenticationAsync(UserVETC user)
        {
            var loginUrl = "https://customer.vetc.com.vn/Account/Login?ReturnUrl=%2FCorporate%2FSearchmodifyinformation";

            // 1. Lấy token + session
            var token = await GetVerificationTokenAsync();

            // 2. Tạo form
            var formData = new List<KeyValuePair<string, string>>
            {
                new("__RequestVerificationToken", token),
                new("UserName", user.username),
                new("Password", user.password),
                new("Captcha", ""), // hiện tại VETC bỏ Captcha
                new("fingerprint", "4375d0d6a1b979c7811095f4573061f334b7f8dd20a6d7b05e42c66f5de968d0"),
                new("clientInfo", "24||Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/139.0.0.0||Win32||vi-VN||4||true||Asia/Bangkok||-420||WebKit WebGL")
            };

            var content = new FormUrlEncodedContent(formData);

            var response = await httpClient.PostAsync(loginUrl, content);
            var resp2 = await httpClient.GetAsync("https://customer.vetc.com.vn/Corporate/SearchModifyInformation");
            var html2 = await resp2.Content.ReadAsStringAsync();

            if (html2.Contains("Trang chủ") || html2.Contains("Corporate/Home"))
            {
                return "Success login!";
            }
            else
            {
                Console.WriteLine(html2.Substring(0, Math.Min(500, html2.Length))); // in ra để debug
                return "Error login";
            }
        }

        public async Task<string> GetDatas()
        {
            var url = "https://customer.vetc.com.vn/corporate/searchmodifyinformation/gettransporttransunionv";

            // 1. Lấy token + session
            var token = await GetVerificationTokenAsync();

            var payload = new
            {
                draw = 1,
                start = 0,
                length = 100000,
                accountid = 3213242,
                fromdate = "25/08/2025",
                todate = "25/08/2025",
                status = "1",
                check = true,
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // thêm referrer để giống request browser (một số site check)
            httpClient.DefaultRequestHeaders.Referrer = new Uri("https://customer.vetc.com.vn/Corporate/Transaction");

            var response = await httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine(result);

            return result;

        }
    }
}