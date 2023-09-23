using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Various.Utils;

public static partial class NativeUtils
{
    [StructLayout(LayoutKind.Sequential)]
    internal class ComRect
    {
        public int bottom;
        public int left;
        public int right;
        public int top;

        public ComRect()
        {
        }

        public ComRect(System.Windows.Rect r)
        {
            left = (int)r.X;
            top = (int)r.Y;
            right = (int)r.Right;
            bottom = (int)r.Bottom;
        }

        public ComRect(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public static ComRect FromXYWH(int x, int y, int width, int height)
        {
            return new ComRect(x, y, x + width, y + height);
        }

        public override string ToString()
        {
            return "Left = " + left + " Top " + top + " Right = " + right + " Bottom = " + bottom;
        }
    }
    
    internal class MonitorEnumCallback
    {
        public ArrayList Screens { get; }
        
        public MonitorEnumCallback()
        {
            Screens = new ArrayList();
        }

        public bool Callback(IntPtr monitor, IntPtr hdc, IntPtr lprcMonitor, IntPtr lparam)
        {
            Screens.Add(new ScreenHelper(monitor));
            return true;
        }
    }
}