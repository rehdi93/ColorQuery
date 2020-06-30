using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace ColorQuery
{
    public static class I18n
    {
        public static string translate(string text)
        {
            var result = Properties.Resources.ResourceManager.GetString(text);
            return string.IsNullOrEmpty(result) ? text : result;
        }
    }

    enum ColorFormat
    {
        RGB, CMYK, HEX
    }

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
