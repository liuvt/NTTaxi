using NTTaxi.Libraries.Extensions;
using NTTaxi.Libraries.Models.Alis;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using NTTaxi.Libraries.GoogleSheetServers.Interfaces;

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
        private readonly string SpreadSheetId = "1rMB8_QIkPG_rJWF_4sph9YPjTqNQ7XIWfzkGWzWVM1k";

        // For Sheet
        private readonly string sheetAPPKH = "APP KHÁCH HÀNG";
        private readonly string sheetKM = "KHUYẾN MÃI";

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

                string range = $"{sheetAPPKH}!A2:K";
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
                string range = $"{sheetAPPKH}!A2:K";
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
    }
}
