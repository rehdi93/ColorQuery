using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows;
using System.Text.RegularExpressions;
using Debug = System.Diagnostics.Debug;


namespace ColorQuery.Properties
{
    //https://putridparrot.com/blog/localizing-a-wpf-application-using-dynamic-resources/
    internal static class Locale
    {
        public static void LoadTextResources(CultureInfo culture)
        {
            var resources = new ResourceDictionary();

            var locations = new string[]
            {
                $"/Resources/text.{culture.Name}.xaml",
                $"/Resources/text.{culture.Parent.Name}.xaml",
                //"Resources/text.xaml" /* already loaded in App.xaml */
            };
            bool loaded = false;

            foreach (var loc in locations)
            {
                try
                {
                    resources.Source = new Uri(loc, UriKind.Relative);
                    loaded = true;
                    break;
                }
                catch (System.IO.IOException)
                {
                    Debug.WriteLine($"'{loc}' not found.", "Locale");
                    continue;
                }
                catch
                {
                    throw;
                }
            }

            if (!loaded)
            {
                Debug.WriteLine($"Could not find text resources for '{culture.Name}'. Using fallback.", "Locale");
                return;
            }

            Regex rx = new Regex(@"text\.\w\w(-\w\w)?\.xaml");
            var current = App.Current.Resources.MergedDictionaries.FirstOrDefault(m => rx.IsMatch(m.Source.OriginalString));

            if (current != null)
            {
                App.Current.Resources.MergedDictionaries.Remove(current);
            }

            App.Current.Resources.MergedDictionaries.Add(resources);

            Debug.WriteLine($"Loaded '{resources["reslocale"]}' text resources successfully.", "Locale");
        }

        public static void LoadTextResources(string locale) => LoadTextResources(new CultureInfo(locale));
        public static void LoadTextResources() => LoadTextResources(CultureInfo.CurrentCulture);
    }
}
