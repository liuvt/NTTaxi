using NTTaxi.Libraries.WindowAutomations.Interfaces;
using System.Diagnostics;


namespace NTTaxi.Libraries.WindowAutomations
{
    public class JavaLauncherAutomation : IJavaLauncherAutomation
    {
        public void OpenSkysoftJnlp(string jnlpPath)
        {
            if (OperatingSystem.IsWindows())
            {
                Console.WriteLine("Chạy trên Windows, mở Skysoft.jnlp");
                var psi = new ProcessStartInfo
                {
                    FileName = @"C:\Program Files (x86)\Java\jre1.8.0_451\bin\javaws.exe",
                    Arguments = $"\"D:\\Dev\\Skysoft\\skysoft.jnlp\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                Process.Start(psi);
            }
            else
            {
                Console.WriteLine("Không phải Windows, không mở Skysoft");
            }
        }
    }
}
