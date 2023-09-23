using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace Various.Utils;

public static class AppBarUtils
{
    private delegate void ResizeDelegate(Window appbarWindow, Rect rect);

    private static readonly Dictionary<Window, RegisterInfo> RegisteredInfos;

    static AppBarUtils()
    {
        RegisteredInfos = new Dictionary<Window, RegisterInfo>();
    }

    public static void Register(Window appBar, AppBarEdge edge = AppBarEdge.Top, bool topmost = true)
    {
        var registerInfo = GetRegisterInfo(appBar);

        if (registerInfo.IsRegistered)
            return;

        var callbackId = NativeUtils.RegisterWindowMessage("AppBarMessage");
        
        registerInfo.IsRegistered = true;
        registerInfo.Edge = edge;
        registerInfo.CallbackId = callbackId;
        
        var appBarData = new NativeUtils.AppBarData();
        appBarData.cbSize = Marshal.SizeOf(appBarData);
        appBarData.hWnd = new WindowInteropHelper(appBar).Handle;
        appBarData.uCallbackMessage = callbackId;
        
        NativeUtils.SHAppBarMessage((int)NativeUtils.AppBarMessage.New, ref appBarData);

        var hWndSource = HwndSource.FromHwnd(appBarData.hWnd);
        hWndSource?.AddHook(registerInfo.WndProc);

        appBar.WindowStyle = WindowStyle.None;
        appBar.ResizeMode = ResizeMode.NoResize;
        appBar.Topmost = topmost;

        var renderPolicy = (int)NativeUtils.DwmNcRenderingPolicy.Enabled;

        NativeUtils.DwmSetWindowAttribute(
            appBarData.hWnd,
            (int)NativeUtils.DwmWindowAttribute.ExcludedFromPeek,
            ref renderPolicy,
            sizeof(int));

        NativeUtils.DwmSetWindowAttribute(
            appBarData.hWnd,
            (int)NativeUtils.DwmWindowAttribute.DisallowPeek,
            ref renderPolicy,
            sizeof(int));

        SetPosition(registerInfo, appBar);
    }

    public static void Unregister(Window appBar)
    {
        var registerInfo = GetRegisterInfo(appBar);

        if (!registerInfo.IsRegistered)
            return;

        var appBarData = new NativeUtils.AppBarData();
        appBarData.cbSize = Marshal.SizeOf(appBarData);
        appBarData.uCallbackMessage = registerInfo.CallbackId;
        appBarData.hWnd = new WindowInteropHelper(appBar).Handle;

        NativeUtils.SHAppBarMessage((int)NativeUtils.AppBarMessage.Remove, ref appBarData);

        registerInfo.IsRegistered = false;

        RestoreWindow(appBar);

        var renderPolicy = (int)NativeUtils.DwmNcRenderingPolicy.UseWindowStyle;

        NativeUtils.DwmSetWindowAttribute(
            appBarData.hWnd,
            (int)NativeUtils.DwmWindowAttribute.ExcludedFromPeek,
            ref renderPolicy,
            sizeof(int));

        NativeUtils.DwmSetWindowAttribute(
            appBarData.hWnd,
            (int)NativeUtils.DwmWindowAttribute.DisallowPeek,
            ref renderPolicy,
            sizeof(int));
    }

    public static void SetPosition(RegisterInfo registerInfo, Window appBar)
    {
        var edge = registerInfo.Edge;
        var appBarData = new NativeUtils.AppBarData();
        appBarData.cbSize = Marshal.SizeOf(appBarData);
        appBarData.hWnd = new WindowInteropHelper(appBar).Handle;
        appBarData.uEdge = (int)edge;

        var toPixel = PresentationSource.FromVisual(appBar)?.CompositionTarget?.TransformToDevice ?? Matrix.Identity;
        var toWpfUnit = PresentationSource.FromVisual(appBar)?.CompositionTarget?.TransformFromDevice ??
                        Matrix.Identity;

        var sizeInPixels = toPixel.Transform(new Vector(appBar.ActualWidth, appBar.ActualHeight));

        var actualWorkArea = GetActualWorkArea(registerInfo);
        var screenSizeInPixels =
            toPixel.Transform(new Vector(actualWorkArea.Width, actualWorkArea.Height));
        var workTopLeftInPixels =
            toPixel.Transform(new Point(actualWorkArea.Left, actualWorkArea.Top));
        var workAreaInPixelsF = new Rect(workTopLeftInPixels, screenSizeInPixels);

        if (appBarData.uEdge is (int)AppBarEdge.Left or (int)AppBarEdge.Right)
        {
            appBarData.rc.top = (int)workAreaInPixelsF.Top;
            appBarData.rc.bottom = (int)workAreaInPixelsF.Bottom;

            if (appBarData.uEdge == (int)AppBarEdge.Left)
            {
                appBarData.rc.left = (int)workAreaInPixelsF.Left;
                appBarData.rc.right = appBarData.rc.left + (int)Math.Round(sizeInPixels.X);
            }
            else
            {
                appBarData.rc.right = (int)workAreaInPixelsF.Right;
                appBarData.rc.left = appBarData.rc.right - (int)Math.Round(sizeInPixels.X);
            }
        }
        else
        {
            appBarData.rc.left = (int)workAreaInPixelsF.Left;
            appBarData.rc.right = (int)workAreaInPixelsF.Right;
            
            if (appBarData.uEdge == (int)AppBarEdge.Top)
            {
                appBarData.rc.top = (int)workAreaInPixelsF.Top;
                appBarData.rc.bottom = appBarData.rc.top + (int)Math.Round(sizeInPixels.Y);
            }
            else
            {
                appBarData.rc.bottom = (int)workAreaInPixelsF.Bottom;
                appBarData.rc.top = appBarData.rc.bottom - (int)Math.Round(sizeInPixels.Y);
            }
        }

        NativeUtils.SHAppBarMessage((int)NativeUtils.AppBarMessage.QueryPos, ref appBarData);
        NativeUtils.SHAppBarMessage((int)NativeUtils.AppBarMessage.SetPos, ref appBarData);

        var location = toWpfUnit.Transform(new Point(appBarData.rc.left, appBarData.rc.top));
        var dimension =
            toWpfUnit.Transform(
                new Vector(appBarData.rc.right - appBarData.rc.left, appBarData.rc.bottom - appBarData.rc.top));

        var rect = new Rect(location, new Size(dimension.X, dimension.Y));
        registerInfo.DockedSize = rect;

        appBar.Dispatcher.BeginInvoke(
            DispatcherPriority.ApplicationIdle, new ResizeDelegate(DoResize), appBar, rect);
    }

    private static void RestoreWindow(Window appBar)
    {
        var registerInfo = GetRegisterInfo(appBar);

        appBar.WindowStyle = registerInfo.OriginalStyle;
        appBar.ResizeMode = registerInfo.OriginalResizeMode;
        appBar.Topmost = registerInfo.OriginalTopmost;

        registerInfo.DockedSize = null;

        var rect = 
            new Rect(
                registerInfo.OriginalPosition.X, 
                registerInfo.OriginalPosition.Y, 
                registerInfo.OriginalSize.Width,
                registerInfo.OriginalSize.Height);
        
        appBar.Dispatcher.BeginInvoke(
            DispatcherPriority.ApplicationIdle, new ResizeDelegate(DoResize), appBar, rect);
    }

    private static void DoResize(Window appBar, Rect rect)
    {
        appBar.Width = rect.Width;
        appBar.Height = rect.Height;
        appBar.Top = rect.Top;
        appBar.Left = rect.Left;
    }

    private static RegisterInfo GetRegisterInfo(Window appBar)
    {
        if (!RegisteredInfos.TryGetValue(appBar, out var registerInfo))
        {
            registerInfo = new RegisterInfo
            {
                CallbackId = 0,
                Window = appBar,
                IsRegistered = false,
                Edge = AppBarEdge.Top,
                OriginalStyle = appBar.WindowStyle,
                OriginalPosition = new Point(appBar.Left, appBar.Top),
                OriginalSize = new Size(appBar.ActualWidth, appBar.ActualHeight),
                OriginalResizeMode = appBar.ResizeMode,
                OriginalTopmost = appBar.Topmost,
                DockedSize = null
            };

            RegisteredInfos.Add(appBar, registerInfo);
        }

        return registerInfo;
    }

    private static Rect GetActualWorkArea(RegisterInfo registerInfo)
    {
        var hWnd = new WindowInteropHelper(registerInfo.Window).Handle;
        var cwa = GetMonitorWorkArea(NativeUtils.MonitorFromWindow(hWnd, NativeUtils.MonitorDefaultTo.Nearest));
        var wa = new Rect(new Point(cwa.left, cwa.top), new Point(cwa.right, cwa.bottom));

        if (registerInfo.DockedSize != null)
        {
            wa.Union(registerInfo.DockedSize.Value);
        }

        return wa;
    }

    private static NativeUtils.Rect GetMonitorWorkArea(IntPtr hMonitor)
    {
        var monitorInfo = new NativeUtils.MonitorInfo();
        monitorInfo.cbSize = Marshal.SizeOf(monitorInfo);
        NativeUtils.GetMonitorInfo(hMonitor, ref monitorInfo);
        return monitorInfo.rcWork;
    }
}