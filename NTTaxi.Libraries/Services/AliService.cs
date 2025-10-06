using ClosedXML.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;
using NTTaxi.Libraries.GoogleSheetServers.Interfaces;
using NTTaxi.Libraries.Models.Alis;
using NTTaxi.Libraries.Models.Gsms;
using NTTaxi.Libraries.Services.Interfaces;
using System.Collections.Generic;
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
        private readonly string endpoint_Switchboard = "request?";
        private readonly string endpoint_PartnerGSM = "widget-request/widget-request-gsm?";
        
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
                    OrderAlisWithProvince("SÓC TRĂNG", "20", start, end),
                    OrderAlisWithProvince("CẦN THƠ", "5", start, end)
                };

                var results = await Task.WhenAll(tasks);
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
                await aliGgSheetServer.ClearOrderAliAsync(); //Xóa dữ liệu cũ
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
            return (province, orders); //return (Province: province, Orders: orders);
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
        public async Task<List<PromoteAli>> GetsPromoteAli(SchemaJson _json, DateTime start, DateTime end)
        {
            try
            {
                var allPromoteAli = new List<PromoteAli>();
                foreach (var ck in _json.CookieAli)
                    _cookieContainer.Add(_baseUri, new Cookie(ck.key, ck.value));

                // Tạo các task async cho từng tỉnh
                var tasks = new[]
                {
                    PromoteAlisWithProvince("BẠC LIÊU", "64", start, end),
                    PromoteAlisWithProvince("VĨNH LONG", "61", start, end),
                    PromoteAlisWithProvince("CÀ MAU", "63", start, end),
                    PromoteAlisWithProvince("KIÊN GIANG", "15", start, end),
                    PromoteAlisWithProvince("HẬU GIANG", "62", start, end),
                    PromoteAlisWithProvince("AN GIANG", "16", start, end),
                    PromoteAlisWithProvince("SÓC TRĂNG", "20", start, end),
                    PromoteAlisWithProvince("CẦN THƠ", "5", start, end)
                };

                var results = await Task.WhenAll(tasks);
                allPromoteAli = results.SelectMany(r => r.Item2).ToList();
                // Gọi phương thức đăng nhập
                return allPromoteAli;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy dữ liệu khuyến mãi: {ex.Message}");
            }
        }

        public async Task PostPromoteAli(SchemaJson _json, DateTime start, DateTime end)
        {
            try
            {
                await aliGgSheetServer.ClearPromoteAliAsync(); //Xóa dữ liệu cũ
                var promote = await GetsPromoteAli(_json, start, end);
                // Post lên Google Sheet
                await aliGgSheetServer.AppendPromoteAliAsync(promote);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy dữ liệu khuyến mãi: {ex.Message}");
            }
        }

        private async Task<(string, List<PromoteAli>)> PromoteAlisWithProvince(string province, string provincecode, DateTime start, DateTime end)
        {
            var promote = await PromoteAlis(provincecode, start, end);
            return (Province: province, PromoteAli: promote);
        }

        private async Task<List<PromoteAli>> PromoteAlis(string area, DateTime start, DateTime end)
        {
            var response = await _client.GetAsync(_baseUri + endpoint_Promote + PromoteQueryStringParameters(area, start, end));
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

        #region Cancel Orders Ali
        public async Task PostCancelOrderAli(DateTime start, DateTime end)
        {
            try
            {
                await aliGgSheetServer.ClearCancelOrderAliAsync(); //Xóa dữ liệu cũ
                var cancelOrders = await GetsCancelOrderAli(start, end);
                await aliGgSheetServer.AppendCancelOrderAliAsync(cancelOrders);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy dữ liệu đơn hàng: {ex.Message}");
            }
        }

        public async Task<List<CancelOrder>> GetsCancelOrderAli(DateTime start, DateTime end)
        {
            try
            {
                var switchboardTask = GetsCancelSwitchboard(start, end);
                var cancelTask = GetsCancelOrder(start, end);

                var switchboard = await switchboardTask;
                var cancel = await cancelTask;

                return cancel.Concat(switchboard).ToList();
            }
            catch (Exception ex)
            {
                // TODO: log exception để debug
                return new List<CancelOrder>();
            }
        }

        #region Switchboard Order 4,6,8 : App tổng đài không chịu nỗi và cơ chế cache không ổn định nên chạy tuần tự
        private async Task<List<CancelOrder>> GetsCancelSwitchboard(DateTime start, DateTime end)
        {
            try
            {
                var allCancelOrders = new List<CancelOrder>();

                var provinces = new (string Name, string Code)[]
                {
                    ("BẠC LIÊU", "64"),
                    ("VĨNH LONG", "61"),
                    ("CÀ MAU", "63"),
                    ("KIÊN GIANG", "15"),
                    ("HẬU GIANG", "62"),
                    ("AN GIANG", "16"),
                    ("SÓC TRĂNG", "20"),
                    ("CẦN THƠ", "5")
                };

                foreach (var (name, code) in provinces)
                {
                    var cancelOrders = await CancelSwitchboard(code, start, end);
                    allCancelOrders.AddRange(cancelOrders);
                    await Task.Delay(500); // cho server thở một chút, tránh spam
                }

                return allCancelOrders;
            }
            catch (Exception ex)
            {
                return new List<CancelOrder>();
            }
        }
        private async Task<List<CancelOrder>> CancelSwitchboard(string area, DateTime start, DateTime end)
        {
            using var client = CreateHttpClient(JsonSerializer.Deserialize<SchemaJson>(await File.ReadAllTextAsync("AliAuthentication.json")));
            var response = await client.GetAsync(_baseUri + endpoint_Switchboard + CancelSwitchboardQueryStringParameters(area, start, end));
            //Console.WriteLine($"Data: {response}");
            var cancelSwitchboards = new List<CancelOrder>();

            if (!response.IsSuccessStatusCode)
                return cancelSwitchboards;
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

                var driveNo = row.Cell(8).GetString();
                if (string.IsNullOrWhiteSpace(driveNo))
                {
                    continue;
                }

                var cancelSwitchboard = new CancelOrder
                {
                    ID = row.Cell(2).GetString(),
                    CustomerPhoneNumber = row.Cell(3).GetString(),
                    CustomerFullName = row.Cell(4).GetString(),
                    Status = row.Cell(5).GetString(),
                    Distance = row.Cell(6).GetString().Replace(".", ","),
                    DriverPhoneNumber = row.Cell(7).GetString(),
                    DriveNo = driveNo.ToUpper(),
                    Price = row.Cell(9).GetString(),
                    Location = row.Cell(10).GetString(),
                    BookingTime = row.Cell(11).GetString(),
                    Note = row.Cell(12).GetString(),
                    Type = "TỔNG ĐÀI" // Thêm loại đơn hàng là Tổng đài
                };

                cancelSwitchboards.Add(cancelSwitchboard);
            }

            return cancelSwitchboards; //Chỉ lấy những đơn hàng hủy có mã tài
        }
        private string CancelSwitchboardQueryStringParameters(string area, DateTime startTime, DateTime endTime)
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
            query["taxi_type"] = "0";
            query["team_id"] = "0";
            query["from-date-request"] = startTime.ToString("dd-MM-yyyy HH:mm");
            query["to-date-request"] = endTime.ToString("dd-MM-yyyy HH:mm");
            query["driver_online"] = "0";
            query["status_statistic"] = "1";
            query["export_excel"] = "1";

            var list = query.AllKeys
                       .Where(k => k != null)
                       .Select(k => $"{HttpUtility.UrlEncode(k)}={HttpUtility.UrlEncode(query[k])}")
                       .ToList();
            //add nhiều request_status[]
            AddArrayParam(list, "request_status[]", "4", "6", "8");

            return string.Join("&", list);
        }
        #endregion

        #region App Customer Order 4,6,8 
        private async Task<List<CancelOrder>> GetsCancelOrder(DateTime start, DateTime end)
        {
            try
            {
                var allCancelOrders = new List<CancelOrder>();

                var provinces = new (string Name, string Code)[]
                {
                    ("BẠC LIÊU", "64"),
                    ("VĨNH LONG", "61"),
                    ("CÀ MAU", "63"),
                    ("KIÊN GIANG", "15"),
                    ("HẬU GIANG", "62"),
                    ("AN GIANG", "16"),
                    ("SÓC TRĂNG", "20"),
                    ("CẦN THƠ", "5")
                };

                foreach (var (name, code) in provinces)
                {
                    var cancelOrders = await CancelOrderAlis(code, start, end);
                    allCancelOrders.AddRange(cancelOrders);
                    await Task.Delay(500); // cho server thở một chút, tránh spam
                }

                return allCancelOrders;
            }
            catch (Exception ex)
            {
                return new List<CancelOrder>();
            }
        }
        private async Task<List<CancelOrder>> CancelOrderAlis(string area, DateTime start, DateTime end)
        {
            using var client = CreateHttpClient(JsonSerializer.Deserialize<SchemaJson>(await File.ReadAllTextAsync("AliAuthentication.json")));
            var response = await client.GetAsync(_baseUri + endpoint_Order + CancelOrderQueryStringParameters(area, start, end));

            logger.LogInformation($"Đang lấy đơn hàng hủy từ khu vực {area} từ {start} đến {end}");
            var cancelOrders = new List<CancelOrder>();
            if (!response.IsSuccessStatusCode)
                return cancelOrders;

            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            //await  File.WriteAllBytesAsync($"download-{area}.xlsx", fileBytes);
            using var ms = new MemoryStream(fileBytes);
            using var workbook = new XLWorkbook(ms);
            var ws = workbook.Worksheet(1);

            //Ingore header row
            foreach (var row in ws.RowsUsed().Skip(1))
            {
                //Check null or empty row
                if (row.Cells().All(c => string.IsNullOrWhiteSpace(c.GetString())))
                    break;

                var driveNo = row.Cell(8).GetString();
                if (string.IsNullOrWhiteSpace(driveNo))
                {
                    continue;
                }

                var cancelOrder = new CancelOrder
                {
                    ID = row.Cell(2).GetString(),
                    CustomerPhoneNumber = row.Cell(3).GetString(),
                    CustomerFullName = row.Cell(4).GetString(),
                    Status = row.Cell(5).GetString(),
                    Distance = row.Cell(6).GetString().Replace(".", ","),
                    DriverPhoneNumber = row.Cell(7).GetString(),
                    DriveNo = driveNo.ToUpper(),
                    Price = row.Cell(9).GetString(),
                    Location = row.Cell(10).GetString(),
                    BookingTime = row.Cell(11).GetString(),
                    Note = row.Cell(12).GetString(),
                    Type = "APP KHÁCH HÀNG" // Thêm loại đơn hàng là App Khách hàng
                };
                cancelOrders.Add(cancelOrder);
            }
            logger.LogInformation($"Lấy được {cancelOrders.Count} đơn hàng hủy từ khu vực {area}");
            return cancelOrders; 
        }
        private string CancelOrderQueryStringParameters(string area, DateTime startTime, DateTime endTime)
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
            //query["widget_id"] = "48";//App khách hàng (Loại bỏ lấy toàn bộ)
            query["taxi_type"] = "0";
            query["team_id"] = "0";
            query["from-date-request"] = startTime.ToString("dd-MM-yyyy HH:mm");
            query["to-date-request"] = endTime.ToString("dd-MM-yyyy HH:mm");
            query["driver_online"] = "0";
            query["status_statistic"] = "1";
            query["export_excel"] = "1";

            var list = query.AllKeys
                       .Where(k => k != null)
                       .Select(k => $"{HttpUtility.UrlEncode(k)}={HttpUtility.UrlEncode(query[k])}")
                       .ToList();
            //add nhiều request_status[]
            AddArrayParam(list, "request_status[]", "4", "6", "8");

            return string.Join("&", list);
        }
        #endregion

        //Đối với tham số "request_status[]" đang trùng nhau. Nên tạo hàm phụ để có thể select nhiều giá trị : Hủy do tài xế, Hủy do khách hàng, Hủy do tổng đài
        private void AddArrayParam(List<string> list, string key, params string[] values)
        {
            foreach (var val in values)
            {
                list.Add($"{key}={HttpUtility.UrlEncode(val)}");
            }
        }
        private HttpClient CreateHttpClient(SchemaJson _json)
        {
            var handler = new HttpClientHandler { CookieContainer = new CookieContainer() };
            foreach (var ck in _json.CookieAli)
                handler.CookieContainer.Add(_baseUri, new Cookie(ck.key, ck.value));

            return new HttpClient(handler);
        }
        #endregion

        #region Switchboard Order
        public async Task PostSwitchboardAli(DateTime start, DateTime end)
        {
            try
            {
                await aliGgSheetServer.ClearSwitchboardAliAsync(); //Xóa dữ liệu cũ
                var switchboards = await GetsSwitchboardAli(start, end);
                await aliGgSheetServer.AppendSwitchboardAliAsync(switchboards);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy dữ liệu đơn hàng: {ex.Message}");
            }
        }
        public async Task<List<SwitchboardAli>> GetsSwitchboardAli(DateTime start, DateTime end)
        {
            try
            {
                var switchBoards = new List<SwitchboardAli>();

                var provinces = new (string Name, string Code)[]
                {
                    ("BẠC LIÊU", "64"),
                    ("VĨNH LONG", "61"),
                    ("CÀ MAU", "63"),
                    ("KIÊN GIANG", "15"),
                    ("HẬU GIANG", "62"),
                    ("AN GIANG", "16"),
                    ("SÓC TRĂNG", "20"),
                    ("CẦN THƠ", "5")
                };

                foreach (var (name, code) in provinces)
                {
                    var switchBoard = await Switchboard(code, start, end);
                    switchBoards.AddRange(switchBoard);
                    await Task.Delay(500); // cho server thở một chút, tránh spam
                }

                return switchBoards;
            }
            catch (Exception ex)
            {
                return new List<SwitchboardAli>();
            }
        }

        private async Task<List<SwitchboardAli>> Switchboard(string area, DateTime start, DateTime end)
        {
            using var client = CreateHttpClient(JsonSerializer.Deserialize<SchemaJson>(await File.ReadAllTextAsync("AliAuthentication.json")));
            var response = await client.GetAsync(_baseUri + endpoint_Switchboard + SwitchboardQueryStringParameters(area, start, end));
            //Console.WriteLine($"Data: {response}");
            var switchboards = new List<SwitchboardAli>();

            if (!response.IsSuccessStatusCode)
                return switchboards;
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

                var driveNo = row.Cell(8).GetString();
                if (string.IsNullOrWhiteSpace(driveNo))
                {
                    continue;
                }

                var switchboard = new SwitchboardAli
                {
                    ID = row.Cell(2).GetString(),
                    CustomerPhoneNumber = row.Cell(3).GetString(),
                    CustomerFullName = row.Cell(4).GetString(),
                    Status = row.Cell(5).GetString(),
                    Distance = row.Cell(6).GetString().Replace(".", ","),
                    DriverPhoneNumber = row.Cell(7).GetString(),
                    DriveNo = driveNo.ToUpper(),
                    Price = row.Cell(9).GetString(),
                    Location = row.Cell(10).GetString(),
                    BookingTime = row.Cell(11).GetString(),
                    Note = row.Cell(12).GetString(),
                };

                switchboards.Add(switchboard);
            }

            return switchboards; //Chỉ lấy những đơn hàng hủy có mã tài
        }

        private string SwitchboardQueryStringParameters(string area, DateTime startTime, DateTime endTime)
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
            query["request_status[]"] = "3";
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

        #region Partner GSM Lấy tất cả nên không cần gọi từ khu vực
        public async Task PostPartnerGSMAli(DateTime start, DateTime end)
        {
            try
            {
                await aliGgSheetServer.ClearPartnerGSMAliAsync(); //Xóa dữ liệu cũ
                var partners = await GetsPartnerGSMAli(start, end);
                await aliGgSheetServer.AppendPartnerGSMAliAsync(partners);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy dữ liệu đơn hàng: {ex.Message}");
            }
        }
        public async Task<List<PartnerGSM>> GetsPartnerGSMAli(DateTime start, DateTime end)
        {
            try
            {
                return await PartnerGSMAli(start, end);
            }
            catch (Exception ex)
            {
                return new List<PartnerGSM>();
            }
        }

        private async Task<List<PartnerGSM>> PartnerGSMAli(DateTime start, DateTime end)
        {
            using var client = CreateHttpClient(JsonSerializer.Deserialize<SchemaJson>(await File.ReadAllTextAsync("AliAuthentication.json")));
            var response = await client.GetAsync(_baseUri + endpoint_PartnerGSM + PartnerGSMQueryStringParameters(start, end));
            //Console.WriteLine($"Data: {response}");
            var partners = new List<PartnerGSM>();

            if (!response.IsSuccessStatusCode)
                return partners;
            var fileBytes = await response.Content.ReadAsByteArrayAsync();

            using var ms = new MemoryStream(fileBytes);
            using var workbook = new XLWorkbook(ms);
            var ws = workbook.Worksheet(1);

            //Ingore header row
            //Header có merge nên bỏ 4 dòng đầu bị mất 2 dòng dữ liệu
            foreach (var row in ws.RowsUsed().Skip(2))
            {
                //Check null or empty row
                if (row.Cells().All(c => string.IsNullOrWhiteSpace(c.GetString())))
                    break;

                var partner = new PartnerGSM
                {
                    Id_partner = row.Cell(2).GetString(),
                    Id_ali = row.Cell(3).GetString(),
                    TripTime = row.Cell(4).GetString(),
                    CustomerPhoneNumber = row.Cell(5).GetString(),
                    CustomerFullName = row.Cell(6).GetString(),
                    Distance = row.Cell(7).GetString().Replace(".", ","),
                    Price = row.Cell(8).GetString().Replace(",", ""),
                    PaymentMethod = row.Cell(9).GetString(),
                    PaymentStatus = row.Cell(10).GetString(),
                    DriverPhoneNumber = row.Cell(11).GetString(),
                    DriverId = row.Cell(12).GetString(),
                    DriveNo = row.Cell(13).GetString(),
                    TripStatus = row.Cell(14).GetString(),
                    PickupLocation = row.Cell(15).GetString(),
                    DropoffLocation = row.Cell(16).GetString(),
                    TypeService = row.Cell(17).GetString(),
                };

                partners.Add(partner);
            }

            return partners; 
        }

        private string PartnerGSMQueryStringParameters(DateTime startTime, DateTime endTime)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["partner_order_id"] = "";
            query["request_id"] = "";
            query["phone_client"] = "";
            query["phone_driver"] = "";
            query["province_id"] = "0";
            query["payment_method_id"] = "0";
            query["from-date-request"] = startTime.ToString("dd-MM-yyyy HH:mm");
            query["to-date-request"] = endTime.ToString("dd-MM-yyyy HH:mm");
            query["status"] = "3";
            query["payment_status"] = "-1";
            query["export_excel"] = "1";

            return query.ToString();
        }
        #endregion
    }
}

