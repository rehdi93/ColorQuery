using System;
using System.Resources;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace ColorQuery
{
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
