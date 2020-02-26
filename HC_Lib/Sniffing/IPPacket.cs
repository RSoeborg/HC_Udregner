using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace HC_Lib.Sniffing
{
    // ReSharper disable once InconsistentNaming
    public class IPPacket
    {
        public int Version { get; }
        public int HeaderLength { get; }
        public int Protocol { get; }
        public IPAddress SourceAddress { get; }
        public IPAddress DestAddress { get; }
        public ushort SourcePort { get; }
        public ushort DestPort { get; }

        public IPPacket(byte[] data)
        {
            var versionAndLength = data[0];
            this.Version = versionAndLength >> 4;

            // Only parse IPv4 packets for now
            if (this.Version != 4)
                return;

            this.HeaderLength = (versionAndLength & 0x0F) << 2;

            this.Protocol = Convert.ToInt32(data[9]);
            this.SourceAddress = new IPAddress(BitConverter.ToUInt32(data, 12));
            this.DestAddress = new IPAddress(BitConverter.ToUInt32(data, 16));

            if (Enum.IsDefined(typeof(ProtocolsWithPort), this.Protocol))
            {
                // Ensure big-endian
                this.SourcePort = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, this.HeaderLength));
                this.DestPort = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, this.HeaderLength + 2));
            }
        }
    }

    /// <summary>
    /// Protocols that have a port abstraction
    /// </summary>
    internal enum ProtocolsWithPort
    {
        TCP = 6,
        UDP = 17,
        SCTP = 132
    }
}
