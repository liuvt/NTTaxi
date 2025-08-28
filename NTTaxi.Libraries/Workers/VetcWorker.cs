using DocumentFormat.OpenXml.Spreadsheet;
using NTTaxi.Libraries.Models.Vetcs;
using NTTaxi.Libraries.Services.Interfaces;
using NTTaxi.Libraries.Workers.Interfaces;
using System.Text.Json;

namespace NTTaxi.Libraries.Workers
{
    public class VetcWorker : IVetcWorker
    {
        private readonly IJavaLauncherService _vetcService;
        private CancellationTokenSource? _cts;

        public event Action? StatusChanged; // Sự kiện trạng thái thay đổi (đang chạy/dừng)
        public bool IsRunning => _cts != null;
        public VetcWorker(IJavaLauncherService vetcService)
        {
            _vetcService = vetcService;
        }

        public void Start()
        {
            if (_cts != null) return;
            _cts = new CancellationTokenSource();
            _ = Task.Run(() => RunAsync(_cts.Token));
            StatusChanged?.Invoke();
        }

        public void Stop()
        {
            _cts?.Cancel();
            _cts = null;
            StatusChanged?.Invoke();
        }

        private async Task RunAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    var json = await LoadSchemaJson();

                    var BLU = now.Date.AddHours(3).AddMinutes(00);
                    var VLG = now.Date.AddHours(3).AddMinutes(05);
                    var STG = now.Date.AddHours(3).AddMinutes(10);

                    /* var BLU = now.AddSeconds(5);
                     var VLG = now.AddSeconds(5);
                     var STG = now.AddSeconds(5);*/

                    // Next day if time has passed
                    if (now > STG)
                    {
                        BLU = BLU.AddDays(1);
                        VLG = VLG.AddDays(1);
                        STG = STG.AddDays(1);
                    }

                    // Waiting 
                    var delayToBLU = BLU - DateTime.Now;
                    if (delayToBLU > TimeSpan.Zero)
                        await Task.Delay(delayToBLU, token);

                    if (!token.IsCancellationRequested)
                    {
                        await _vetcService.GetAuthenticationAsync(json.BLU.User!, "BLU");

                        await _vetcService.PostVetcAsync(
                            new GetsPayload
                            {
                                accountid = json.BLU.accountid,
                                fromdate = DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy"),
                                toDate = DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy"),
                            }, "BLU");
                    }

                    // Waiting 
                    var delayToVLG = VLG - DateTime.Now;
                    if (delayToVLG > TimeSpan.Zero)
                        await Task.Delay(delayToVLG, token);

                    if (!token.IsCancellationRequested)
                    {
                        await _vetcService.GetAuthenticationAsync(json.VLG.User!, "VLG");

                        await _vetcService.PostVetcAsync(
                            new GetsPayload
                            {
                                accountid = json.VLG.accountid,
                                fromdate = DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy"),
                                toDate = DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy"),
                            }, "VLG");
                    }

                    // Waiting 
                    var delayToSTG = STG - DateTime.Now;
                    if (delayToSTG > TimeSpan.Zero)
                        await Task.Delay(delayToSTG, token);

                    if (!token.IsCancellationRequested)
                    {
                        await _vetcService.GetAuthenticationAsync(json.STG.User!, "STG");

                        await _vetcService.PostVetcAsync(
                            new GetsPayload
                            {
                                accountid = json.STG.accountid,
                                fromdate = DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy"),
                                toDate = DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy"),
                            }, "STG");
                    }

                }
                catch (TaskCanceledException)
                {
                    // bị hủy thì thoát vòng lặp
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[VetcWorker] Lỗi: {ex.Message}");
                    await Task.Delay(5000, token); // nghỉ 5s rồi thử lại
                }
            }
        }

        private async Task<RootObjectVetc> LoadSchemaJson()
        {
            var json = await File.ReadAllTextAsync("VetcAuthentication.json");
            return JsonSerializer.Deserialize<RootObjectVetc>(json)!;
        }
    }
}
