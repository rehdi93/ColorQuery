using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Res = ColorQuery.Properties.Resources;
using IRect = System.Windows.Int32Rect;


namespace ColorQuery
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            preview.Source = CaptureScreen(GetScreenRect());
            ScrollHome();

            // set toolbar item tooltips
            var toolmenuitems = tooltray.ToolBars
                .SelectMany(t => t.Items.OfType<Menu>())
                .SelectMany(t => t.Items.OfType<MenuItem>())
                .Where(mi => mi.Command != null);
            ;

            foreach (MenuItem mi in toolmenuitems)
            {
                var cmd = (RoutedUICommand)mi.Command;
                mi.ToolTip = cmd.Text;

                if (!string.IsNullOrEmpty(mi.InputGestureText))
                {
                    mi.ToolTip += " (" + mi.InputGestureText + ")";
                }
            }

            miGoHome.ToolTip = NavigationCommands.BrowseHome.Text;
        }

        BitmapSource CaptureScreen(IRect screenRect)
        {
            using var bm = new System.Drawing.Bitmap(screenRect.Width, screenRect.Height);
            bm.SetResolution(96f, 96f);

            using var g = System.Drawing.Graphics.FromImage(bm);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            g.CopyFromScreen(screenRect.X, screenRect.Y, 0, 0, bm.Size);

            using var mem = new System.IO.MemoryStream();
            bm.Save(mem, System.Drawing.Imaging.ImageFormat.Png);

            var screen = new BitmapImage();
            screen.BeginInit();
            screen.CacheOption = BitmapCacheOption.OnLoad;
            screen.StreamSource = mem;
            screen.EndInit();

            return screen;
        }

        // https://stackoverflow.com/a/14508110
        Color GetPixel(BitmapSource image, int x, int y)
        {
            try
            {
                var crop = new CroppedBitmap(image, new IRect(x, y, 1, 1));
                var pixelbuff = new byte[4]; // [0] blue, green, red, alpha [3]
                crop.CopyPixels(pixelbuff, 4, 0);
                return Color.FromRgb(pixelbuff[2], pixelbuff[1], pixelbuff[0]);
            }
            catch (Exception)
            {
                return Colors.Transparent;
            }
        }

        IRect GetScreenRect(DpiScale dpi)
        {
            return new IRect(
                (int)(SystemParameters.VirtualScreenLeft * dpi.DpiScaleX),
                (int)(SystemParameters.VirtualScreenTop * dpi.DpiScaleY),
                (int)(SystemParameters.VirtualScreenWidth * dpi.DpiScaleX),
                (int)(SystemParameters.VirtualScreenHeight * dpi.DpiScaleY)
            );
        }
        IRect GetScreenRect() => GetScreenRect(VisualTreeHelper.GetDpi(this));

        private void ScrollHome()
        {
            var rect = GetScreenRect();
            // displays to the left of main have negative coords
            scrollview.ScrollToHorizontalOffset(Math.Abs(rect.X));
            scrollview.ScrollToVerticalOffset(Math.Abs(rect.Y));
        }


        private void RefreshCmd_Executed(object _, ExecutedRoutedEventArgs __)
        {
            var rect = GetScreenRect();

            // hide window by moving it offscreen, make sure to
            // get the dpi before moving
            var bounds = RestoreBounds;
            Left = Top = int.MaxValue;
            
            preview.Source = CaptureScreen(rect);

            Left = bounds.Left;
            Top = bounds.Top;
        }

        private void preview_MouseBtnClick(object sender, MouseButtonEventArgs e)
        {
            var image = (Image)sender;
            var bm = (BitmapSource)image.Source;

            var pos = e.GetPosition(image);
            model.CurrentColor = GetPixel(bm, (int)pos.X, (int)pos.Y);
            model.Status = string.Format(Res.mousepos_fmt2, (int)pos.X, (int)pos.Y);
        }

        int lastMouseMove = 0;
        private void preview_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.RoutedEvent.Name == MouseLeaveEvent.Name)
            {
                model.Status = string.Format(Res.mousepos_fmt, "OB");
            }
            else if (e.RoutedEvent.Name == MouseMoveEvent.Name && e.Timestamp - lastMouseMove >= 100)
            {
                var pos = e.GetPosition((Image)sender);
                model.Status = string.Format(Res.mousepos_fmt2, (int)pos.X, (int)pos.Y);
                lastMouseMove = e.Timestamp;
            }
        }

        private void CopyCmd_Exec(object _, ExecutedRoutedEventArgs e)
        {
            var format = model.CurrentFormat;
            if (e.Parameter is ColorFormat fmt)
                format = fmt;

            Clipboard.SetText(model.GetText(format));
            model.Status = Res.ColorCopied;
        }
        private void CopyCmd_CanExec(object _, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = model.CurrentColor != Colors.Transparent;
        }

        private void ZoomCmd_Exec(object _, ExecutedRoutedEventArgs e)
        {
            var zoomcmd = (RoutedCommand)e.Command;

            if (zoomcmd.Name == NavigationCommands.IncreaseZoom.Name)
            {
                model.Zoom += zoomSlider.LargeChange;
            }
            else if (zoomcmd.Name == NavigationCommands.DecreaseZoom.Name)
            {
                model.Zoom -= zoomSlider.LargeChange;
            }
            else if (zoomcmd.Name == NavigationCommands.Zoom.Name)
            {
                // ...
            }
        }
        private void ZoomCmd_CanExec(object _, CanExecuteRoutedEventArgs e)
        {
            var zoomcmd = (RoutedCommand)e.Command;

            if (zoomcmd.Name == NavigationCommands.IncreaseZoom.Name)
            {
                e.CanExecute = model.Zoom < zoomSlider?.Maximum;
            }
            else if (zoomcmd.Name == NavigationCommands.DecreaseZoom.Name)
            {
                e.CanExecute = model.Zoom > zoomSlider?.Minimum;
            }
        }

        private void CloseCmd_Exec(object _, ExecutedRoutedEventArgs __) => Close();

        private void onColorFmtChanged(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)e.Source;
            if (element.Tag is ColorFormat fmt)
            {
                model.CurrentFormat = fmt;
            }
        }

        private void miAbout_Click(object sender, RoutedEventArgs e)
        {
            var aboutbox = new AboutBox();
            aboutbox.ShowDialog();
        }

        private void miGoHome_Click(object _, RoutedEventArgs __) => ScrollHome();

    }
}
