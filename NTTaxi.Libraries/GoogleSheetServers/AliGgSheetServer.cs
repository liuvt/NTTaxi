using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using NTTaxi.Libraries.Extensions;
using NTTaxi.Libraries.GoogleSheetServers.Interfaces;
using NTTaxi.Libraries.Models.Alis;
using System.IO;
using System.Net.NetworkInformation;

namespace NTTaxi.Libraries.GoogleSheetServers

{
    public class AliGgSheetServer : IAliGgSheetServer
    {
        #region Constructor 
        //For Connection to Spreads
        private SheetsService sheetsService;
        private readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        private readonly string CredentialGGSheetService = "ggsheetaccount.json";
        private readonly string AppName = "ADMIN ALI";
        private readonly string SpreadSheetId = "1LrZoup8C2JnOmCqB9pzjlU5nKDTy_qgLUEYlKO8moME";

        // For Sheet
        private readonly string sheetAPPKH = "APP KHÁCH HÀNG";
        private readonly string sheetKM = "KHUYẾN MÃI"; 
        private readonly string sheetCancel = "HỦY CUỐC TỔNG ĐÀI/APP";
        private readonly string sheetSwitchboard = "KIẾM SOÁT DỊCH VỤ HOÀN THÀNH";
        private readonly string sheetPartnerGSM = "ĐÔI TÁC GSM TRÊN ALI";


        public AliGgSheetServer()
        {
            //File xác thực google tài khoản
            GoogleCredential credential;
            using (var stream = new FileStream(CredentialGGSheetService, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(Scopes);
            }

            // Đăng ký service
            sheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = AppName,
            });
        }
        #endregion

        #region App Khách Hàng
        //Ghi log vào Google Sheet
        public async Task<bool> AppendOrderAliAsync(List<OrderAli> models)
        {
            try
            {
                //Convert list models to a list of values
                var values = models.Select(model => new List<object> { 
                    model.ID,
                    model.CustomerPhoneNumber,
                    model.CustomerFullName,
                    model.Status,
                    model.Distance,
                    model.DriverPhoneNumber,
                    model.DriveNo,
                    model.Price,
                    model.Location,
                    model.BookingTime,
                    model.Note,
                    DateTime.Now.ToString("dd/MM/yyyy")
                }).ToList<IList<object>>();

                var valueRange = new ValueRange
                {
                    Values = values
                };

                string range = $"{sheetAPPKH}!A2:L";
                await sheetsService.ltvAppendSheetValuesAsync(SpreadSheetId, range, valueRange);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Connection GoogleSheet Success] Đã ghi {models.Count} đơn hàng vào Google Sheet.");
                Console.ResetColor();
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Connection GoogleSheet Error] {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
                return false;
            }
        }

        //Xóa data trong Google Sheet
        public async Task<bool> ClearOrderAliAsync()
        {
            try
            {
                string range = $"{sheetAPPKH}!A2:L";
                await sheetsService.ltvClearSheetValuesAsync(SpreadSheetId, range);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Connection GoogleSheet Success] Đã xóa thành công.");
                Console.ResetColor();
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Connection GoogleSheet Error] {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
                return false;
            }
        }
        #endregion

        #region Khuyến mãi
        //Ghi log vào Google Sheet
        public async Task<bool> AppendPromoteAliAsync(List<PromoteAli> models)
        {
            try
            {
                //Convert list models to a list of values
                var values = models.Select(model => new List<object> {
                    model.ID,
                    model.PartnerCode,
                    model.DriverPhoneNumber,
                    model.Price,
                    model.PromotionPrice,
                    model.ReturnDiscount,
                    model.CustomerPay,
                    model.ExtraFee,
                    model.Discount,
                    model.Revenue,
                    model.DepositRemaining,
                    model.PaymentMethod,
                    model.CreatedAt,
                    DateTime.Now.ToString("dd/MM/yyyy")
                }).ToList<IList<object>>();

                var valueRange = new ValueRange
                {
                    Values = values
                };

                string range = $"{sheetKM}!A2:N";
                await sheetsService.ltvAppendSheetValuesAsync(SpreadSheetId, range, valueRange);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Connection GoogleSheet Success] Đã ghi {models.Count} khuyến mãi vào Google Sheet.");
                Console.ResetColor();
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Connection GoogleSheet Error] {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
                return false;
            }
        }

        //Xóa data trong Google Sheet
        public async Task<bool> ClearPromoteAliAsync()
        {
            try
            {
                string range = $"{sheetKM}!A2:N";
                await sheetsService.ltvClearSheetValuesAsync(SpreadSheetId, range);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Connection GoogleSheet Success] Đã xóa thành công.");
                Console.ResetColor();
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Connection GoogleSheet Error] {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
                return false;
            }
        }
        #endregion

        #region Cancel Order Ali & Switchboard
        //Xóa data trong Google Sheet
        public async Task<bool> ClearCancelOrderAliAsync()
        {
            try
            {
                string range = $"{sheetCancel}!A2:M";
                await sheetsService.ltvClearSheetValuesAsync(SpreadSheetId, range);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Connection GoogleSheet Success] Đã xóa thành công.");
                Console.ResetColor();
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Connection GoogleSheet Error] {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
                return false;
            }
        }

        //Ghi log vào Google Sheet
        public async Task<bool> AppendCancelOrderAliAsync(List<CancelOrder> models)
        {
            try
            {
                //Convert list models to a list of values
                var values = models.Select(model => new List<object> {
                    model.ID,
                    model.CustomerPhoneNumber,
                    model.CustomerFullName,
                    model.Status,
                    model.Distance,
                    model.DriverPhoneNumber,
                    model.DriveNo,
                    model.Price,
                    model.Location,
                    model.BookingTime,
                    model.Note,
                    model.Type,
                    DateTime.Now.ToString("dd/MM/yyyy")
                }).ToList<IList<object>>();

                var valueRange = new ValueRange
                {
                    Values = values
                };

                string range = $"{sheetCancel}!A2:M";
                await sheetsService.ltvAppendSheetValuesAsync(SpreadSheetId, range, valueRange);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Connection GoogleSheet Success] Đã thêm {models.Count} bản ghi vào Google Sheet.");
                Console.ResetColor();
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Connection GoogleSheet Error] {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
                return false;
            }
        }

        #endregion

        #region Switchboard
        //Xóa data trong Google Sheet
        public async Task<bool> ClearSwitchboardAliAsync()
        {
            try
            {
                string range = $"{sheetSwitchboard}!A2:L";
                await sheetsService.ltvClearSheetValuesAsync(SpreadSheetId, range);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Connection GoogleSheet Success] Đã xóa thành công.");
                Console.ResetColor();
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Connection GoogleSheet Error] {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
                return false;
            }
        }

        //Ghi log vào Google Sheet
        public async Task<bool> AppendSwitchboardAliAsync(List<SwitchboardAli> models)
        {
            try
            {
                //Convert list models to a list of values
                var values = models.Select(model => new List<object> {
                    model.ID,
                    model.CustomerPhoneNumber,
                    model.CustomerFullName,
                    model.Status,
                    model.Distance,
                    model.DriverPhoneNumber,
                    model.DriveNo,
                    model.Price,
                    model.Location,
                    model.BookingTime,
                    model.Note,
                    DateTime.Now.ToString("dd/MM/yyyy")
                }).ToList<IList<object>>();

                var valueRange = new ValueRange
                {
                    Values = values
                };

                string range = $"{sheetSwitchboard}!A2:L";
                await sheetsService.ltvAppendSheetValuesAsync(SpreadSheetId, range, valueRange);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Connection GoogleSheet Success] Đã thêm {models.Count} bản ghi vào Google Sheet.");
                Console.ResetColor();
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Connection GoogleSheet Error] {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
                return false;
            }
        }
        #endregion

        #region GSM Partner
        //Xóa data trong Google Sheet
        public async Task<bool> ClearPartnerGSMAliAsync()
        {
            try
            {
                string range = $"{sheetPartnerGSM}!A2:Q";
                await sheetsService.ltvClearSheetValuesAsync(SpreadSheetId, range);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Connection GoogleSheet Success] Đã xóa thành công.");
                Console.ResetColor();
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Connection GoogleSheet Error] {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
                return false;
            }
        }

        //Ghi log vào Google Sheet
        public async Task<bool> AppendPartnerGSMAliAsync(List<PartnerGSM> models)
        {
            try
            {
                //Convert list models to a list of values
                var values = models.Select(model => new List<object> {
                    model.Id_partner,
                    model.Id_ali,
                    model.TripTime,
                    model.CustomerPhoneNumber,
                    model.CustomerFullName,
                    model.Distance,
                    model.Price,
                    model.PaymentMethod,
                    model.PaymentStatus,
                    model.DriverPhoneNumber,
                    model.DriverId,
                    model.DriveNo,
                    model.TripStatus,
                    model.PickupLocation,
                    model.DropoffLocation,
                    model.TypeService,
                    DateTime.Now.ToString("dd/MM/yyyy")
                }).ToList<IList<object>>();

                var valueRange = new ValueRange
                {
                    Values = values
                };

                string range = $"{sheetPartnerGSM}!A2:Q";
                await sheetsService.ltvAppendSheetValuesAsync(SpreadSheetId, range, valueRange);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Connection GoogleSheet Success] Đã thêm {models.Count} bản ghi vào Google Sheet.");
                Console.ResetColor();
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Connection GoogleSheet Error] {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
                return false;
            }
        }
        #endregion
    }
}
