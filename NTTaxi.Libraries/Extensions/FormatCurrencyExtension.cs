using System.Globalization;

namespace NTTaxi.Libraries.Extensions;
public static class FormatCurrencyExtension
{
    // Convert string to string VND
    private static readonly string zero = "0";
    // Culture info for Vietnamese currency formatting
    private static readonly CultureInfo viCulture = new CultureInfo("vi-VN");

    // Phương thức trả về giá trị string
    public static string ltvVNDCurrency(this object input)
    {
        try
        {
            // Nếu đầu vào là null, trả về chuỗi "0"
            if (input == null) return zero;

            // Nếu đầu vào là số nguyên, định dạng nó
            if (input is decimal dec)
            {
                return dec.ToString("N0", viCulture);
            }

            // Nếu đầu vào là số nguyên 64-bit, định dạng nó
            if (input is string str)
            {
                // Định dạng lại chuỗi vào
                var clean = str.Replace(",", "").Replace(".", "").Trim();

                // Nếu chuỗi là số nguyên, định dạng nó
                if (decimal.TryParse(clean, out decimal parsed))
                {
                    // Nếu chuỗi là số nguyên, định dạng nó
                    return parsed.ToString("N0", viCulture);
                }
            }

            // Nếu đầu vào là số nguyên 32-bit, định dạng nó
            return zero;
        }
        catch
        {
            // Nếu có lỗi xảy ra, trả về chuỗi "0"
            return zero;
        }
    }

    // Phương thức trả về giá trị decimal
    public static decimal ltvVNDCurrencyToDecimal(this object input)
    {
        try
        {
            if (input == null)
                return 0;

            if (input is decimal dec)
                return dec;

            if (input is string str)
            {
                // Xoá đơn vị tiền tệ nếu có
                str = str.Replace("VND", "", StringComparison.OrdinalIgnoreCase)
                         .Replace("₫", "")
                         .Replace(".", "")
                         .Replace(",", "")
                         .Trim();

                if (decimal.TryParse(str, out decimal parsed))
                    return parsed;
            }

            if (input is IConvertible)
                return Convert.ToDecimal(input);

            return 0;
        }
        catch
        {
            return 0;
        }
    }
}
