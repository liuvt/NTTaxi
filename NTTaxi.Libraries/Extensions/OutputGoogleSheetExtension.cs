using System.Globalization;

namespace NTTaxi.Libraries.Extensions
{
    public static class OutputGoogleSheetExtension
    {
        public static DateTime? vltVetcDateTime(this string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            //Date time "HH:mm:ss - dd/MM/yyyy" => "dd/MM/yyyy HH:mm:ss"
            string format = "HH:mm:ss - dd/MM/yyyy";
            if (DateTime.TryParseExact(input.Trim(), format, CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out DateTime result))
            {
                return result;
            }
            return null;
        }

        public static string? vltVetcDateTimeToString(this string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            //Date time "HH:mm:ss - dd/MM/yyyy" => "dd/MM/yyyy HH:mm:ss"
            string format = "HH:mm:ss - dd/MM/yyyy";
            if (DateTime.TryParseExact(input.Trim(), format, CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out DateTime result))
            {
                return result.ToString("dd/MM/yyyy HH:mm:ss");
            }
            return null;
        }

        public static string vltVetcNormalizePlate(this string plate)
        {
            if (string.IsNullOrWhiteSpace(plate))
                return plate;

            // Nếu biển số kết thúc bằng 'V' thì cắt bỏ
            return plate.EndsWith("V", StringComparison.OrdinalIgnoreCase)
                ? plate[..^1]   // bỏ ký tự cuối
                : plate;
        }
    }
}