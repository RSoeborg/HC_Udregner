using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HC_Lib.Win
{
    public class MSElement
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, [Out] StringBuilder lParam);
        
        private readonly IntPtr handle;
        public MSElement(IntPtr handle) {
            this.handle = handle;
        }
        
        public string GetText()
        {
            return GetText(handle);
        }

        // Gets text text from a control by it's handle.
        private static string GetText(IntPtr handle)
        {
            const uint WM_GETTEXTLENGTH = 0x000E;
            const uint WM_GETTEXT = 0x000D;

            // Gets the text length.
            var length = (int)SendMessage(handle, WM_GETTEXTLENGTH, IntPtr.Zero, null);

            // Init the string builder to hold the text.
            var sb = new StringBuilder(length + 1);

            // Writes the text from the handle into the StringBuilder
            SendMessage(handle, WM_GETTEXT, (IntPtr)sb.Capacity, sb);

            // Return the text as a string.
            return sb.ToString();
        }
    }
}
