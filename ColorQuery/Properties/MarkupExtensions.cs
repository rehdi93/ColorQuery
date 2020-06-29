using System;
using System.Windows.Markup;

namespace ColorQuery.Properties
{
    public class ResxExtension : MarkupExtension
    {
        public string Key { get; set; }

        public ResxExtension(string key)
        {
            Key = key;
        }
        public ResxExtension() { }

        public override object ProvideValue(IServiceProvider _)
        {
            string result;

            try
            {
                result = Resources.ResourceManager.GetString(Key);
                if (string.IsNullOrEmpty(result))
                    result = Key;
            }
            catch (ArgumentNullException)
            {
                result = "NULL_ID";
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Print("Resx error: '{0}'", e.Message);
                result = Key;
            }

            return result;
        }
    }

    public class I18nExtension : MarkupExtension
    {
        public string Text { get; set; }

        public override object ProvideValue(IServiceProvider _)
        {
            try
            {
                var result = Resources.ResourceManager.GetString(Text);
                return string.IsNullOrEmpty(result) ? Text : result;
            }
            catch (ArgumentNullException)
            {
                return null;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Print("Resx error: '{0}'", e.Message);
                return Text;
            }
        }
    }
}
