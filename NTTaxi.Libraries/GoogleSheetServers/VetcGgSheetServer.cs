using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using NTTaxi.Libraries.Extensions;
using NTTaxi.Libraries.GoogleSheetServers.Interfaces;
using NTTaxi.Libraries.Models.Vetcs;

namespace NTTaxi.Libraries.GoogleSheetServers

{
    public class VetcGgSheetServer : IVetcGgSheetServer
    {
        #region Constructor 
        //For Connection to Spreads
        private SheetsService sheetsService;
        private readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        private readonly string CredentialGGSheetService = "ggsheetaccount.json";
        private readonly string AppName = "ADMIN VETC";
        private readonly string SpreadSheetId = "1NlFKTU6Rqe5F1AJqzv_nML1-Lve7ScBLD9QuvR9YuUA";

        // For Sheet
        private readonly string sheetBLU = "BLU";


        public VetcGgSheetServer()
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
        public async Task<bool> AppendVetcAsync(List<VetcItem> models, string provinceCode)
        {
            try
            {
                //Convert list models to a list of values
                var values = models.Select(model => new List<object> { 
                    model.TransportTransId,
                    model.Plate,
                    model.CheckInName,
                    model.Amount?.ToString()!,
                    model.CheckerOutDateTime?.ToString("dd/MM/yyyy HH:mm:ss")!,
                    model.Pass,
                    model.PriceTicketType,
                    "",
                    DateTime.Now.ToString("dd/MM/yyyy")
                }).ToList<IList<object>>();

                var valueRange = new ValueRange
                {
                    Values = values
                };

                string range = $"{provinceCode}!A2:I";
                await sheetsService.ltvAppendSheetValuesAsync(SpreadSheetId, range, valueRange);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Connection GoogleSheet Success] Đã ghi {models.Count} vào Google Sheet.");
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
        public async Task<bool> ClearVetcAsync(string provinceCode)
        {
            try
            {
                string range = $"{provinceCode}!A2:I";
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
