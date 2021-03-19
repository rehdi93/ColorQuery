using System;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using static ColorQuery.Resources.I18n;

namespace ColorQuery
{
    /// <summary>
    /// Interaction logic for AboutBox.xaml
    /// </summary>
    public partial class AboutBox : Window
    {
        public AboutBox()
        {
            InitializeComponent();

            var assembly = Assembly.GetExecutingAssembly();
            var attributes = assembly.GetCustomAttributes();
            var assbname = assembly.GetName();

            Title = translate("About") + " " + assbname.Name;
            txtProductName.Text = attributes.OfType<AssemblyProductAttribute>().FirstOrDefault()?.Product;
            txtVersion.Text = assbname.Version.ToString();
            txtCopyright.Text = attributes.OfType<AssemblyCopyrightAttribute>().FirstOrDefault()?.Copyright;

            btnOk.Click += delegate { Close(); };
            btnOk.Content = ApplicationCommands.Close.Text;

            weblink.RequestNavigate += (_, e) => {
                var psi = new ProcessStartInfo(e.Uri.ToString()) {
                    UseShellExecute = true
                };
                Process.Start(psi);
                e.Handled = true;
            };
        }
    }
}
