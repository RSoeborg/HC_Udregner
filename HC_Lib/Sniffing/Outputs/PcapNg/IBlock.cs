using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HC_Lib.Sniffing.Outputs.PcapNg
{
    public interface IBlock
    {
        byte[] GetBytes();
    }
}
