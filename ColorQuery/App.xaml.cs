using System;
using System.Linq;
using System.Diagnostics;
using System.Windows;
using System.Globalization;
using Res = ColorQuery.Properties.Resources;

namespace ColorQuery
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // override language
            foreach (var name in new[] { "LANG", "LC_CTYPE", "LC_ALL" })
            {
                var lang = Environment.GetEnvironmentVariable(name);
                if (lang == null) continue;

                try
                {
                    // .net doesn't understand encoding suffix (pt-br.uf8)
                    int suffix = lang.LastIndexOf('.');
                    if (suffix != -1)
                    {
                        lang = lang.Substring(0, suffix);
                    }

                    CultureInfo.CurrentUICulture = new CultureInfo(lang);
                }
                catch (CultureNotFoundException ex)
                {
                    Trace.WriteLine(ex.Message);
                }
            }
        }
    }
}
