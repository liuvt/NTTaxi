using NTTaxi.Libraries.Models.Alis;
using NTTaxi.Libraries.Services.Interfaces;
using NTTaxi.Libraries.Workers.Interfaces;
using System.Text.Json;

namespace NTTaxi.Libraries.Workers
{
    public class AliWorker : IAliWorker
    {
        private readonly IAliService _aliService;
        private CancellationTokenSource? _cts;

        public event Action? StatusChanged; // Sự kiện trạng thái thay đổi (đang chạy/dừng)
        public bool IsRunning => _cts != null;
        public AliWorker(IAliService aliService)
        {
            _aliService = aliService;
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
                    // PostOrder (05:20)
                    var orderTime = now.Date.AddHours(5).AddMinutes(10);
                    var partnergsmTime = now.Date.AddHours(5).AddMinutes(15);
                    var promoteTime = now.Date.AddHours(5).AddMinutes(25);
                    var switchboardTime = now.Date.AddHours(5).AddMinutes(35);
                    // PostCancelOrder (05:50)
                    var cancelOrderTime = now.Date.AddHours(5).AddMinutes(45);
                    var onlineApp = now.Date.AddHours(5).AddMinutes(55);


                    /*
                    var orderTime = now.AddSeconds(5);
                    var promoteTime = now.AddSeconds(5);
                    var cancelOrderTime = now.AddSeconds(5);*/

                    // Next day if time has passed
                    if (now > cancelOrderTime)
                    {
                        orderTime = orderTime.AddDays(1);
                        partnergsmTime = cancelOrderTime.AddDays(1);
                        promoteTime = promoteTime.AddDays(1);
                        switchboardTime = cancelOrderTime.AddDays(1);
                        cancelOrderTime = cancelOrderTime.AddDays(1);
                        onlineApp = onlineApp.AddDays(1);
                    }

                    // Waiting PostOrder
                    var delayToOrder = orderTime - DateTime.Now;
                    if (delayToOrder > TimeSpan.Zero)
                        await Task.Delay(delayToOrder, token);

                    if (!token.IsCancellationRequested)
                    {
                        await _aliService.PostOrderAli(
                            await LoadSchemaJson(),
                            DateTime.Now.AddDays(-1).Date.AddHours(5),
                            DateTime.Now.Date.AddHours(5));
                    }

                    // Waiting PostPartnerGSM
                    var delayToPartnert = partnergsmTime - DateTime.Now;
                    if (delayToPartnert > TimeSpan.Zero)
                        await Task.Delay(delayToPartnert, token);

                    if (!token.IsCancellationRequested)
                    {
                        await _aliService.PostPartnerGSMAli(
                            DateTime.Now.AddDays(-1).Date.AddHours(5),
                            DateTime.Now.Date.AddHours(5));
                    }

                    // Waiting PostPromote
                    var delayToPromote = promoteTime - DateTime.Now;
                    if (delayToPromote > TimeSpan.Zero)
                        await Task.Delay(delayToPromote, token);

                    if (!token.IsCancellationRequested)
                    {
                        await _aliService.PostPromoteAli(
                            await LoadSchemaJson(),
                            DateTime.Now.AddDays(-1).Date.AddHours(5),
                            DateTime.Now.Date.AddHours(5));
                    }

                    // Waiting PosSwitchboard
                    var delayToSwitchboard = switchboardTime - DateTime.Now;
                    if (delayToSwitchboard > TimeSpan.Zero)
                        await Task.Delay(delayToSwitchboard, token);

                    if (!token.IsCancellationRequested)
                    {
                        await _aliService.PostSwitchboardAli(
                            DateTime.Now.AddDays(-1).Date.AddHours(5),
                            DateTime.Now.Date.AddHours(5));
                    }

                    // Waiting PostCancelOrder
                    var delayToCancelOrder = cancelOrderTime - DateTime.Now;
                    if (delayToCancelOrder > TimeSpan.Zero)
                        await Task.Delay(delayToCancelOrder, token);

                    if (!token.IsCancellationRequested)
                    {
                        await _aliService.PostCancelOrderAli(
                            DateTime.Now.AddDays(-1).Date.AddHours(5),
                            DateTime.Now.Date.AddHours(5));
                    }

                    // Waiting OnlineApp
                    var delayToOnlineApp = onlineApp - DateTime.Now;
                    if (delayToOnlineApp > TimeSpan.Zero)
                        await Task.Delay(delayToOnlineApp, token);

                    if (!token.IsCancellationRequested)
                    {
                        await _aliService.PostOnlineAppAli(
                            await LoadSchemaJson(),
                            DateTime.Now.AddDays(-1).Date.AddHours(5),
                            DateTime.Now.Date.AddHours(5));
                    }
                }
                catch (TaskCanceledException)
                {
                    // bị hủy thì thoát vòng lặp
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AliWorker] Lỗi: {ex.Message}");
                    await Task.Delay(5000, token); // nghỉ 5s rồi thử lại
                }
            }
        }

        private async Task<SchemaJson> LoadSchemaJson()
        {
            var json = await File.ReadAllTextAsync("AliAuthentication.json");
            return JsonSerializer.Deserialize<SchemaJson>(json)!;
        }
    }
}
