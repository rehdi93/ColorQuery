using System;
using System.Linq;
using System.Diagnostics;
using System.Windows;
using System.Globalization;

namespace ColorQuery
{
    using Env = Environment;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // override language
            try
            {
                var lang = new[] { "LANG", "LC_CTYPE", "LC_ALL" }.Select(Env.GetEnvironmentVariable).First(v => v != null);
                
                // .net doesn't understand encoding suffix (pt-br.uf8)
                int suffix = lang.LastIndexOf('.');
                if (suffix != -1)
                    lang = lang.Substring(0, suffix);

                CultureInfo.CurrentUICulture = new CultureInfo(lang);
            }
            catch (InvalidOperationException)
            {
                // no lang override found
            }
            catch (CultureNotFoundException ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
    }
}
