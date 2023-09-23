using System.Runtime.InteropServices;

namespace Various.Utils;

public static partial class NativeUtils
{
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("06152247-6f50-465a-9245-118bfd3b6007")]
    internal interface ID2D1Factory
    {
        int ReloadSystemMetrics();

        [PreserveSig]
        void GetDesktopDpi(out float dpiX, out float dpiY);
    }
}