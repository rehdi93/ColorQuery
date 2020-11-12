using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Gdi = System.Drawing;
using static System.Windows.Interop.Imaging;
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

            previewImg.Source = CaptureScreen();
            ScrollHome();

            // set toolbar item tooltips
            var btnsWithCmds = tooltray.ToolBars.SelectMany(tb => tb.Items.OfType<ButtonBase>()).Where(b => b.Command is RoutedUICommand);
            foreach (var btn in btnsWithCmds)
            {
                var cmd = (RoutedUICommand)btn.Command;
                btn.ToolTip = cmd.Text;

                try
                {
                    var kg = cmd.InputGestures.OfType<KeyGesture>().First();
                    btn.ToolTip += " (" + kg.DisplayString + ")";
                }
                catch (Exception) {}
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

        double maxZoom { get => zoomSlider.Maximum; set => zoomSlider.Maximum = value; }
        double minZoom { get => zoomSlider.Minimum; set => zoomSlider.Minimum = value; }
        double smallZoomChange { get => zoomSlider.SmallChange; set => zoomSlider.SmallChange = value; }
        double bigZoomChange { get => zoomSlider.LargeChange; set => zoomSlider.LargeChange = value; }


        BitmapSource CaptureScreen(Rect area, DpiScale dpi)
        {
            var screenRect = new Int32Rect(
                (int)(area.X * dpi.DpiScaleX),
                (int)(area.Y * dpi.DpiScaleY),
                (int)(area.Width * dpi.DpiScaleX),
                (int)(area.Height * dpi.DpiScaleY)
            );

            using var bm = new Gdi.Bitmap(screenRect.Width, screenRect.Height);
            bm.SetResolution(96f, 96f);

            using var g = Gdi.Graphics.FromImage(bm);

            g.SmoothingMode = Gdi.Drawing2D.SmoothingMode.HighQuality;
            g.InterpolationMode = Gdi.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = Gdi.Drawing2D.PixelOffsetMode.HighQuality;

            g.CopyFromScreen(screenRect.X, screenRect.Y, 0, 0, bm.Size);

            var hbitmap = bm.GetHbitmap();
            using var handle = new HBitmapHandle(hbitmap);
            return CreateBitmapSourceFromHBitmap(
                    hbitmap,
                    IntPtr.Zero,
                    new Int32Rect(0, 0, bm.Width, bm.Height),
                    BitmapSizeOptions.FromEmptyOptions());
        }
        BitmapSource CaptureScreen()
        {
            var (rect, dpi) = GetScreenInfo();
            return CaptureScreen(rect, dpi);
        }

        // https://stackoverflow.com/a/14508110
        Color GetPixel(BitmapSource image, Point p)
        {
            var crop = new CroppedBitmap(image, new Int32Rect((int)p.X, (int)p.Y, 1, 1));
            var pixelbuff = new byte[4]; // { blue, green, red, alpha }
            crop.CopyPixels(pixelbuff, 4, 0);
            return Color.FromRgb(pixelbuff[2], pixelbuff[1], pixelbuff[0]);
        }

        DpiScale GetDpi() => VisualTreeHelper.GetDpi(this);

        (Rect,DpiScale) GetScreenInfo()
        {
            var screen = new Rect(
                SystemParameters.VirtualScreenLeft,
                SystemParameters.VirtualScreenTop,
                SystemParameters.VirtualScreenWidth,
                SystemParameters.VirtualScreenHeight
            );
            var dpi = GetDpi();

            return (screen, dpi);
        }

        private void ScrollHome()
        {
            var (rect, dpi) = GetScreenInfo();
            var (X, Y) = (rect.X * dpi.DpiScaleX, rect.Y * dpi.DpiScaleY);

            // displays to the left of main have negative coords
            scrollview.ScrollToHorizontalOffset(Math.Abs(X));
            scrollview.ScrollToVerticalOffset(Math.Abs(Y));
        }


        private void RefreshCmd_Executed(object _, ExecutedRoutedEventArgs __)
        {
            var (rect, dpi) = GetScreenInfo();

            // hide window by moving it offscreen
            var bounds = RestoreBounds;
            Left = Top = int.MaxValue;

            previewImg.Source = CaptureScreen(rect, dpi);

            Left = bounds.Left;
            Top = bounds.Top;
        }

        private void previewImg_MouseBtnClick(object sender, MouseButtonEventArgs e)
        {
            var image = (Image)sender;

            if (e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Right)
            {
                var pos = e.GetPosition(image);
                model.Color = GetPixel((BitmapSource)image.Source, pos);
                model.Footer = string.Format(translate("Mouse pos") + ": {0:F0}", pos);
            }
        }
        private void previewImg_MouseMove(object sender, MouseEventArgs e)
        {
            var format = translate("Mouse pos") + ": {0:F0}";

            if (e.RoutedEvent == MouseLeaveEvent)
            {
                model.Footer = string.Format(format, "OB");
            }
            else if (e.RoutedEvent == MouseMoveEvent && e.Timestamp - lastMouseTimestamp >= 100)
            {
                var pos = e.GetPosition((Image)sender);
                model.Footer = string.Format(format, pos);
                lastMouseTimestamp = e.Timestamp;
            }
        }
        private void previewImg_MouseWheel(object _, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                var command = e.Delta > 0 ? NavigationCommands.IncreaseZoom : NavigationCommands.DecreaseZoom;

                if (command.CanExecute(null, previewImg))
                    command.Execute(smallZoomChange, previewImg);

                e.Handled = true;
            }
        }

        private void CopyCmd_Exec(object _, ExecutedRoutedEventArgs e)
        {
            var format = e.Parameter is ColorFormat f ? f : model.Format;

            var text = model.GetText(format);
            Clipboard.SetText(text);
            
            var msg = translate("Color copied") + ": [" + text + "]";
            model.Footer = msg;

            e.Handled = true;
        }

        private void ZoomCmd_Exec(object _, ExecutedRoutedEventArgs e)
        {
            var amount = e.Parameter is double param ? param : bigZoomChange;

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
                e.CanExecute = model.Zoom < maxZoom;
            }
            else if (e.Command == NavigationCommands.DecreaseZoom)
            {
                e.CanExecute = model.Zoom > minZoom;
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
