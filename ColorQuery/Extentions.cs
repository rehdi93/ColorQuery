using System;
using Color = System.Windows.Media.Color;

namespace ColorQuery
{
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
