using System;
using System.Runtime.InteropServices;

namespace Queueinator.Forms.Services
{
    public static class NativeWindowAPI
    {
        public static readonly int GWL_EXSTYLE = -20;
        public static readonly int WS_EX_COMPOSITE = 0x02000000;

        [DllImport("user32")]
        internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32")]
        internal static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}
