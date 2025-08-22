using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using NTTaxi.Libraries.GoogleSheetServers.Interfaces;
using NTTaxi.Libraries.Models.Alis;
using NTTaxi.Libraries.Services.Interfaces;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;

namespace NTTaxi.Libraries.Services
{
    public class AliService : IAliService
    {
        private readonly HttpClient _client;
        private readonly CookieContainer _cookieContainer;
        private readonly string endpoint_Login = "account/login";
        private readonly string endpoint_Order = "request/widget?";
        private readonly string endpoint_Promote = "transaction/money?";
        private readonly Uri _baseUri = new Uri("https://adminphuquoc2.dieuhanhtaxi.vn/");
        private readonly IAliGgSheetServer aliGgSheetServer;
        private readonly ILogger<AliService> logger;

        // Constructor không cần tham số
        public AliService(IAliGgSheetServer _aliGgSheetServer, ILogger<AliService> _logger)
        {
            this.aliGgSheetServer = _aliGgSheetServer;
            this.logger = _logger;
            _cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler
            {
                CookieContainer = _cookieContainer,
                AllowAutoRedirect = true
            };

            _client = new HttpClient(handler)
            {
                BaseAddress = _baseUri
            };
        }
        #region Authentication
        public async Task<bool> GetAuthenticationAsync(UserAli user)
        {
            try
            {
                //Kiểu dữ liệu Form KEY/VALUE
                var postData = new FormUrlEncodedContent(
                    new[] {
                        new KeyValuePair<string, string>(nameof(user.username), user.username),
                        new KeyValuePair<string, string>(nameof(user.password), user.password)
                    });

                var response = await _client.PostAsync(_baseUri + endpoint_Login, postData);
                var body = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var cookies = _cookieContainer.GetCookies(_baseUri);
                    List<CookieAli> cookieList = cookies.Cast<Cookie>()
                                            .Select(c => new CookieAli
                                            {
                                                key = c.Name,
                                                value = c.Value
                                            })
                                            .ToList();
                    // Lưu cookie vào SchemaJson
                    var schemaJson = new SchemaJson
                    {
                        User = user,
                        CookieAli = cookieList
                    };

                    //Format JSON
                    var option = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };

                    string json = System.Text.Json.JsonSerializer.Serialize(schemaJson, option);
                    // Xuất ra file
                    File.WriteAllText("AliAuthentication.json", json, new UTF8Encoding(false));
                    return true;
                }
                else
                {
                    throw new Exception($"Tài khoản không đúng!");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Chi tiết: {ex.Message}");
            }
        }
        #endregion

        #region Orders Ali
        public async Task<List<OrderAli>> GetsOrderAli(SchemaJson _json, DateTime start, DateTime end)
        {
            try
            {
                var allOrders = new List<OrderAli>();
                foreach (var ck in _json.CookieAli)
                    _cookieContainer.Add(_baseUri, new Cookie(ck.key, ck.value));

                // Tạo các task async cho từng tỉnh
                var tasks = new[]
                {
                    OrderAlisWithProvince("BẠC LIÊU", "64", start, end),
                    OrderAlisWithProvince("VĨNH LONG", "61", start, end),
                    OrderAlisWithProvince("CÀ MAU", "63", start, end),
                    OrderAlisWithProvince("KIÊN GIANG", "15", start, end),
                    OrderAlisWithProvince("HẬU GIANG", "62", start, end),
                    OrderAlisWithProvince("AN GIANG", "16", start, end),
                    OrderAlisWithProvince("SÓC TRĂNG", "20", start, end)
                };

                        var results = await Task.WhenAll(tasks);

                var ordersByProvince = results.ToDictionary(r => r.Item1, r => r.Item2);
                allOrders = results.SelectMany(r => r.Item2).ToList();
                return allOrders;
            }
            catch (Exception ex)
            {
                return new List<OrderAli>();
            }
        }
        public async Task PostOrderAli(SchemaJson _json, DateTime start, DateTime end)
        {
            try
            {
                var orders = await GetsOrderAli(_json, start, end);
                // Post lên Google Sheet
                await aliGgSheetServer.AppendOrderAliAsync(orders);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy dữ liệu đơn hàng: {ex.Message}");
            }
        }

        // Helper chạy 1 tỉnh
        private async Task<(string, List<OrderAli>)> OrderAlisWithProvince(string province, string provincecode, DateTime start, DateTime end)
        {
            var orders = await OrderAlis(provincecode, start, end);
            return (province, orders);
        }


        private async Task<List<OrderAli>> OrderAlis(string area, DateTime start, DateTime end)
        {
            var response = await _client.GetAsync(_baseUri + endpoint_Order + OrderQueryStringParameters(area, start, end));
            //Console.WriteLine($"Data: {response}");
            var orders = new List<OrderAli>();

            if (!response.IsSuccessStatusCode)
                return orders;

            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            using var ms = new MemoryStream(fileBytes);
            using var workbook = new XLWorkbook(ms);
            var ws = workbook.Worksheet(1);

            //Ingore header row
            foreach (var row in ws.RowsUsed().Skip(1))
            {
                //Check null or empty row
                if (row.Cells().All(c => string.IsNullOrWhiteSpace(c.GetString())))
                    break;

                var order = new OrderAli
                {
                    ID = row.Cell(2).GetString(),
                    CustomerPhoneNumber = row.Cell(3).GetString(),
                    CustomerFullName = row.Cell(4).GetString(),
                    Status = row.Cell(5).GetString(),
                    Distance = row.Cell(6).GetString().Replace(".", ","),
                    DriverPhoneNumber = row.Cell(7).GetString(),
                    DriveNo = row.Cell(8).GetString(),
                    Price = row.Cell(9).GetString(),
                    Location = row.Cell(10).GetString(),
                    BookingTime = row.Cell(11).GetString(),
                    Note = row.Cell(12).GetString()
                };
                orders.Add(order);
            }

            return orders;
        }

        private string OrderQueryStringParameters(string area, DateTime startTime, DateTime endTime)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["province"] = area;
            query["phone_client"] = "";
            query["phone_driver"] = "";
            query["request_id"] = "";
            query["content"] = "";
            query["taxi_code"] = "";
            query["distance"] = "0";
            query["agency_id"] = "0";
            query["time_finish_request"] = "0";
            query["agency_handle"] = "-1";
            query["widget_id"] = "48";
            query["request_status[]"] = "3"; // sẽ encode thành %5B%5D
            query["taxi_type"] = "0";
            query["team_id"] = "0";
            query["from-date-request"] = startTime.ToString("dd-MM-yyyy HH:mm");
            query["to-date-request"] = endTime.ToString("dd-MM-yyyy HH:mm");
            query["driver_online"] = "0";
            query["status_statistic"] = "1";
            query["export_excel"] = "1";

            return query.ToString();
        }
        #endregion

        #region Promotes Ali
        public async Task<bool> GetsPromoteAli()
        {
            try
            {
                logger.LogInformation("Đang lấy dữ liệu khuyến mãi...");
                if (File.Exists("AliAuthentication.json"))
                {
                    var schemaJson = JsonSerializer.Deserialize<SchemaJson>(File.ReadAllText("AliAuthentication.json"));
                    foreach (var ck in schemaJson.CookieAli)
                    {
                        _cookieContainer.Add(_baseUri, new Cookie(ck.key, ck.value));
                    }

                    //Create areas data
                    var promotesByProvince = new Dictionary<string, List<PromoteAli>>
                    {
                        { "BẠC LIÊU", await PromoteAlis("64") },
                        { "VĨNH LONG", await PromoteAlis("61") },
                        { "CÀ MAU", await PromoteAlis("63") },
                        { "KIÊN GIANG", await PromoteAlis("15") },
                        { "HẬU GIANG", await PromoteAlis("62") },
                        { "AN GIANG", await PromoteAlis("16") },
                        { "SÓC TRĂNG", await PromoteAlis("20") }
                    };

                    foreach (var kvp in promotesByProvince)
                    {
                        string province = kvp.Key;
                        var promotes = kvp.Value;

                        logger.LogInformation($"     === {province.ToUpper()} ===");
                        foreach (var promote in promotes)
                        {
                            logger.LogInformation($"ID: {promote.ID}, " +
                                              $"Số Tài: {promote.PartnerCode}, " +
                                              $"Tiền khuyến mãi: {promote.PromotionPrice}, " +
                                              $"Thời gian tạo: {promote.CreatedAt}");
                        }
                        logger.LogInformation($"Records: ({promotes.Count})");
                    }

                    // Post to Google Sheet
                    var allPromotes = promotesByProvince.SelectMany(kvp => kvp.Value).ToList();
                    await aliGgSheetServer.AppendPromoteAliAsync(allPromotes);
                }
                // Gọi phương thức đăng nhập
                return true;
            }
            catch (Exception ex)
            {
                logger.LogInformation($"Lỗi khi lấy dữ liệu đơn hàng: {ex.Message}");
                return false;
            }
        }

        private async Task<List<PromoteAli>> PromoteAlis(string area)
        {
            var response = await _client.GetAsync(_baseUri + endpoint_Promote + PromoteQueryStringParameters(area, DateTime.Now.AddDays(-1).Date.AddHours(5), DateTime.Now.Date.AddHours(5)));
            //Console.WriteLine($"Data: {response}");
            var promotes = new List<PromoteAli>();

            if (!response.IsSuccessStatusCode)
                return promotes;

            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            using var ms = new MemoryStream(fileBytes);
            using var workbook = new XLWorkbook(ms);
            var ws = workbook.Worksheet(1);

            //Ingore header row
            foreach (var row in ws.RowsUsed().Skip(1))
            {
                //Check null or empty row
                if (row.Cells().All(c => string.IsNullOrWhiteSpace(c.GetString())))
                    break;

                var promote = new PromoteAli
                {
                    ID = row.Cell(2).GetString(),
                    PartnerCode = row.Cell(3).GetString(),
                    DriverPhoneNumber = row.Cell(4).GetString(),
                    Price = row.Cell(5).GetString(),
                    PromotionPrice = row.Cell(6).GetString(),
                    ReturnDiscount = row.Cell(7).GetString(),
                    CustomerPay = row.Cell(8).GetString(),
                    ExtraFee = row.Cell(9).GetString(),
                    Discount = row.Cell(10).GetString(),
                    Revenue = row.Cell(11).GetString(),
                    DepositRemaining = row.Cell(12).GetString(),
                    PaymentMethod = row.Cell(13).GetString(),
                    CreatedAt = row.Cell(14).GetString()
                };
                promotes.Add(promote);
            }

            return promotes;
        }

        private string PromoteQueryStringParameters(string area, DateTime startTime, DateTime endTime)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["province"] = area;
            query["driver_code"] = "";
            query["phone"] = "";
            query["request_id"] = "";
            query["payment_method_id"] = "0";
            query["payment_status"] = "1";
            query["team_id"] = "0";
            query["from-date"] = startTime.ToString("dd-MM-yyyy HH:mm");
            query["to-date"] = endTime.ToString("dd-MM-yyyy HH:mm");
            query["promote_status"] = "1";
            query["act"] = "2";

            return query.ToString();
        }
        #endregion
    }
}

