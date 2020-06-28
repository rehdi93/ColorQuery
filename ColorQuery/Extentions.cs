using System;
using Color = System.Windows.Media.Color;

namespace ColorQuery
{
    static class Extentions
    {
        public static string ToHexString(this Color c, bool pound=true)
        {
            var val = $"{c.R:X2}{c.G:X2}{c.B:X2}";
            return pound ? '#' + val : val;
        }
    }

    public static class I18n
    {
        public static string translate(string text)
        {
            var result = Properties.Resources.ResourceManager.GetString(text);
            return string.IsNullOrEmpty(result) ? text : result;
        }
    }

    enum ColorFormat
    {
        RGB, CMYK, HEX
    }
}
