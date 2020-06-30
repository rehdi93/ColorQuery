using System;
using System.Windows.Markup;

namespace ColorQuery.Properties
{
    public class I18nExtension : MarkupExtension
    {
        public string Text { get; set; }

        public I18nExtension(string txt)
        {
            Text = txt;
        }
        public I18nExtension() {}

        public override object ProvideValue(IServiceProvider _)
        {
            try
            {
                return I18n.translate(Text);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Print("I18n error: '{0}'", e.Message);
                return Text;
            }
        }
    }
}
