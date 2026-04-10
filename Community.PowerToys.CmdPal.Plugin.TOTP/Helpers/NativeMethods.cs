using System;
using System.Runtime.InteropServices;

namespace Community.PowerToys.CmdPal.Plugin.TOTP {
    internal partial class NativeMethods {
        [LibraryImport("user32.dll")]
        internal static partial IntPtr GetForegroundWindow();

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);


        [LibraryImport("gdi32.dll")]
        internal static partial int GetDeviceCaps(IntPtr hdc, int nIndex);


        [LibraryImport("Microsoft.ui.xaml.dll")]
        internal static partial void XamlCheckProcessRequirements();
    }
}
