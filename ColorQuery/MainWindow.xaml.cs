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
            Resources.Remove("mockModel");

            NavigationCommands.IncreaseZoom.InputGestures.Add(new KeyGesture(Key.OemPlus, ModifierKeys.Control, "Ctrl++"));
            NavigationCommands.DecreaseZoom.InputGestures.Add(new KeyGesture(Key.OemMinus, ModifierKeys.Control, "Ctrl+-"));

            // HACK: SystemParameters.VirtualScreen* values aren't updated with dpi changes,
            // 'physical_left == VirtualScreenLeft * dpiScale' ONLY if dpiScale is the dpiScale of the main monitor 😢
            // calculate the desktop area using the current dpi
            var screenRect = new Rect(
                SystemParameters.VirtualScreenLeft,
                SystemParameters.VirtualScreenTop,
                SystemParameters.VirtualScreenWidth,
                SystemParameters.VirtualScreenHeight
            );
            desktopRect = screenRect;

            desktopDpi = VisualTreeHelper.GetDpi(this);
            var dpiScale = new Vector(desktopDpi.DpiScaleX, desktopDpi.DpiScaleY);
            
            desktopRect.Scale(dpiScale.X, dpiScale.Y);
            desktopRect.Offset(desktopRect.Left * dpiScale.X, desktopRect.Top * dpiScale.Y);

            previewImg.Source = CaptureScreen(desktopRect);
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
                catch (Exception)
                {
                    System.Diagnostics.Debug.Print("toolbar: {0} has no KeyGestures.", cmd.Name);
                }
            }

            miGoHome.ToolTip = ComponentCommands.MoveToHome.Text;
            tbZoom.ToolTip = NavigationCommands.Zoom.Text;

            var ctxm = (ContextMenu)Resources["ctxmColorCopy"];
            ctxm.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, CopyCmd_Exec));

            cbFormatSelect.ItemsSource = Enum.GetValues(typeof(ColorFormat));
        }

        Rect desktopRect;
        DpiScale desktopDpi;
        Point zoomCenter = new Point(); // relative to scrollview
        int lastMouseTimestamp = 0;

        double maxZoom { get => zoomSlider.Maximum; set => zoomSlider.Maximum = value; }
        double minZoom { get => zoomSlider.Minimum; set => zoomSlider.Minimum = value; }
        double smallZoomChange { get => zoomSlider.SmallChange; set => zoomSlider.SmallChange = value; }
        double bigZoomChange { get => zoomSlider.LargeChange; set => zoomSlider.LargeChange = value; }


        BitmapSource CaptureScreen(Rect physicalArea)
        {
            System.Diagnostics.Debug.Print($"CaptureScreen: {physicalArea}");
            
            var screenRect = new Int32Rect(
                (int)physicalArea.X,
                (int)physicalArea.Y,
                (int)physicalArea.Width,
                (int)physicalArea.Height
            );

            using var bm = new Gdi.Bitmap(screenRect.Width, screenRect.Height);
            using var g = Gdi.Graphics.FromImage(bm);

            g.SmoothingMode = Gdi.Drawing2D.SmoothingMode.HighQuality;
            g.InterpolationMode = Gdi.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = Gdi.Drawing2D.PixelOffsetMode.HighQuality;

            g.CopyFromScreen(screenRect.X, screenRect.Y, 0, 0, bm.Size, Gdi.CopyPixelOperation.SourceCopy);

            var hbitmap = bm.GetHbitmap();
            using var handle = new HBitmapHandle(hbitmap);
            return CreateBitmapSourceFromHBitmap(
                    hbitmap,
                    IntPtr.Zero,
                    new Int32Rect(0, 0, bm.Width, bm.Height),
                    BitmapSizeOptions.FromEmptyOptions());
        }

        Color GetPixelColor(BitmapSource image, Point p)
        {
            var crop = new CroppedBitmap(image, new Int32Rect((int)p.X, (int)p.Y, 1, 1));
            var pixelbuff = new byte[4]; // { blue, green, red, alpha }
            crop.CopyPixels(pixelbuff, 4, 0);
            return Color.FromRgb(pixelbuff[2], pixelbuff[1], pixelbuff[0]);
        }

        private void ScrollHome()
        {
            var (X, Y) = (desktopRect.X, desktopRect.Y);

            // displays to the left of main have negative coords
            scrollview.ScrollToHorizontalOffset(Math.Abs(X));
            scrollview.ScrollToVerticalOffset(Math.Abs(Y));
        }


        private void RefreshCmd_Executed(object _, ExecutedRoutedEventArgs __)
        {
            // hide window by moving it offscreen
            var bounds = RestoreBounds;
            Left = Top = int.MaxValue;

            previewImg.Source = CaptureScreen(desktopRect);

            Left = bounds.Left;
            Top = bounds.Top;
        }

        private void previewImg_MouseBtnClick(object sender, MouseButtonEventArgs e)
        {
            var image = (Image)sender;

            if (e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Right)
            {
                var pos = e.GetPosition(image);
                model.Color = GetPixelColor((BitmapSource)image.Source, pos);
                model.Position = pos;

                zoomCenter = e.GetPosition((ScrollViewer)image.Parent);
            }
        }
        private void previewImg_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.RoutedEvent == MouseLeaveEvent)
            {
                var pos = zoomCenter + new Vector(scrollview.HorizontalOffset, scrollview.VerticalOffset);
                model.Position = pos;
            }
            else if (e.RoutedEvent == MouseMoveEvent && e.Timestamp - lastMouseTimestamp > 100)
            {
                var pos = e.GetPosition(previewImg);
                model.Position = pos;
                lastMouseTimestamp = e.Timestamp;
            }
        }
        private void previewImg_MouseWheel(object _, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                var command = e.Delta > 0 ? NavigationCommands.IncreaseZoom : NavigationCommands.DecreaseZoom;

                if (command.CanExecute(null, previewImg))
                {
                    command.Execute(smallZoomChange, previewImg);
                }

                e.Handled = true;
            }
        }

        private void CopyCmd_Exec(object _, ExecutedRoutedEventArgs e)
        {
            var format = e.Parameter is ColorFormat f ? f : model.Format;

            var text = model.GetText(format);
            Clipboard.SetText(text);
            
            model.Footer = translate("Color copied") + "!";
            Task.Delay(2000).ContinueWith(t => model.Footer = "");
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
            else if (e.Command == NavigationCommands.Zoom)
            {
                model.Zoom = (double)e.Parameter;
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
        }

        private void histList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = (ListView)sender;
            if (list.HasItems)
            {
                var color = (Color)list.SelectedItem;
                model.Color = color;
            }
        }

        private void scrollview_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // keep scroll position relative to last mouse click pos.
            var oldSize = new Size(e.ExtentWidth - e.ExtentWidthChange, e.ExtentHeight - e.ExtentHeightChange);
            e.Handled = oldSize == new Size() || (e.ExtentWidthChange == 0 && e.ExtentHeightChange == 0);

            if (e.Handled)
                return;

            var sv = (ScrollViewer)sender;
            var content = (Image)sv.Content;

            if (content.IsMouseDirectlyOver)
            {
                zoomCenter = Mouse.GetPosition(sv);
            }

            Point mpos = zoomCenter;

            var offset = new Vector(e.HorizontalOffset, e.VerticalOffset) + mpos;
            var relpos = new Vector(offset.X / oldSize.Width, offset.Y / oldSize.Height);

            offset.X = Math.Max(relpos.X * e.ExtentWidth - mpos.X, 0);
            offset.Y = Math.Max(relpos.Y * e.ExtentHeight - mpos.Y, 0);

            sv.ScrollToHorizontalOffset(offset.X);
            sv.ScrollToVerticalOffset(offset.Y);
        }
    }
}
