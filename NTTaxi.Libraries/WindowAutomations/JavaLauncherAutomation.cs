using NTTaxi.Libraries.Models.Skysofts;
using NTTaxi.Libraries.WindowAutomations.Interfaces;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NTTaxi.Libraries.WindowAutomations
{
    public class JavaLauncherAutomation : IJavaLauncherAutomation
    {
        public void OpenSkysoftJnlp(string jnlpPath, UserSkysoft user)
        {
            if (OperatingSystem.IsWindows())
            {
                Console.WriteLine("Chạy trên Windows, mở Skysoft.jnlp");
                var psi = new ProcessStartInfo
                {
                    FileName = @"C:\Program Files (x86)\Java\jre1.8.0_451\bin\javaws.exe",
                    Arguments = $"\"{jnlpPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                Process.Start(psi);

                // Đợi Skysoft load xong (ví dụ 5 giây)
                Thread.Sleep(5000);

                var engine = new SkysoftAutomationEngine();

                // Bước điền username
                engine.AddStep(new AutomationStep("Điền Username",
                    () => AutoItHelper.WaitClickSend("textboxUser.png", user.Username)));

                // Bước điền password
                engine.AddStep(new AutomationStep("Điền Password",
                    () => AutoItHelper.WaitClickSend("textboxPass.png", user.Password)));

                // Bước click Login
                engine.AddStep(new AutomationStep("Click Login",
                    () => AutoItHelper.WaitAndClick("buttonLogin.png", 17000)));

                // Bước click Report
                engine.AddStep(new AutomationStep("Click Report",
                    () => AutoItHelper.WaitAndClick("reportImage.png", 3000)));

               
                engine.AddStep(new AutomationStep("Click cross multiple times", () =>
                {
                    AutoItHelper.ShowWindow("ADMIN NAM THANG TAXI"); // đảm bảo cửa sổ active
                    for (int i = 0; i < 25; i++)
                    {
                        var pos = ImageSearchWrapper.FindImage("crossImage.png");
                        if (pos == null)
                        {
                            Console.WriteLine("[INFO] Không còn nút cross, dừng click.");
                            break;
                        }
                        AutoItX3Native.AU3_MouseClick("left", pos.Value.x, pos.Value.y, 1, 0);
                        Thread.Sleep(100); // đợi app render
                    }
                    return StepResult.Success;
                }));

                // Bước chọn GPS report
                engine.AddStep(new AutomationStep("Chọn GPS Report",
                    () => AutoItHelper.WaitAndClick("ImgAutomations/tittleImage.png", 2000)));

                // Bước điền từ ngày
                engine.AddStep(new AutomationStep("Điền FromDate",
                    () => AutoItHelper.WaitClickSend("ImgAutomations/fromdataImage.png", DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy"))));

                // Bước điền đến ngày
                engine.AddStep(new AutomationStep("Điền ToDate",
                    () => AutoItHelper.WaitClickSend("ImgAutomations/todataImage.png", DateTime.Now.ToString("dd/MM/yyyy"))));

                // Bước điền từ giờ
                engine.AddStep(new AutomationStep("Điền FromTime",
                    () => AutoItHelper.WaitClickSend("ImgAutomations/fromtimeImage.png", "050000")));

                // Bước điền đến giờ
                engine.AddStep(new AutomationStep("Điền ToTime",
                    () => AutoItHelper.WaitClickSend("ImgAutomations/totimeImage.png", "050000")));

                // Bước điền đường dẫn file
                string path = $"D:/NT/202508/27/{user.ProviderCode}-27082025-{((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds()}.xls";
                engine.AddStep(new AutomationStep("Điền file path",
                    () => AutoItHelper.WaitClickSend("urlImage.png", path)));

                // Bước click Run Report
                engine.AddStep(new AutomationStep("Run Report",
                    () => AutoItHelper.WaitAndClick("ImgAutomations/reportviewImage.png", 2000)));

                // Chạy engine
                engine.Run();

                // Hiện Notepad (hoặc cửa sổ khác)
                AutoItHelper.ShowWindow("ADMIN NAM THANG TAXI");

            }
            else
            {
                Console.WriteLine("Không phải Windows, không mở Skysoft");
            }
        }

    }
    public enum StepResult
    {
        Success,
        Fail
    }
    public class AutomationStep
    {
        public string Name { get; set; }
        public Func<StepResult> Action { get; set; }

        public AutomationStep(string name, Func<StepResult> action)
        {
            Name = name;
            Action = action;
        }
    }
    public class SkysoftAutomationEngine
    {
        private readonly List<AutomationStep> _steps = new();

        public void AddStep(AutomationStep step)
        {
            _steps.Add(step);
        }

        public bool Run()
        {
            foreach (var step in _steps)
            {
                Console.WriteLine($"[INFO] Thực hiện bước: {step.Name}");
                var result = step.Action();
                if (result == StepResult.Fail)
                {
                    Console.WriteLine($"[ERROR] Bước '{step.Name}' thất bại. Dừng quy trình!");
                    return false; // hoặc rollback nếu bạn muốn
                }
            }

            Console.WriteLine("[INFO] Quy trình hoàn tất thành công!");
            return true;
        }
    }

    public static class AutoItHelper
    {
        /// <summary>
        /// Chờ ảnh xuất hiện và click
        /// </summary>
        public static StepResult WaitAndClick(string imagePath, int timeoutMs = 50000, int retryDelayMs = 200)
        {
            int elapsed = 0;
            while (elapsed < timeoutMs)
            {
                var pos = ImageSearchWrapper.FindImage(imagePath);
                if (pos != null)
                {
                    AutoItX3Native.AU3_MouseClick("left", pos.Value.x, pos.Value.y, 1, 0);
                    return StepResult.Success;
                }
                Thread.Sleep(retryDelayMs);
                elapsed += retryDelayMs;
            }

            Console.WriteLine($"[ERROR] Không tìm thấy image: {imagePath}");
            return StepResult.Fail;
        }

        /// <summary>
        /// Chờ ảnh xuất hiện, click và gửi text
        /// </summary>
        public static StepResult WaitClickSend(string imagePath, string text, int timeoutMs = 50000)
        {
            if (WaitAndClick(imagePath, timeoutMs) == StepResult.Success)
            {
                AutoItX3Native.AU3_Send("^a", 0);
                AutoItX3Native.AU3_Send("{DEL}", 0);
                AutoItX3Native.AU3_Send(text, 1);
                return StepResult.Success;
            }
            return StepResult.Fail;
        }

        /// <summary>
        /// Show và kích hoạt cửa sổ
        /// </summary>
        public static void ShowWindow(string title)
        {
            AutoItX3Native.AU3_WinSetState(title, "", AutoItX3Native.SW_SHOW);
            AutoItX3Native.AU3_WinActivate(title, "");
        }
    }

    public class ImageSearchWrapper
    {
        [DllImport("ImageSearchDLL.dll", EntryPoint = "ImageSearch")]
        private static extern IntPtr ImageSearch(
            int x1, int y1, int x2, int y2, string imagePath);

        public static (int x, int y)? FindImage(string imagePath, int x1 = 0, int y1 = 0, int x2 = 1920, int y2 = 1080)
        {
            var result = ImageSearch(x1, y1, x2, y2, imagePath);
            if (result == IntPtr.Zero) return null;

            var resStr = Marshal.PtrToStringAnsi(result);
            // format result: "0|x|y|width|height"
            var parts = resStr.Split('|');
            if (parts.Length >= 5)
            {
                int.TryParse(parts[1], out int x);
                int.TryParse(parts[2], out int y);
                int.TryParse(parts[3], out int w);
                int.TryParse(parts[4], out int h);

                // lấy tâm của ảnh
                x += w / 2;
                y += h / 2;

                return (x, y);
            }
            return null;
        }
    }

    public static class AutoItX3Native
    {
        // Hàm MouseClick trong AutoItX3.dll
        [DllImport("AutoItX3_x64.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern int AU3_MouseClick(
            [MarshalAs(UnmanagedType.LPWStr)] string button,
            int x,
            int y,
            int clicks,
            int speed
        );

        // Hàm Send trong AutoItX3.dll
        [DllImport("AutoItX3_x64.dll", CharSet = CharSet.Auto)]
        public static extern void AU3_Send(
            [MarshalAs(UnmanagedType.LPWStr)] string sendText,
            int mode
        );

        //Hide/show apps
        [DllImport("AutoItX3_x64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern void AU3_WinActivate(
        [MarshalAs(UnmanagedType.LPWStr)] string title,
        [MarshalAs(UnmanagedType.LPWStr)] string text);

        [DllImport("AutoItX3_x64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern void AU3_WinSetState(
            [MarshalAs(UnmanagedType.LPWStr)] string title,
            [MarshalAs(UnmanagedType.LPWStr)] string text,
            int flags);
        // State flags
        public const int SW_HIDE = 0;
        public const int SW_SHOW = 5;
        public const int SW_MINIMIZE = 6;
        public const int SW_MAXIMIZE = 3;
        public const int SW_RESTORE = 9;
    }
}
