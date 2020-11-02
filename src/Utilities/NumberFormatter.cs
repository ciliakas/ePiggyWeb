using System.Drawing;
using System.Globalization;

namespace ePiggyWeb.Utilities
{
    public class NumberFormatter
    {
        public static CurrencyWithColor FormatCurrencyWithColor(decimal value)
        {
            var color = value >= 0 ? Color.Green : Color.Red;
            return new CurrencyWithColor(FormatCurrency(value), color);
        }
        
        public static string FormatCurrency(decimal value)
        {
            var cultureInfo = CultureInfo.CurrentCulture;
            var numberFormat = cultureInfo.NumberFormat;
            var pattern = string.Empty;
            if (value >= decimal.Zero)
            {
                pattern = numberFormat.CurrencyPositivePattern switch
                {
                    0 => "{0}{1:N" + numberFormat.CurrencyDecimalDigits + "}",
                    1 => "{1:N" + numberFormat.CurrencyDecimalDigits + "}{0}",
                    2 => "{0} {1:N" + numberFormat.CurrencyDecimalDigits + "}",
                    3 => "{1:N" + numberFormat.CurrencyDecimalDigits + "} {0}",
                    _ => pattern
                };
            }
            else
            {
                value = -value;
                pattern = numberFormat.CurrencyNegativePattern switch
                {
                    0 => "({0}{1:N" + numberFormat.CurrencyDecimalDigits + "})",
                    1 => "-{0}{1:N" + numberFormat.CurrencyDecimalDigits + "}",
                    2 => "{0}-{1:N" + numberFormat.CurrencyDecimalDigits + "}",
                    3 => "{0}{1:N" + numberFormat.CurrencyDecimalDigits + "}-",
                    4 => "({1:N" + numberFormat.CurrencyDecimalDigits + "}{0})",
                    5 => "-{1:N" + numberFormat.CurrencyDecimalDigits + "}{0}",
                    6 => "{1:N" + numberFormat.CurrencyDecimalDigits + "}-{0}",
                    7 => "{1:N" + numberFormat.CurrencyDecimalDigits + "}{0}-",
                    8 => "-{1:N" + numberFormat.CurrencyDecimalDigits + "} {0}",
                    9 => "-{0} {1:N" + numberFormat.CurrencyDecimalDigits + "}",
                    10 => "{1:N" + numberFormat.CurrencyDecimalDigits + "} {0}-",
                    11 => "{0} {1:N" + numberFormat.CurrencyDecimalDigits + "}-",
                    12 => "{0} -{1:N" + numberFormat.CurrencyDecimalDigits + "}",
                    13 => "{1:N" + numberFormat.CurrencyDecimalDigits + "}- {0}",
                    14 => "({0} {1:N" + numberFormat.CurrencyDecimalDigits + "})",
                    15 => "({1:N" + numberFormat.CurrencyDecimalDigits + "} {0})",
                    _ => pattern
                };
            }
            var formattedValue = string.Format(cultureInfo, pattern, numberFormat.CurrencySymbol, value);
            return formattedValue;
        }
    }
}
