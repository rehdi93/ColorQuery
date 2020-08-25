using System;
using System.Resources;
using System.Diagnostics;
using System.Windows.Markup;

namespace ColorQuery.Resources
{
    public static class I18n
    {
        static readonly ResourceManager RM = new ResourceManager("ColorQuery.Resources.strings", typeof(App).Assembly);

        public static string translate(string text)
        {
            try
            {
                var result = RM.GetString(text);
                return string.IsNullOrEmpty(result) ? text : result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("translate(\"{0}\") error: {1}", text, e.Message);
                return text;
            }
        }
    }

    public class I18nExtension : MarkupExtension
    {
        public string Text { get; set; }

        public I18nExtension(string txt)
        {
            Text = txt;
        }
        public I18nExtension() { }

        public override object ProvideValue(IServiceProvider _)
        {
            return I18n.translate(Text);
        }
    }

}
