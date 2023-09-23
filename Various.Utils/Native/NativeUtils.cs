using System;
using System.Runtime.InteropServices;

namespace Various.Utils;

public static partial class NativeUtils
{
    public delegate bool MonitorEnumProc(IntPtr monitor, IntPtr hdc, IntPtr lpRcMonitor, IntPtr lParam);
    
    [DllImport("shell32.dll", CallingConvention = CallingConvention.StdCall)]
    internal static extern uint SHAppBarMessage(int dwMessage, ref AppBarData pData);
    
    [DllImport("dwmapi.dll")]
    internal static extern int DwmSetWindowAttribute(IntPtr hWnd, int attr, ref int attrValue, int attrSize);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    internal static extern int GetSystemMetrics(SystemMetric nIndex);
    
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    internal static extern bool SystemParametersInfo(Spi nAction, int nParam, ref Rect rc, SpiF nUpdate);
    
    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool IsProcessDPIAware();
    
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    internal static extern int RegisterWindowMessage(string msg);
    
    [DllImport("user32.dll")]
    internal static extern IntPtr MonitorFromWindow(IntPtr hWnd, MonitorDefaultTo dwFlags);
    
    [DllImport("user32.dll")]
    internal static extern IntPtr MonitorFromPoint(Point pt, MonitorDefaultTo dwFlags);

    [DllImport("shcore.dll", CharSet = CharSet.Auto)]
    internal static extern IntPtr GetDpiForMonitor(IntPtr hMonitor, DpiType dpiType, out uint dpiX, out uint dpiY);
    
    [DllImport("user32.dll")]
    internal static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpMi);
    
    [DllImport("user32.dll")]
    internal static extern bool EnumDisplayMonitors(IntPtr hWnd, ComRect? rcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);
    
    [DllImport("d2d1.dll")]
    internal static extern int D2D1CreateFactory(D2D1FactoryType factoryType, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, IntPtr pFactoryOptions, out ID2D1Factory ppIFactory);
}