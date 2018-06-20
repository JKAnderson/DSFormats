﻿using System;
using System.Collections.Generic;

namespace DSFormats
{
    public class BND
    {
        private string signature;
        private byte format;
        public List<BNDEntry> Files;

        public static BND Unpack(byte[] bytes)
        {
            return new BND(bytes);
        }

        private BND(byte[] bytes)
        {
            BinaryReaderEx br = new BinaryReaderEx(bytes, false);
            br.AssertASCII("BND3");
            // FaceGen.fgbnd: 09G17X51
            // Everything else (that I'm checking): 07D7R6\0\0
            signature = br.ReadASCII(8);
            format = br.AssertByte(0x54, 0x74);
            br.AssertByte(0);
            br.AssertByte(0);
            br.AssertByte(0);

            int fileCount = br.ReadInt32();
            if (fileCount == 0)
                throw new NotSupportedException("Empty BND :(");
            int fileNameEnd = br.ReadInt32();
            br.AssertInt32(0);
            br.AssertInt32(0);

            Files = new List<BNDEntry>();
            for (int i = 0; i < fileCount; i++)
            {
                br.AssertInt32(0x40);
                int fileSize = br.ReadInt32();
                int fileOffset = br.ReadInt32();
                // This is not the same as i
                int id = br.ReadInt32();
                int fileNameOffset = br.ReadInt32();
                br.AssertInt32(fileSize);

                string name = br.GetShiftJIS(fileNameOffset);
                byte[] data = br.GetBytes(fileOffset, fileSize);

                BNDEntry entry = new BNDEntry
                {
                    Filename = name,
                    ID = id,
                    Bytes = data
                };
                Files.Add(entry);
            }
        }

        public byte[] Repack()
        {
            BinaryWriterEx bw = new BinaryWriterEx(false);
            bw.WriteASCII("BND3");
            bw.WriteASCII(signature);
            bw.WriteByte(format);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteByte(0);

            bw.WriteInt32(Files.Count);
            bw.ReserveInt32("NameEnd");
            bw.WriteInt32(0);
            bw.WriteInt32(0);

            for (int i = 0; i < Files.Count; i++)
            {
                BNDEntry entry = Files[i];
                bw.WriteInt32(0x40);
                bw.WriteInt32(entry.Bytes.Length);
                bw.ReserveInt32($"FileData{i}");
                bw.WriteInt32(entry.ID);
                bw.ReserveInt32($"FileName{i}");
                bw.WriteInt32(entry.Bytes.Length);
            }

            for (int i = 0; i < Files.Count; i++)
            {
                BNDEntry entry = Files[i];
                bw.FillInt32($"FileName{i}", bw.Position);
                bw.WriteShiftJIS(entry.Filename, true);
            }
            // Do not include padding
            bw.FillInt32($"NameEnd", bw.Position);
            bw.Pad(0x10);

            for (int i = 0; i < Files.Count; i++)
            {
                BNDEntry entry = Files[i];
                bw.FillInt32($"FileData{i}", bw.Position);
                bw.WriteBytes(entry.Bytes);
                bw.Pad(0x10);
            }

            return bw.Finish();
        }
    }

    public class BNDEntry
    {
        public string Filename;
        public int ID;
        public byte[] Bytes;
    }
}
