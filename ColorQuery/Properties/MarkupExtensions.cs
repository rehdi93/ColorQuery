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
}
