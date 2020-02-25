namespace ColorQuery
{
    using Color = System.Windows.Media.Color;

    static class Extentions
    {
        public static string ToHexString(this Color c, bool pound=true)
        {
            var val = $"{c.R:X2}{c.G:X2}{c.B:X2}";
            return pound ? '#' + val : val;
        }
    }

    enum ColorFormat
    {
        RGB, CMYK, HEX
    }
}
