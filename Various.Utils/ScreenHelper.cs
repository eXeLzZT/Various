using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Various.Utils;

public class ScreenHelper
{
    private const int PrimaryMonitor = unchecked((int)0xBAADF00D);
    private const int MonitorInfoFPrimary = 0x00000001;

    private static readonly bool MultiMonitorSupport;

    private readonly IntPtr _monitorHandle;

    public static IEnumerable<ScreenHelper> AllScreens
    {
        get
        {
            if (MultiMonitorSupport)
            {
                var closure = new NativeUtils.MonitorEnumCallback();
                var proc = new NativeUtils.MonitorEnumProc(closure.Callback);
                NativeUtils.EnumDisplayMonitors(IntPtr.Zero, null, proc, IntPtr.Zero);

                if (closure.Screens.Count > 0)
                {
                    return closure.Screens.Cast<ScreenHelper>();
                }
            }

            return new[] { new ScreenHelper(PrimaryMonitor) };
        }
    }

    public static ScreenHelper? PrimaryScreen =>
        MultiMonitorSupport ? AllScreens.FirstOrDefault(s => s.Primary) : new ScreenHelper(PrimaryMonitor);

    public Rect WorkingArea
    {
        get
        {
            Rect workingArea;

            if (!MultiMonitorSupport || _monitorHandle == PrimaryMonitor)
            {
                var rc = new NativeUtils.Rect();
                NativeUtils.SystemParametersInfo(
                    NativeUtils.Spi.GetWorkArea, 0, ref rc, NativeUtils.SpiF.SendChange);

                workingArea = new Rect(rc.left, rc.top, rc.right - rc.left, rc.bottom - rc.top);
            }
            else
            {
                var monitorInfo = new NativeUtils.MonitorInfo();
                monitorInfo.cbSize = Marshal.SizeOf(monitorInfo);
                
                NativeUtils.GetMonitorInfo(_monitorHandle, ref monitorInfo);

                workingArea =
                    new Rect(
                        monitorInfo.rcWork.left,
                        monitorInfo.rcWork.top,
                        monitorInfo.rcWork.right - monitorInfo.rcWork.left,
                        monitorInfo.rcWork.bottom - monitorInfo.rcWork.top);
            }

            return workingArea;
        }
    }

    public Rect WpfBounds =>
        ScaleFactor.Equals(1.0)
            ? Bounds
            : new Rect(
                Bounds.X / ScaleFactor,
                Bounds.Y / ScaleFactor,
                Bounds.Width / ScaleFactor,
                Bounds.Height / ScaleFactor);

    public Rect WpfWorkingArea =>
        ScaleFactor.Equals(1.0)
            ? WorkingArea
            : new Rect(
                WorkingArea.X / ScaleFactor,
                WorkingArea.Y / ScaleFactor,
                WorkingArea.Width / ScaleFactor,
                WorkingArea.Height / ScaleFactor);

    public string DeviceName { get; }
    public Rect Bounds { get; }
    public bool Primary { get; }
    public double ScaleFactor { get; }

    static ScreenHelper()
    {
        MultiMonitorSupport = NativeUtils.GetSystemMetrics(NativeUtils.SystemMetric.CMonitors) != 0;
    }

    internal ScreenHelper(IntPtr monitorHandle)
    {
        if (NativeUtils.IsProcessDPIAware())
        {
            uint dpiX;

            try
            {
                if (monitorHandle == PrimaryMonitor)
                {
                    var ptr =
                        NativeUtils.MonitorFromPoint(
                            new NativeUtils.Point(0, 0), NativeUtils.MonitorDefaultTo.Primary);
                    NativeUtils.GetDpiForMonitor(ptr, NativeUtils.DpiType.Effective, out dpiX, out _);
                }
                else
                {
                    NativeUtils.GetDpiForMonitor(monitorHandle, NativeUtils.DpiType.Effective, out dpiX, out _);
                }
            }
            catch
            {
                var hr =
                    NativeUtils.D2D1CreateFactory(
                        NativeUtils.D2D1FactoryType.SingleThreaded,
                        typeof(NativeUtils.ID2D1Factory).GUID,
                        IntPtr.Zero,
                        out var d2D1Factory);

                if (hr < 0)
                {
                    dpiX = 96;
                }
                else
                {
                    d2D1Factory.GetDesktopDpi(out var x, out _);
                    Marshal.ReleaseComObject(d2D1Factory);
                    dpiX = (uint)x;
                }
            }

            ScaleFactor = dpiX / 96.0;
        }

        if (!MultiMonitorSupport || monitorHandle == PrimaryMonitor)
        {
            var size =
                new Size(
                    NativeUtils.GetSystemMetrics(NativeUtils.SystemMetric.CxScreen),
                    NativeUtils.GetSystemMetrics(NativeUtils.SystemMetric.CyScreen));

            Bounds = new Rect(0, 0, size.Width, size.Height);
            Primary = true;
            DeviceName = "DISPLAY";
        }
        else
        {
            var monitorInfo = new NativeUtils.MonitorInfo();
            monitorInfo.cbSize = Marshal.SizeOf(monitorInfo);

            NativeUtils.GetMonitorInfo(monitorHandle, ref monitorInfo);

            Bounds =
                new Rect(
                    monitorInfo.rcMonitor.left,
                    monitorInfo.rcMonitor.top,
                    monitorInfo.rcMonitor.right - monitorInfo.rcMonitor.left,
                    monitorInfo.rcMonitor.bottom - monitorInfo.rcMonitor.top);
            Primary = (monitorInfo.dwFlags & MonitorInfoFPrimary) != 0;
            // DeviceName = new string(monitorInfo.szDevice).TrimEnd((char)0);
        }

        _monitorHandle = monitorHandle;
    }

    public static ScreenHelper FromHandle(IntPtr hWnd)
    {
        return MultiMonitorSupport
            ? new ScreenHelper(
                NativeUtils.MonitorFromWindow(
                    hWnd, NativeUtils.MonitorDefaultTo.Nearest))
            : new ScreenHelper(PrimaryMonitor);
    }

    public static ScreenHelper FromPoint(Point point)
    {
        if (MultiMonitorSupport)
        {
            var nativePoint = new NativeUtils.Point((int)point.X, (int)point.Y);
            return new ScreenHelper(
                NativeUtils.MonitorFromPoint(nativePoint, NativeUtils.MonitorDefaultTo.Nearest));
        }

        return new ScreenHelper(PrimaryMonitor);
    }

    public static ScreenHelper FromWindow(Window window)
    {
        return FromHandle(new WindowInteropHelper(window).Handle);
    }

    public override bool Equals(object? obj)
    {
        if (obj is ScreenHelper screen)
        {
            if (_monitorHandle == screen._monitorHandle)
            {
                return true;
            }
        }

        return false;
    }

    public override int GetHashCode()
    {
        return _monitorHandle.GetHashCode();
    }
}