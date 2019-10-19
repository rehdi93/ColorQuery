namespace ColorQuery
{
    using GDIColor = System.Drawing.Color;
    using Color = System.Windows.Media.Color;

    static class Extentions
    {
        public static string ToHexString(this GDIColor c, bool hashtag=true)
        {
            var val = $"{c.R:X2}{c.G:X2}{c.B:X2}";
            return hashtag ? '#' + val : val;
        }

        public static string ToHexString(this Color c, bool hashtag=true)
        {
            var val = $"{c.R:X2}{c.G:X2}{c.B:X2}";
            return hashtag ? '#' + val : val;
        }
    }

    enum ColorFormat
    {
        RGB, CMYK, HEX
    }
}
