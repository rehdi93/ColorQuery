﻿using System;
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
            LangOverride(e.Args);
            base.OnStartup(e);
        }

        void LangOverride(string[] args)
        {
            string lang;
            CultureInfo culture = null;
            var sep = new[] { '=' };

            // via cmdline
            // cmd> ColorQuery lang=pt-br
            foreach (var arg in args)
            {
                var sp = arg.Split(sep, 2, StringSplitOptions.RemoveEmptyEntries);
                (string key, string value) pair;

                if (sp.Length < 2)
                    continue;
                else
                    pair = (sp[0], sp[1]);

                if (pair.key == "lang")
                {
                    lang = pair.value;
                    culture = new CultureInfo(StripEnc(lang));
                }
            }

            // or via env. variables
            if (culture == null)
            {
                try
                {
                    lang = new[] { "LC_ALL", "LC_CTYPE", "LANG" }.Select(Env.GetEnvironmentVariable).First(v => v != null);
                    culture = new CultureInfo(StripEnc(lang));
                }
                catch (InvalidOperationException)
                {
                    Debug.WriteLine("No environment override found.", nameof(LangOverride));
                }
                catch (CultureNotFoundException ex)
                {
                    Trace.WriteLine(ex.Message, nameof(LangOverride));
                }
            }

            if (culture != null)
            {
                Trace.WriteLine($"using {culture} for the Ui.", nameof(LangOverride));
                CultureInfo.CurrentUICulture = culture;
            }

            // .net doesn't understand encoding suffix (pt_BR.uf8)
            static string StripEnc(string l)
            {
                var dot = l.LastIndexOf('.');
                if (dot != -1)
                    return l.Substring(0, dot);
                else
                    return l;
            }
        }
    }
}
