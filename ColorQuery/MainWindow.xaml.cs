﻿using System;
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

            // set toolbar item tooltips
            var toolmenuitems = tooltray.ToolBars
                .SelectMany(t => t.Items.Cast<object>())
                .Where(o => o is Menu).Cast<Menu>()
                .SelectMany(t => t.Items.Cast<object>())
                .Where(obj => obj is MenuItem mi && mi.Command != null);
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
        }


        ColorQueryModel m = new ColorQueryModel();


        BitmapSource CaptureScreen()
        {
            // get screen bitmap
            var dpi = VisualTreeHelper.GetDpi(this);
            int width = (int)(SystemParameters.VirtualScreenWidth * dpi.DpiScaleX),
                height = (int)(SystemParameters.VirtualScreenHeight * dpi.DpiScaleY);

            using var bm = new System.Drawing.Bitmap(width, height);
            bm.SetResolution(96f, 96f);

            using var g = System.Drawing.Graphics.FromImage(bm);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            g.CopyFromScreen(0, 0, 0, 0, bm.Size);

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
                var crop = new CroppedBitmap(image, new Int32Rect(x, y, 1, 1));
                var pixelbuff = new byte[4]; // [0] blue, green, red, alpha [3]
                crop.CopyPixels(pixelbuff, 4, 0);
                return Color.FromRgb(pixelbuff[2], pixelbuff[1], pixelbuff[0]);
            }
            catch (Exception)
            {
                return Colors.Transparent;
            }
        }

        
        private void RefreshCmd_Executed(object _, ExecutedRoutedEventArgs __)
        {
            // hide window by moving it offscreen
            var bounds = RestoreBounds;
            Left = -Width;
            Top = -Height;
            
            preview.Source = CaptureScreen();

            Left = bounds.Left;
            Top = bounds.Top;
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
                m.Status = string.Format(Res.mousepos_fmt, "OB");
            }
            else if (e.RoutedEvent.Name == MouseMoveEvent.Name)
            {
                var pos = e.GetPosition((Image)sender);
                m.Status = string.Format(Res.mousepos_fmt2, (int)pos.X, (int)pos.Y);
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
            m.Status = Res.ColorCopied;
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
                m.Zoom += zoomSlider.LargeChange;
            }
            else if (zoomcmd.Name == NavigationCommands.DecreaseZoom.Name)
            {
                m.Zoom -= zoomSlider.LargeChange;
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
                e.CanExecute = m.Zoom < zoomSlider?.Maximum;
            }
            else if (zoomcmd.Name == NavigationCommands.DecreaseZoom.Name)
            {
                e.CanExecute = m.Zoom > zoomSlider?.Minimum;
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
