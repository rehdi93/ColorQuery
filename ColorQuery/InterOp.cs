using System;
using System.Runtime.InteropServices;

namespace ColorQuery
{
    internal class InterOp
    {
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr hObject);
    }
}
