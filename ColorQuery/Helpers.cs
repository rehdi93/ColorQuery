using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace ColorQuery
{
    class HBitmapHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        [System.Security.SecurityCritical]
        public HBitmapHandle(IntPtr handle, bool owns = true) : base(owns)
        {
            SetHandle(handle);
        }

        public IntPtr Get() => handle;

        protected override bool ReleaseHandle()
        {
            return DeleteObject(handle);
        }

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DeleteObject(IntPtr hObject);
    }
}
