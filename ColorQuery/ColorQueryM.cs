using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Media;
using MvvmHelpers;


namespace ColorQuery
{

    class ColorQueryModel : BaseViewModel
    {
        public Color Color
        {
            get => color;
            set {
                if (SetProperty(ref color, value))
                {
                    OnPropertyChanged(nameof(UiText));
                    AddRecent(color);
                }
            }
        }
        public ColorFormat Format
        {
            get => format;
            set {
                if (SetProperty(ref format, value))
                {
                    OnPropertyChanged(nameof(UiText));
                }
            }
        }
        public ObservableCollection<Color> History { get; } = new ObservableCollection<Color>();

        public double Zoom
        {
            get { return zoom; }
            set { SetProperty(ref zoom, value); }
        }

        public string UiText
        {
            get {
                switch (format)
                {
                    case ColorFormat.RGB:
                        return $"R={color.R}; G={color.G}; B={color.B}";
                    case ColorFormat.HEX:
                        return color.ToString();
                    case ColorFormat.CMYK:
                        var (c,m,y,k) = toCMYK(color);
                        return $"C={c:F3}; M={m:F3}; Y={y:F3}; K={k:F3}";
                    default:
                        return "???";
                }
            }
        }

        public string GetText(ColorFormat format)
        {
            switch (format)
            {
                case ColorFormat.RGB:
                    return $"{color.R} {color.G} {color.B}";
                case ColorFormat.CMYK:
                    var (c, m, y, k) = toCMYK(color);
                    return $"{c:F3} {m:F3} {y:F3} {k:F3}";
                case ColorFormat.HEX:
                    return color.ToString();
                default:
                    return null;
            }
        }


        void AddRecent(Color color)
        {
            const int MAX = 10;

            var idx = History.IndexOf(color);
            if (idx == -1)
            {
                History.Insert(0, color);
                if (History.Count > MAX)
                    History.RemoveAt(MAX);
            }
            else
            {
                History.Move(idx, 0);
            }
        }

        static (float c, float m, float y, float k)
        toCMYK(Color c)
        {
            if (c == Colors.Black)
            {
                return (0, 0, 0, 0);
            }

            var r = c.R / 255f;
            var g = c.G / 255f;
            var b = c.B / 255f;

            var k = 1 - Math.Max(r, Math.Max(g, b));

            return (
                (1 - r - k) / (1 - k),
                (1 - g - k) / (1 - k),
                (1 - b - k) / (1 - k),
                k
            );
        }


        Color color;
        ColorFormat format;
        double zoom = 1;
    }


}
