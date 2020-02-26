using System.Runtime.InteropServices;
using System.Security.Principal;

namespace HC_Lib.Sniffing
{
    public static class SystemInformation
    {        
        public static bool IsAdmin()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent())
                    .IsInRole(WindowsBuiltInRole.Administrator);    
        }
    }
}
