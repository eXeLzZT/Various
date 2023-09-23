using System;
using System.Windows;

namespace Various.Utils;

public class RegisterInfo
{
    public int CallbackId { get; set; }
    public bool IsRegistered { get; set; }
    public Window Window { get; set; }
    public AppBarEdge Edge { get; set; }
    public WindowStyle OriginalStyle { get; set; }
    public Point OriginalPosition { get; set; }
    public Size OriginalSize { get; set; }
    public ResizeMode OriginalResizeMode { get; set; }
    public bool OriginalTopmost { get; set; }
    
    public Rect? DockedSize { get; set; }

    public IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == CallbackId)
        {
            if (wParam.ToInt32() == (int)NativeUtils.AppBarNotify.PosChanged)
            {
                AppBarUtils.SetPosition(this, Window);
                handled = true;
            }
        }
        
        return IntPtr.Zero;
    }
}