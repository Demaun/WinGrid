using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinGridApp
{
    public static class PInvoke
    {
        internal delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern short GetKeyState(int nVirtKey);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern uint MapVirtualKey(uint uCode, uint uMapType);

        internal static class KeyStateConstants
        {
            internal const int KEY_PRESSED = 0x8000;
            internal const int WM_KEYDOWN = 0x0100;
            internal const int WM_KEYUP = 0x0101;
            internal const int WM_SYSKEYDOWN = 0x0104;
            internal const int WM_SYSKEYUP = 0x0105;
            internal const int WH_KEYBOARD_LL = 13;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SWP uFlags);
        public static bool SetWindowRect(IntPtr hWnd, RECT rect)
        {
            return SetWindowPos(hWnd, IntPtr.Zero, rect.Left, rect.Top, Math.Max(0, rect.Right - rect.Left), Math.Max(0, rect.Bottom - rect.Top), SWP.NOOWNERZORDER | SWP.NOCOPYBITS | SWP.NOACTIVATE);
        }
        public static bool SetWindowRect(IntPtr hWnd, int x, int y, int w, int h)
        {
            return SetWindowPos(hWnd, IntPtr.Zero, x, y, Math.Max(0, w), Math.Max(0, h), SWP.NOOWNERZORDER | SWP.NOCOPYBITS | SWP.NOACTIVATE);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
            public int Width => Right - Left;
            public int Height => Bottom - Top;
            public Point Center => new Point((Left + Right) / 2, (Top + Bottom) / 2);
        }
    }
}
