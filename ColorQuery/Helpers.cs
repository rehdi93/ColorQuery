using System;
using System.Resources;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace ColorQuery
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
                Console.Error.WriteLine("translate(\"{0}\") error: {1}", text, e.Message);
                return text;
            }
        }
    }

    enum ColorFormat { RGB, HEX, CMYK }

    class HBitmapHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        [System.Security.SecurityCritical]
        public HBitmapHandle(IntPtr handle, bool owns = true) : base(owns)
        {
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            return DeleteObject(handle);
        }

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DeleteObject(IntPtr hObject);
    }
}
