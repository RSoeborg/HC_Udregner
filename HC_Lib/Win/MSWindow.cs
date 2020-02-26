using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HC_Lib.Win
{
    public class MSWindow : IWindow
    {
        const uint WM_KEYDOWN = 0x100;
        const uint WM_KEYUP = 0x0101;
        const uint WM_CLOSE = 0x0010;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);


        public enum SW : int
        {
            HIDE = 0,
            SHOWNORMAL = 1,
            SHOWMINIMIZED = 2,
            SHOWMAXIMIZED = 3,
            SHOWNOACTIVATE = 4,
            SHOW = 5,
            MINIMIZE = 6,
            SHOWMINNOACTIVE = 7,
            SHOWNA = 8,
            RESTORE = 9,
            SHOWDEFAULT = 10
        }

        public readonly IntPtr Handle; // ReadOnly: Used for HashCode() - Do NOT change after init.

        public MSWindow(IntPtr Handle) {
            this.Handle = Handle;
        }

        public string Title
        {
            get
            {
                int length = GetWindowTextLength(Handle);
                StringBuilder sb = new StringBuilder(length + 1);
                GetWindowText(Handle, sb, sb.Capacity);
                return sb.ToString();
            }
        }

        public void Hide()
        {
            ShowWindowAsync(Handle, (int)SW.HIDE);
        }

        public void Show()
        {
            ShowWindowAsync(Handle, (int)SW.SHOW);
        }

        public void SetWindowState(SW State)
        {
            ShowWindowAsync(Handle, (int)State);
        }

        public void WindowPos(int x, int y, int Width, int Height)
        {
            MoveWindow(Handle, x, y, Width, Height, true);
        }

        public void SendKeyStroke(Keys key) {
            PostMessage(Handle, WM_KEYDOWN, (IntPtr)(key), IntPtr.Zero);
            PostMessage(Handle, WM_KEYUP, (IntPtr)(key), IntPtr.Zero);

        }

        public void SendKeyDown(Keys key)
        {
            PostMessage(Handle, WM_KEYDOWN, (IntPtr)(key), IntPtr.Zero);
        }

        public void SendKeyUp(Keys key)
        {
            PostMessage(Handle, WM_KEYUP, (IntPtr)(key), IntPtr.Zero);
        }

        public void Close()
        {
            SendMessage(Handle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }

        public override bool Equals(object obj)
        {
            if (obj is MSWindow) {
                return Handle.Equals(((MSWindow)obj).Handle);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Handle.GetHashCode();
        }
    }
}
