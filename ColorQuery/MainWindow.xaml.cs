using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using RectI = System.Windows.Int32Rect;
using Gdi = System.Drawing;
using ImgInterop = System.Windows.Interop.Imaging;
using static ColorQuery.Resources.I18n;


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
            NavigationCommands.IncreaseZoom.InputGestures.Add(new KeyGesture(Key.OemPlus, ModifierKeys.Control, "Ctrl++"));
            NavigationCommands.DecreaseZoom.InputGestures.Add(new KeyGesture(Key.OemMinus, ModifierKeys.Control, "Ctrl+-"));

            var dpi = GetDpi();
            preview.Source = CaptureScreen(dpi);
            ScrollHome();

            // set toolbar item tooltips
            var items = tooltray.ToolBars.SelectMany(tb => tb.Items.OfType<ButtonBase>()).Where(b => b.Command is RoutedUICommand);
            foreach (var btn in items)
            {
                var cmd = (RoutedUICommand)btn.Command;
                btn.ToolTip = cmd.Text;

                var g = cmd.InputGestures.OfType<KeyGesture>().FirstOrDefault();
                if (g != null)
                {
                    btn.ToolTip += " (" + g.DisplayString + ")";
                }
            }

            miGoHome.ToolTip = ComponentCommands.MoveToHome.Text;
            tbZoom.ToolTip = NavigationCommands.Zoom.Text;

            var ctxm = (ContextMenu)Resources["ctxmColorCopy"];
            ctxm.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, CopyCmd_Exec));

            // enum as itemsource: https://stackoverflow.com/a/6145957
            cbFormatSelect.ItemsSource = Enum.GetValues(typeof(ColorFormat));

            Resources.Remove("mockModel");
        }


        int lastMouseTimestamp = 0;

        BitmapSource CaptureScreen(RectI screenRect)
        {
            using var bm = new Gdi.Bitmap(screenRect.Width, screenRect.Height);
            bm.SetResolution(96f, 96f);

            using var g = Gdi.Graphics.FromImage(bm);

            g.SmoothingMode = Gdi.Drawing2D.SmoothingMode.HighQuality;
            g.InterpolationMode = Gdi.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = Gdi.Drawing2D.PixelOffsetMode.HighQuality;

            g.CopyFromScreen(screenRect.X, screenRect.Y, 0, 0, bm.Size);

            var hbitmap = bm.GetHbitmap();
            using var handle = new HBitmapHandle(hbitmap);
            return ImgInterop.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, screenRect, 
                    BitmapSizeOptions.FromEmptyOptions());
        }
        BitmapSource CaptureScreen(DpiScale dpi) => CaptureScreen(GetScreenRect(dpi));

        // https://stackoverflow.com/a/14508110
        Color GetPixel(BitmapSource image, int x, int y)
        {
            var crop = new CroppedBitmap(image, new RectI(x, y, 1, 1));
            var pixelbuff = new byte[4]; // { blue, green, red, alpha }
            crop.CopyPixels(pixelbuff, 4, 0);
            return Color.FromRgb(pixelbuff[2], pixelbuff[1], pixelbuff[0]);
        }

        RectI GetScreenRect(DpiScale dpi)
        {
            return new RectI(
                (int)(SystemParameters.VirtualScreenLeft * dpi.DpiScaleX),
                (int)(SystemParameters.VirtualScreenTop * dpi.DpiScaleY),
                (int)(SystemParameters.VirtualScreenWidth * dpi.DpiScaleX),
                (int)(SystemParameters.VirtualScreenHeight * dpi.DpiScaleY)
            );
        }
        RectI GetScreenRect() => GetScreenRect(GetDpi());

        DpiScale GetDpi() => VisualTreeHelper.GetDpi(this);

        private void ScrollHome()
        {
            var rect = GetScreenRect();
            // displays to the left of main have negative coords
            scrollview.ScrollToHorizontalOffset(Math.Abs(rect.X));
            scrollview.ScrollToVerticalOffset(Math.Abs(rect.Y));
        }

        private void RefreshCmd_Executed(object _, ExecutedRoutedEventArgs __)
        {
            var dpi = GetDpi();

            // hide window by moving it offscreen
            var bounds = RestoreBounds;
            Left = Top = int.MaxValue;
            
            preview.Source = CaptureScreen(dpi);

            Left = bounds.Left;
            Top = bounds.Top;
        }

        private void preview_MouseBtnClick(object sender, MouseButtonEventArgs e)
        {
            var image = (Image)sender;

            if (e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Right)
            {
                var pos = e.GetPosition(image);
                model.Color = GetPixel((BitmapSource)image.Source, (int)pos.X, (int)pos.Y);
                model.Footer = string.Format(translate("mousepos_fmt"), pos);
            }

        }
        private void preview_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.RoutedEvent == MouseLeaveEvent)
            {
                model.Footer = string.Format(translate("mousepos_fmt"), "OB");
            }
            else if (e.RoutedEvent == MouseMoveEvent && e.Timestamp - lastMouseTimestamp >= 100)
            {
                var pos = e.GetPosition((Image)sender);
                model.Footer = string.Format(translate("mousepos_fmt"), pos);
                lastMouseTimestamp = e.Timestamp;
            }
        }
        private void preview_MouseWheel(object _, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                double amount = zoomSlider.SmallChange;
                var command = e.Delta > 0 ? NavigationCommands.IncreaseZoom : NavigationCommands.DecreaseZoom;

                if (command.CanExecute(null, preview))
                    command.Execute(amount, preview);

                e.Handled = true;
            }
        }

        private void CopyCmd_Exec(object _, ExecutedRoutedEventArgs e)
        {
            var format = e.Parameter is ColorFormat f ? f : model.Format;

            var text = model.GetText(format);
            Clipboard.SetText(text);
            model.Footer = translate("Color copied") + ": [" + text + "]";
            e.Handled = true;
        }

        private void ZoomCmd_Exec(object _, ExecutedRoutedEventArgs e)
        {
            var amount = e.Parameter is double param ? param : zoomSlider.LargeChange;

            if (e.Command == NavigationCommands.IncreaseZoom)
            {
                model.Zoom += amount;
            }
            else if (e.Command == NavigationCommands.DecreaseZoom)
            {
                model.Zoom -= amount;
            }
        }
        private void ZoomCmd_CanExec(object _, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == NavigationCommands.IncreaseZoom)
            {
                e.CanExecute = model.Zoom < zoomSlider?.Maximum;
            }
            else if (e.Command == NavigationCommands.DecreaseZoom)
            {
                e.CanExecute = model.Zoom > zoomSlider?.Minimum;
            }
        }

        private void miAbout_Click(object _, RoutedEventArgs __)
        {
            var about = new AboutBox();
            about.ShowDialog();
        }

        private void miGoHome_Click(object _, RoutedEventArgs __) => ScrollHome();

        private void histClear_Click(object sender, RoutedEventArgs e)
        {
            histPopup.IsOpen = false;
            model.History.Clear();
            e.Handled = true;
        }

        private void histList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = (ListView)sender;
            if (list.HasItems)
            {
                var color = (Color)list.SelectedItem;
                model.Color = color;
            }

            e.Handled = true;
        }
    }

}
