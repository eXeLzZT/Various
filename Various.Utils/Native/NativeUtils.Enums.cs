using System;

namespace Various.Utils;

public static partial class NativeUtils
{
    internal enum Spi : uint
    {
        GetWorkArea = 0x0030
    }
    
    [Flags]
    internal enum SpiF
    {
        None = 0x00,
        UpdateIniFile = 0x01,
        SendChange = 0x02,
        SendWinIniChange = 0x02
    }
    
    internal enum DpiType
    {
        Effective,
        Angular,
        Raw
    }

    internal enum SystemMetric
    {
        CxScreen = 0,
        CyScreen = 1,
        XVirtualScreen = 76,
        YVirtualScreen = 77,
        CxVirtualScreen = 78,
        CyVirtualScreen = 79,
        CMonitors = 80
    }
    
    internal enum MonitorDefaultTo
    {
        Null,
        Primary,
        Nearest
    }
    
    internal enum AppBarNotify
    {
        StateChange = 0,
        PosChanged,
        FullScreenApp,
        WindowArrange
    }

    internal enum AppBarMessage
    {
        New = 0,
        Remove,
        QueryPos,
        SetPos,
        GetState,
        GetTaskbarPos,
        Activate,
        GetAutoHideBar,
        SetAutoHideBar,
        WindowPosChanged,
        SetState
    }
        
    [Flags]
    internal enum DwmNcRenderingPolicy
    {
        UseWindowStyle,
        Disabled,
        Enabled,
        Last
    }
    
    [Flags]
    internal enum DwmWindowAttribute
    {
        NcRenderingEnabled = 1,
        NcRenderingPolicy,
        TransitionsForceDisabled,
        AllowNcPaint,
        CaptionButtonBounds,
        NonClientRtlLayout,
        ForceIconicRepresentation,
        Flip3DPolicy,
        ExtendedFrameBounds,
        HasIconicBitmap,
        DisallowPeek,
        ExcludedFromPeek,
        Last
    }
    
    internal enum D2D1FactoryType
    {
        SingleThreaded,
        MultiThreaded,
    }
}