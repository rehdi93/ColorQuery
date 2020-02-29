﻿using System;

namespace ColorQuery.Properties
{
    public class ResxExtension : System.Windows.Markup.MarkupExtension
    {
        public string Key { get; set; }

        public ResxExtension(string key)
        {
            Key = key;
        }
        public ResxExtension() { }

        public override object ProvideValue(IServiceProvider _)
        {
            if (Key == null) return null;
            string result;

            try
            {
                result = Resources.ResourceManager.GetString(Key);
                if (string.IsNullOrEmpty(result))
                {
                    result = Key;
                }
            }
            catch (ArgumentNullException)
            {
                result = "NULL";
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
