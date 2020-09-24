using System;
using System.Collections.Generic;
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
            LangOverride();
            base.OnStartup(e);
        }

        void LangOverride()
        {
            var envvarsUI = new[] { "LC_ALL", "LC_CTYPE", "LANG" };

            try
            {
                var lang = envvarsUI.Select(Env.GetEnvironmentVariable).First(v => v != null);
                var culture = new CultureInfo(StripEnc(lang));
                CultureInfo.CurrentUICulture = culture;
                
                Trace.WriteLine($"using {culture} for the Ui.", nameof(LangOverride));
            }
            catch (InvalidOperationException)
            {
                // no lang override found
            }
            catch (CultureNotFoundException ex)
            {
                Trace.WriteLine(ex.Message, nameof(LangOverride));
            }

            // .net doesn't understand encoding suffix (pt_BR.uf8)
            static string StripEnc(string l)
            {
                var dot = l.LastIndexOf('.');
                if (dot != -1)
                    l = l.Substring(0, dot);
                return l;
            }
        }
    }
}
