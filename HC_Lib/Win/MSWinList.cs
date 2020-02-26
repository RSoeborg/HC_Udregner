using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HC_Lib.Win
{
    class MSWinList : IWinList
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumedWindow lpEnumFunc, ArrayList lParam);
        private delegate bool EnumedWindow(IntPtr handleWindow, ArrayList handles);

        public List<IWindow> Windows => GetWindows()
            .Select(hWnd => new MSWindow(hWnd))
            .Where(window => !string.IsNullOrWhiteSpace(window.Title))
            .Cast<IWindow>()
            .ToList();
        
        private List<IntPtr> GetWindows()
        {
            ArrayList WindowHandles = new ArrayList();
            EnumedWindow callBackPtr = GetWindowHandle;
            EnumWindows(callBackPtr, WindowHandles);
            return WindowHandles.Cast<IntPtr>().ToList();
        }
        private bool GetWindowHandle(IntPtr WindowHandle, ArrayList WindowHandles)
        {
            WindowHandles.Add(WindowHandle);
            return true;
        }
        
    }
}
