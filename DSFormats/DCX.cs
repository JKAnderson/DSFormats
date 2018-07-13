using System.IO;
using System.IO.Compression;

namespace DSFormats
{
    public static class DCX
    {
        public static byte[] Decompress(byte[] data)
        {
            BinaryReaderEx br = new BinaryReaderEx(true, data);
            return decompress(br);
        }

        public static byte[] Decompress(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                BinaryReaderEx br = new BinaryReaderEx(true, stream);
                return decompress(br);
            }
        }

        private static byte[] decompress(BinaryReaderEx br)
        {
            br.AssertASCII("DCX\0");
            br.AssertInt32(0x10000);
            br.AssertInt32(0x18);
            br.AssertInt32(0x24);
            br.AssertInt32(0x24);
            int headerLength = br.ReadInt32();
            br.AssertASCII("DCS\0");
            int uncompressedSize = br.ReadInt32();
            int compressedSize = br.ReadInt32();
            br.AssertASCII("DCP\0");
            br.AssertASCII("DFLT");
            br.AssertInt32(0x20);
            br.AssertInt32(0x9000000);
            br.AssertInt32(0x0);
            br.AssertInt32(0x0);
            br.AssertInt32(0x0);
            // These look suspiciously like flags
            br.AssertInt32(0x00010100);
            br.AssertASCII("DCA\0");
            int compressedHeaderLength = br.ReadInt32();
            // Some kind of magic values for zlib
            br.AssertByte(0x78);
            br.AssertByte(0xDA);

            // Size includes 78DA
            byte[] compressed = br.ReadBytes(compressedSize - 2);
            byte[] decompressed = new byte[uncompressedSize];

            using (MemoryStream cmpStream = new MemoryStream(compressed))
            using (DeflateStream dfltStream = new DeflateStream(cmpStream, CompressionMode.Decompress))
            using (MemoryStream dcmpStream = new MemoryStream(decompressed))
                dfltStream.CopyTo(dcmpStream);

            return decompressed;
        }

        public static byte[] Compress(byte[] data)
        {
            BinaryWriterEx bw = new BinaryWriterEx(true);
            compress(data, bw);
            return bw.FinishBytes();
        }

        public static byte[] Compress(byte[] data, string path)
        {
            using (FileStream stream = File.Create(path))
            {
                BinaryWriterEx bw = new BinaryWriterEx(true, stream);
                compress(data, bw);
                return bw.FinishBytes();
            }
        }

        private static void compress(byte[] data, BinaryWriterEx bw)
        {
            byte[] compressed;
            using (MemoryStream cmpStream = new MemoryStream())
            using (MemoryStream dcmpStream = new MemoryStream(data))
            {
                DeflateStream dfltStream = new DeflateStream(cmpStream, CompressionMode.Compress);
                dcmpStream.CopyTo(dfltStream);
                dfltStream.Close();
                compressed = cmpStream.ToArray();
            }

            bw.WriteASCII("DCX\0");
            bw.WriteInt32(0x10000);
            bw.WriteInt32(0x18);
            bw.WriteInt32(0x24);
            bw.WriteInt32(0x24);
            bw.WriteInt32(0x2C);
            bw.WriteASCII("DCS\0");
            bw.WriteInt32(data.Length);
            // Size includes 78DA
            bw.WriteInt32(compressed.Length + 2);
            bw.WriteASCII("DCP\0");
            bw.WriteASCII("DFLT");
            bw.WriteInt32(0x20);
            bw.WriteInt32(0x9000000);
            bw.WriteInt32(0x0);
            bw.WriteInt32(0x0);
            bw.WriteInt32(0x0);
            bw.WriteInt32(0x00010100);
            bw.WriteASCII("DCA\0");
            bw.WriteInt32(0x8);
            bw.WriteByte(0x78);
            bw.WriteByte(0xDA);
            bw.WriteBytes(compressed);
        }
    }
}
