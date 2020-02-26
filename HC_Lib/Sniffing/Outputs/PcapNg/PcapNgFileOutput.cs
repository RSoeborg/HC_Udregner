using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HC_Lib.Sniffing.Outputs.PcapNg
{
    /// <summary>
    /// Outputs files in PCAPNG file format
    /// https://tools.ietf.org/html/draft-tuexen-opswg-pcapng-00 
    /// </summary>
    public class PcapNgFileOutput : IOutput, IDisposable
    {
        private readonly NetworkInterfaceInfo nic;
        private readonly FileStream fileStream;
        private readonly BinaryWriter writer;

        private readonly List<byte> buffer;

        public PcapNgFileOutput(NetworkInterfaceInfo nic, string filename)
        {
            this.nic = nic;
            this.fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
            this.writer = new BinaryWriter(this.fileStream);
            this.WriteHeader();
            this.buffer = new List<byte>();
        }

        public void Output(TimestampedData timestampedData)
        {
            var block = new EnhancedPacketBlock(timestampedData);
            var blockData = block.GetBytes();

            //this.buffer.AddRange(blockData);
            this.writer.Write(blockData);
            this.writer.Flush();
        }

        public byte[] GetBuffer() => buffer.ToArray();

        public void Dispose()
        {
            this.writer.Close();
            this.writer.Dispose();
            this.fileStream.Close();
            this.fileStream.Dispose();
            this.buffer.Clear();
        }

        private void WriteHeader()
        {
            var sectionHeaderBlock = new SectionHeaderBlock();
            this.writer.Write(sectionHeaderBlock.GetBytes());
            //this.buffer.AddRange(sectionHeaderBlock.GetBytes());

            var interfaceDescriptionBlock = new InterfaceDescriptionBlock(this.nic);
            this.writer.Write(interfaceDescriptionBlock.GetBytes());
            //this.buffer.AddRange(interfaceDescriptionBlock.GetBytes());
        }
    }
}
