using System;
using System.Runtime.InteropServices;

namespace Various.Utils;

public static partial class NativeUtils
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Rect
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    internal struct Point
    {
        public int x;
        public int y;

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
    
    [StructLayout(LayoutKind.Sequential)]
    internal struct AppBarData
    {
        public int cbSize;
        public IntPtr hWnd;
        public int uCallbackMessage;
        public int uEdge;
        public Rect rc;
        public IntPtr lParam;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    internal struct MonitorInfo
    {
        public int cbSize;
        public Rect rcMonitor;
        public Rect rcWork;
        public uint dwFlags;
    }
}