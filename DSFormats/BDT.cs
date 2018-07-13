using System;
using System.Collections.Generic;
using System.IO;

namespace DSFormats
{
    public class BDT
    {
        public List<BDTEntry> Files;
        private int flag;

        public static BDT Read(byte[] bhdBytes, byte[] bdtBytes)
        {
            BinaryReaderEx bhdReader = new BinaryReaderEx(false, bhdBytes);
            BinaryReaderEx bdtReader = new BinaryReaderEx(false, bdtBytes);
            return new BDT(bhdReader, bdtReader);
        }

        public static BDT Read(byte[] bhdBytes, string bdtPath)
        {
            using (FileStream bdtStream = File.OpenRead(bdtPath))
            {
                BinaryReaderEx bhdReader = new BinaryReaderEx(false, bhdBytes);
                BinaryReaderEx bdtReader = new BinaryReaderEx(false, bdtStream);
                return new BDT(bhdReader, bdtReader);
            }
        }

        public static BDT Read(string bhdPath, string bdtPath)
        {
            using (FileStream bhdStream = File.OpenRead(bhdPath))
            using (FileStream bdtStream = File.OpenRead(bdtPath))
            {
                BinaryReaderEx bhdReader = new BinaryReaderEx(false, bhdStream);
                BinaryReaderEx bdtReader = new BinaryReaderEx(false, bdtStream);
                return new BDT(bhdReader, bdtReader);
            }
        }

        private BDT(BinaryReaderEx bhdReader, BinaryReaderEx bdtReader)
        {
            BHD bhd = new BHD(bhdReader);
            flag = bhd.Flag;

            bdtReader.AssertASCII("BDF307D7R6\0\0");
            bdtReader.AssertInt32(0);

            Files = new List<BDTEntry>();
            for (int i = 0; i < bhd.Entries.Count; i++)
            {
                BHDEntry bhdEntry = bhd.Entries[i];
                string name = bhdEntry.Name;
                byte[] data = bdtReader.GetBytes(bhdEntry.Offset, bhdEntry.Size);

                BDTEntry bdtEntry = new BDTEntry
                {
                    Filename = name,
                    Bytes = data
                };
                Files.Add(bdtEntry);
            }
        }

        public (byte[], byte[]) Write()
        {
            BinaryWriterEx bhdWriter = new BinaryWriterEx(false);
            BinaryWriterEx bdtWriter = new BinaryWriterEx(false);
            write(bhdWriter, bdtWriter);
            return (bhdWriter.FinishBytes(), bdtWriter.FinishBytes());
        }

        public byte[] Write(string bdtPath)
        {
            using (FileStream bdtStream = File.Create(bdtPath))
            {
                BinaryWriterEx bhdWriter = new BinaryWriterEx(false);
                BinaryWriterEx bdtWriter = new BinaryWriterEx(false, bdtStream);
                write(bhdWriter, bdtWriter);
                bdtWriter.Finish();
                return bhdWriter.FinishBytes();
            }
        }

        public void Write(string bhdPath, string bdtPath)
        {
            using (FileStream bhdStream = File.Create(bhdPath))
            using (FileStream bdtStream = File.Create(bdtPath))
            {
                BinaryWriterEx bhdWriter = new BinaryWriterEx(false, bhdStream);
                BinaryWriterEx bdtWriter = new BinaryWriterEx(false, bdtStream);
                write(bhdWriter, bdtWriter);
                bhdWriter.Finish();
                bdtWriter.Finish();
            }
        }

        private void write(BinaryWriterEx bhdWriter, BinaryWriterEx bdtWriter)
        {
            bhdWriter.WriteASCII("BHF307D7R6\0\0");
            bhdWriter.WriteInt32(flag);
            bhdWriter.WriteInt32(Files.Count);
            bhdWriter.WriteInt32(0);
            bhdWriter.WriteInt32(0);
            bhdWriter.WriteInt32(0);

            bdtWriter.WriteASCII("BDF307D7R6\0\0");
            bdtWriter.WriteInt32(0);

            for (int i = 0; i < Files.Count; i++)
            {
                BDTEntry file = Files[i];
                bhdWriter.WriteInt32(0x40);
                bhdWriter.WriteInt32(file.Bytes.Length);
                bhdWriter.WriteInt32(bdtWriter.Position);
                bhdWriter.WriteInt32(i);
                bhdWriter.ReserveInt32($"FileName{i}");
                bhdWriter.WriteInt32(file.Bytes.Length);

                bdtWriter.WriteBytes(file.Bytes);
                bdtWriter.Pad(0x10);
            }

            for (int i = 0; i < Files.Count; i++)
            {
                BDTEntry file = Files[i];
                bhdWriter.FillInt32($"FileName{i}", bhdWriter.Position);
                bhdWriter.WriteShiftJIS(file.Filename, true);
            }
        }

        private class BHD
        {
            public List<BHDEntry> Entries;
            public int Flag;

            public BHD(BinaryReaderEx br)
            {
                br.AssertASCII("BHF307D7R6\0\0");
                Flag = br.ReadInt32();
                if (Flag != 0x54 && Flag != 0x74)
                    throw new NotSupportedException($"Unrecognized BHD flag: 0x{Flag:X}");

                int fileCount = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);

                Entries = new List<BHDEntry>();
                for (int i = 0; i < fileCount; i++)
                {
                    br.AssertInt32(0x40);
                    int fileSize = br.ReadInt32();
                    int fileOffset = br.ReadInt32();
                    br.AssertInt32(i);
                    int fileNameOffset = br.ReadInt32();
                    // Why is this here twice?
                    br.AssertInt32(fileSize);

                    string name = br.GetShiftJIS(fileNameOffset);
                    BHDEntry entry = new BHDEntry()
                    {
                        Name = name,
                        Offset = fileOffset,
                        Size = fileSize,
                    };
                    Entries.Add(entry);
                }
            }
        }

        private class BHDEntry
        {
            public string Name;
            public int Offset;
            public int Size;
        }
    }

    public class BDTEntry
    {
        public string Filename;
        public byte[] Bytes;
    }
}
