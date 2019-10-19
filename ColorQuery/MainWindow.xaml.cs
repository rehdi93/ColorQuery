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

namespace ColorQuery
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = m;
            InitializeComponent();
            preview.Source = CaptureScreen();
            m.PropertyChanged += Model_PropertyChanged;
            // ...
            gbZoom.Header = NavigationCommands.Zoom.Text;
        }

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }

        ColorQueryModel m = new ColorQueryModel();

        BitmapSource CaptureScreen()
        {
            BitmapSource screen;

            // get screen bitmap
            var dpi = VisualTreeHelper.GetDpi(this);
            var bm = new System.Drawing.Bitmap((int)(SystemParameters.VirtualScreenWidth * dpi.DpiScaleX),
                                               (int)(SystemParameters.VirtualScreenHeight * dpi.DpiScaleY));

            using (var g = System.Drawing.Graphics.FromImage(bm))
                g.CopyFromScreen(0, 0, 0, 0, bm.Size);

            // convert GDI bitmap into a WPF bitmap
            IntPtr handle = IntPtr.Zero;

            try
            {
                handle = bm.GetHbitmap();
                screen = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    handle, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (System.ComponentModel.Win32Exception)
            {
                return null;
            }
            finally
            {
                if (handle != IntPtr.Zero)
                    InterOp.DeleteObject(handle);
            }

            return screen;
        }
        
        // https://stackoverflow.com/a/14508110
        Color GetPixel(BitmapSource image, int x, int y)
        {
            if (image != null)
            {
                try
                {
                    var crop = new CroppedBitmap(image, new System.Windows.Int32Rect(x, y, 1, 1));
                    var pixelbuff = new byte[4]; // [0] blue, green, red, alpha [3]
                    crop.CopyPixels(pixelbuff, 4, 0);
                    return Color.FromRgb(pixelbuff[2], pixelbuff[1], pixelbuff[0]);
                }
                catch (Exception)
                {
                }
            }

            return Colors.Transparent;
        }

        
        private void RefreshCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // hide window, change window style so it doesn't play the minimize animation
            var wstyle = WindowStyle;
            WindowStyle = WindowStyle.ToolWindow;
            Hide();

            preview.Source = CaptureScreen();

            Show();
            WindowStyle = wstyle;
        }

        private void preview_MouseBtnClick(object sender, MouseButtonEventArgs e)
        {
            var image = (Image)sender;
            var bm = (BitmapSource)image.Source;

            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Released)
            {
                var pos = e.GetPosition(image);
                m.CurrentColor = GetPixel(bm, (int)pos.X, (int)pos.Y);
            }
        }
        private void preview_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.RoutedEvent.Name == MouseLeaveEvent.Name)
            {
                m.Status = FindResource("mouseposOB") as string;
            }
            else if (e.RoutedEvent.Name == MouseMoveEvent.Name)
            {
                var pos = e.GetPosition((Image)sender);
                m.Status = string.Format((string)FindResource("mousepos"), (int)pos.X, (int)pos.Y);
            }
        }

        private void CopyCmd_Exec(object _, ExecutedRoutedEventArgs e)
        {
            var element = (FrameworkElement)e.Source;

            if (element.Tag is ColorFormat fmt)
            {
                m.CurrentFormat = fmt;
            }

            Clipboard.SetText(m.Text);
            m.Status = FindResource("ui_ColorCopied") as string;
        }
        private void CopyCmd_CanExec(object _, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = m.CurrentColor != Colors.Transparent;
        }

        private void ZoomCmd_Exec(object _, ExecutedRoutedEventArgs e)
        {
            var zoomcmd = (RoutedCommand)e.Command;

            if (zoomcmd.Name == NavigationCommands.IncreaseZoom.Name)
            {
                //Slider.IncreaseLarge.Execute(null, zoomSlider);
                m.Zoom += zoomSlider.LargeChange;
            }
            else if (zoomcmd.Name == NavigationCommands.DecreaseZoom.Name)
            {
                //Slider.DecreaseLarge.Execute(null, zoomSlider);
                m.Zoom -= zoomSlider.LargeChange;
            }
            else if (zoomcmd.Name == NavigationCommands.Zoom.Name)
            {
                // ...
            }
        }

        private void CloseCmd_Exec(object _, ExecutedRoutedEventArgs __) => Close();

        private void onColorFmtChanged(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)e.Source;
            if (element.Tag is ColorFormat fmt)
            {
                m.CurrentFormat = fmt;
            }
        }

        private void miAbout_Click(object sender, RoutedEventArgs e)
        {
            var aboutbox = new AboutBox();
            aboutbox.ShowDialog();
        }

    }
}
