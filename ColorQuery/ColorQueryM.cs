using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Media;
using MvvmHelpers;


namespace ColorQuery
{

    class ColorQueryModel : ObservableObject
    {
        public Color CurrentColor
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
        public ColorFormat CurrentFormat
        {
            get => format;
            set
            {
                if (SetProperty(ref format, value))
                {
                    OnPropertyChanged(nameof(UiText));
                }
            }
        }
        public ObservableCollection<Color> History { get; } = new ObservableCollection<Color>();

        public string Status
        {
            get { return status; }
            set { SetProperty(ref status, value); }
        }

        public double Zoom
        {
            get { return zoom; }
            set { SetProperty(ref zoom, value); }
        }


        public string UiText => FormatColorInfo(true, this.format);

        public string GetText(ColorFormat fmt) => FormatColorInfo(false, fmt);


        void AddRecent(Color color)
        {
            var idx = History.IndexOf(color);
            if (idx == -1)
            {
                History.Insert(0, color);
                if (History.Count > 10)
                {
                    History.RemoveAt(10);
                }
            }
        }

        string FormatColorInfo(bool ui, ColorFormat format)
        {
            string infoText = "...";

            if (ui)
            {
                switch (format)
                {
                    case ColorFormat.RGB:
                        infoText = $"R: {color.R}\n" +
                                   $"G: {color.G}\n" +
                                   $"B: {color.B}";
                        break;
                    case ColorFormat.CMYK:
                        var cmyk = new Cmyk(color);

                        infoText = $"C: {cmyk.C:F3}\n" +
                                   $"M: {cmyk.M:F3}\n" +
                                   $"Y: {cmyk.Y:F3}\n" +
                                   $"K: {cmyk.K:F3}";
                        break;
                    case ColorFormat.HEX:
                        infoText = "Hex: " + color.ToHexString();
                        break;
                }
            }
            else
            {
                switch (format)
                {
                    case ColorFormat.RGB:
                        infoText = $"{color.R} {color.G} {color.B}";
                        break;
                    case ColorFormat.CMYK:
                        var cmyk = new Cmyk(color);
                        infoText = $"{cmyk.C:F3} {cmyk.M:F3} {cmyk.Y:F3} {cmyk.K:F3}";
                        break;
                    case ColorFormat.HEX:
                        infoText = color.ToHexString();
                        break;
                }
            }

            return infoText;
        }

        string status;
        Color color;
        ColorFormat format;
        double zoom = 1;
    }


}
