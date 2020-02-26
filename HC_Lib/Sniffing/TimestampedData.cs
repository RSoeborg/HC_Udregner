using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HC_Lib.Sniffing
{
    public class TimestampedData
    {
        public DateTime Timestamp { get; }
        public byte[] Data { get; }

        public TimestampedData(DateTime timestamp, byte[] data)
        {
            this.Timestamp = timestamp;
            this.Data = data;
        }
    }
}
