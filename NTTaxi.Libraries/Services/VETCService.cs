using Microsoft.Extensions.Logging;
using NTTaxi.Libraries.Extensions;
using NTTaxi.Libraries.GoogleSheetServers;
using NTTaxi.Libraries.GoogleSheetServers.Interfaces;
using NTTaxi.Libraries.Models.Vetcs;
using NTTaxi.Libraries.Services.Interfaces;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace NTTaxi.Libraries.Services
{
    public class JavaLauncherService : IJavaLauncherService
    {
        private readonly HttpClient httpClient;
        private readonly CookieContainer cookieContainer;
        private readonly Uri _baseUri = new Uri("https://customer.vetc.com.vn/");
        private readonly string endpoint_Login = "Account/Login?ReturnUrl=Corporate/SearchModifyInformation"; //Trỏ về trang Corporate/SearchModifyInformation
        private readonly string endpoint_SearchModifyInformation = "Corporate/SearchModifyInformation";
        private readonly string endpoint_Gettransporttransunion = "corporate/searchmodifyinformation/gettransporttransunionv";
        private string _verificationToken;

        private readonly IVetcGgSheetServer vetcGgSheetServer;
        private readonly ILogger<JavaLauncherService> logger;
        public JavaLauncherService(IVetcGgSheetServer _vetcGgSheetServer, ILogger<JavaLauncherService> _logger)
        {
            this.vetcGgSheetServer = _vetcGgSheetServer;
            this.logger = _logger;
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

        //Đăng nhập
        public async Task<bool> GetAuthenticationAsync(UserVetc user, string provinceCode)
        {
            // Truy cập lấy session and verification token lần đầu
            var token = await GetVerificationTokenAsync();

            if(user == null)
            {
                throw new Exception("Không tìm thấy User!");
            }
            // tạo form data gửi qua body
            var formData = new List<KeyValuePair<string, string>>
            {
                new("__RequestVerificationToken", token),
                new("UserName", user.username),
                new("Password", user.password),
            };

            var content = new FormUrlEncodedContent(formData);

            var response = await httpClient.PostAsync(_baseUri + endpoint_Login, content);

            // Đảm bảo phản hồi thành công sau login
            var resp2 = await httpClient.GetAsync(_baseUri + endpoint_SearchModifyInformation);
            var html2 = await resp2.Content.ReadAsStringAsync();

            if (html2.Contains("Trang chủ") || html2.Contains("Corporate/Home"))
            {
                // Load file cũ nếu có
                RootObjectVetc objToSave = new RootObjectVetc();
                string filePath = "VetcAuthentication.json";

                if (File.Exists(filePath))
                {
                    try
                    {
                        string oldJson = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                        objToSave = System.Text.Json.JsonSerializer.Deserialize<RootObjectVetc>(oldJson)
                                    ?? new RootObjectVetc();
                    }
                    catch
                    {
                        objToSave = new RootObjectVetc();
                    }
                }
                // Cập nhật province tương ứng
                SchemaJsonVetc schemaJson;
                switch (provinceCode)
                {
                    case "BLU":
                        schemaJson = objToSave.BLU ?? new SchemaJsonVetc();
                        schemaJson.User = user;
                        objToSave.BLU = schemaJson;
                        break;
                    case "VLG":
                        schemaJson = objToSave.VLG ?? new SchemaJsonVetc();
                        schemaJson.User = user;
                        objToSave.VLG = schemaJson;
                        break;
                    case "STG":
                        schemaJson = objToSave.STG ?? new SchemaJsonVetc();
                        schemaJson.User = user;
                        objToSave.STG = schemaJson;
                        break;
                    default:
                        throw new Exception($"ProvinceCode {provinceCode} chưa hỗ trợ!");
                }

                // Ghi lại file
                var option = new System.Text.Json.JsonSerializerOptions {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                string json = System.Text.Json.JsonSerializer.Serialize(objToSave, option);
                await File.WriteAllTextAsync(filePath, json, new UTF8Encoding(false));

                return true;
            }
            else
            {
                throw new Exception("Đăng nhập không thành công, vui lòng kiểm tra lại tài khoản");
            }
        }

        public async Task<List<VetcItem>> PostVetcAsync(GetsPayload payload, string provinceCode)
        {
            try
            {
                var data = await GetsVetcAsync(payload);
                if (data != null && data.Count > 0)
                {
                    // Xóa dữ liệu trước khi add vào Google Sheets
                    await vetcGgSheetServer.ClearVetcAsync(provinceCode);
                    // Ghi dữ liệu vào Google Sheets
                    var result = await vetcGgSheetServer.AppendVetcAsync(data, provinceCode);
                    return data;
                }
                else
                {
                    return new List<VetcItem>();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy hoặc ghi dữ liệu VETC BLU: " + ex.Message);
            }
        }

        public async Task<List<VetcItem>> GetsVetcAsync(GetsPayload payload)
        {
            // Gọi lại để lấy token sau khi đăng nhập
            var token = _verificationToken;
            if (string.IsNullOrEmpty(_verificationToken))
                throw new InvalidOperationException("Vui lòng đăng nhập lại");

            var _payload = new
            {
                draw = 1,
                start = 0,
                length = 100000,
                accountid = payload.accountid,
                fromdate = payload.fromdate,
                todate = payload.toDate,
                status = "1",
                check = true,
            };

            var json = JsonSerializer.Serialize(_payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            //Add referrer để giống request browser (một số site check)
            httpClient.DefaultRequestHeaders.Referrer = new Uri(_baseUri + endpoint_SearchModifyInformation);

            var response = await httpClient.PostAsync(_baseUri + endpoint_Gettransporttransunion, content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var vetcResponse = JsonSerializer.Deserialize<VetcResponse>(result, options);
            var resultList = vetcResponse?.data ?? new List<VetcItemResponse>();

            var formatResutList = resultList
                .Select(item => new VetcItem
                {
                    TransportTransId = item.TransportTransId ?? string.Empty,
                    Plate = item.Plate?.vltVetcNormalizePlate() ?? string.Empty,
                    CheckInName = item.CheckInName ?? string.Empty,
                    Amount = item.Amount.ltvVNDCurrencyToDecimal(),
                    CheckerOutDateTime = item.CheckerOutDateTime?.vltVetcDateTime(),
                    Pass = item.Pass ?? string.Empty,
                    PriceTicketType = item.PriceTicketType ?? string.Empty
                }).OrderByDescending(x => x.CheckerOutDateTime) // hoặc OrderBy tùy ý
            .ToList();

            return formatResutList;
        }

        //Truy cập lấy session and verification token
        private async Task<string> GetVerificationTokenAsync()
        {
            ClearCookies();
            // Gọi GET -> CookieContainer sẽ tự giữ ASP.NET_SessionId
            var response = await httpClient.GetAsync(_baseUri + endpoint_Login);
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();

            // Regex lấy __RequestVerificationToken
            var match = Regex.Match(html,
                @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)""");
            if (!match.Success)
                throw new Exception("Notfound: __RequestVerificationToken");

            _verificationToken = match.Groups[1].Value;
            return match.Groups[1].Value;
        }
        private void ClearCookies()
        {
            cookieContainer?.GetCookies(_baseUri)?.Cast<Cookie>().ToList().ForEach(c =>
            {
                c.Expired = true;
            });
        }
    }
}