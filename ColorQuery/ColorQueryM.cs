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

        public string UiText => GetText(this.format);

        public string GetText(ColorFormat format)
        {
            switch (format)
            {
                case ColorFormat.RGB:
                    return $"{color.R} {color.G} {color.B}";
                case ColorFormat.CMYK:
                    return new Cmyk(color).ToString();
                case ColorFormat.HEX:
                    return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
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


        Color color;
        ColorFormat format;
        double zoom = 1;
    }


}
