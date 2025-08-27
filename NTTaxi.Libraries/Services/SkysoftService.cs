using Google.Apis.Sheets.v4.Data;
using NTTaxi.Libraries.Extensions;
using NTTaxi.Libraries.Services.Interfaces;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace NTTaxi.Libraries.Services
{
    public class SkysoftService : ISkysoftService
    {
        private readonly HttpClient httpClient;
        private readonly CookieContainer cookieContainer;

        public SkysoftService()
        {
            cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                UseCookies = true,
                AutomaticDecompression = DecompressionMethods.None // bỏ gzip/deflate
            };

            httpClient = new HttpClient(handler);

            // Thêm cookie
            cookieContainer.Add(new Uri("https://tracking.skysoft.vn"),
                    new Cookie("JSESSIONID", "s26~8516E92F772873AB1E926EE15578B1BC"));
            cookieContainer.Add(new Uri("https://tracking.skysoft.vn"),
                new Cookie("tokenID", Uri.UnescapeDataString("qFqToxW%2BL4kKaJnqVOawNpeXvUb3nETt%2BibcHRPZao7%2FfOa6vLSWafAp46CBimQLyscaEEr2l9irp943oVJTRJP8%2FfV2aOXh9Tisdp%2B2N2%2Fc%2FtHxUW4EE0ytS6JcMWi9YvjGO6rXWOuy2DheXePYO%2Frrs76jvWqO%2FRhL1UzcgtLQF3Rug%2FQUin7H4UZMFseyi5JOqnQhCg9mBLp15pUnf%2B5TH6yXkFqrlUMGuJTb1%2B7xNC9xtD9j%2F%2Bh319%2Bhr75HNqXwq7ie9MIUb1RzhRESBhw5Uf7yGLdNJ96mj8T7WyniE24AGN%2FXVa3p1Q33UYG4vTlBpy03J3d9WIT7mv6HaSTgnzmBgG0rNCYulSFLlyBg36d2NUK5svem1QpiOViSIMucWZK1LgSxlvMbgyVUBowxwJZ3m8qOzVvsKt4XmNkZS03cYWERh2B0LaKBLBEfVBM62quCIYUfeNYQbUwzEp7thrZZKagGJMPtyngRphU%3D")));

            // Thêm User-Agent
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows 2000)");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/gif"));
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/jpeg"));
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.2));
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
        }

        public async Task<string> GetDatas()
        {
            string inputFile = "236_Response.bin";
            string outputFile = "output.xls"; // hoặc .xls tùy gói dữ liệu

            // 1. Đọc toàn bộ file nhị phân
            byte[] data = File.ReadAllBytes(inputFile);

            try
            {
                byte[] header = new byte[4];
                using (var fs = File.OpenRead(inputFile))
                {
                    fs.Read(header, 0, 4);
                }
                if (Encoding.UTF8.GetString(header) != "DDTP")
                    return "File is not a valid DDTP package.";

                // 2. Nạp vào DDTP
                var ddtp = new DDTP(data);

                // 3. Lấy byte[] Excel từ $Var.Return$
                byte[] excelBytes = ddtp.GetByteArray("$Var.Return$");

                if (excelBytes != null && excelBytes.Length > 0)
                {
                    File.WriteAllBytes(outputFile, excelBytes);
                   return $"Xuất Excel thành công: {outputFile} ({excelBytes.Length} bytes)";
                }
                else
                {
                    return "Không tìm thấy dữ liệu Excel trong $Var.Return$";
                }
            }
            catch (Exception ex)
            {
                return  "Lỗi khi đọc DDTP: " + ex.Message;
            }
        }

        public static DDTP HexToDDTP(string hex)
        {
            // 1. Làm sạch string: chỉ giữ hex
            string hexClean = new string(hex.Where(c => Uri.IsHexDigit(c)).ToArray());

            // 2. Kiểm tra số ký tự chẵn
            if (hexClean.Length % 2 != 0)
                throw new ArgumentException("Hex string phải có số ký tự chẵn.");

            // 3. Chuyển sang byte[]
            byte[] data = new byte[hexClean.Length / 2];
            for (int i = 0; i < data.Length; i++)
                data[i] = Convert.ToByte(hexClean.Substring(i * 2, 2), 16);

            // 4. Tạo DDTP từ byte[]
            var ddtp = new DDTP(data);
            return ddtp;
        }

        public static byte[] HexStringToByteArray(string hex)
        {
            if (hex.Length % 2 != 0)
                throw new ArgumentException("Hex string phải có số ký tự chẵn");

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return bytes;
        }

        public async Task<string> DownloadDdtpFile()
        {
            using var response = await httpClient.GetAsync("https://tracking.skysoft.vn/");
            response.EnsureSuccessStatusCode();
            byte[] ddtpBytes = await response.Content.ReadAsByteArrayAsync();
            File.WriteAllBytes("236_Response.bin", ddtpBytes);
            return "Tải tệp DDTP thành công.";
        }
    }
}