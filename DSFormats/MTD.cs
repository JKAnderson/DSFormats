using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSFormats
{
    public class MTD
    {
        private int unk1, unk2, unk3, unk4, unk5, unk6, unk7, unk8, unk9, unk10, unk11, unk12;
        public string SpxPath, Description;
        public List<MTDEntryInternal> Internal;
        public List<MTDEntryExternal> External;

        public static MTD Read(byte[] bytes)
        {
            return new MTD(bytes);
        }

        private MTD(byte[] bytes)
        {
            BinaryReaderEx br = new BinaryReaderEx(bytes, false);
            br.AssertInt32(0);
            int fileSize = br.ReadInt32();
            br.AssertInt32(0);
            br.AssertInt32(3);
            unk1 = br.ReadInt32();
            br.AssertInt32(0);
            br.AssertInt32(0x1C);
            br.AssertInt32(1);
            br.AssertInt32(2);
            unk2 = br.ReadInt32();
            br.AssertInt32(4);
            br.AssertASCII("MTD ");
            unk3 = br.ReadInt32();
            br.AssertInt32(0x3E8);
            unk4 = br.ReadInt32();
            br.AssertInt32(0);
            int dataSize = br.ReadInt32();
            br.AssertInt32(2);
            br.AssertInt32(4);
            unk5 = br.ReadInt32();
            SpxPath = br.ReadShiftJISLengthPrefixed(0xA3);
            Description = br.ReadShiftJISLengthPrefixed(0x03);
            br.AssertInt32(1);
            br.AssertInt32(0);
            unk6 = br.ReadInt32();
            br.AssertInt32(3);
            br.AssertInt32(4);
            unk7 = br.ReadInt32();
            br.AssertInt32(0);
            unk8 = br.ReadInt32();

            Internal = new List<MTDEntryInternal>();
            int internalEntryCount = br.ReadInt32();
            for (int i = 0; i < internalEntryCount; i++)
                Internal.Add(MTDEntryInternal.Read(br));

            unk9 = br.ReadInt32();

            External = new List<MTDEntryExternal>();
            int externalEntryCount = br.ReadInt32();
            for (int i = 0; i < externalEntryCount; i++)
                External.Add(MTDEntryExternal.Read(br));

            unk10 = br.ReadInt32();
            br.AssertInt32(0);
            unk11 = br.ReadInt32();
            br.AssertInt32(0);
            unk12 = br.ReadInt32();
            br.AssertInt32(0);
        }

        public byte[] Write()
        {
            BinaryWriterEx bw = new BinaryWriterEx(false);
            bw.WriteInt32(0);
            bw.ReserveInt32("FileSize");
            bw.WriteInt32(0);
            bw.WriteInt32(3);
            bw.WriteInt32(unk1);
            bw.WriteInt32(0);
            bw.WriteInt32(0x1C);
            bw.WriteInt32(1);
            bw.WriteInt32(2);
            bw.WriteInt32(unk2);
            bw.WriteInt32(4);
            bw.WriteASCII("MTD ");
            bw.WriteInt32(unk3);
            bw.WriteInt32(0x3E8);
            bw.WriteInt32(unk4);
            bw.WriteInt32(0);
            bw.ReserveInt32("DataSize");
            bw.WriteInt32(2);
            bw.WriteInt32(4);
            bw.WriteInt32(unk5);
            bw.WriteShiftJISLengthPrefixed(SpxPath, 0xA3);
            bw.WriteShiftJISLengthPrefixed(Description, 0x03);
            bw.WriteInt32(1);
            bw.WriteInt32(0);
            bw.WriteInt32(unk6);
            bw.WriteInt32(3);
            bw.WriteInt32(4);
            bw.WriteInt32(unk7);
            bw.WriteInt32(0);
            bw.WriteInt32(unk8);

            bw.WriteInt32(Internal.Count);
            foreach (MTDEntryInternal internalEntry in Internal)
                internalEntry.Write(bw);

            bw.WriteInt32(unk9);

            bw.WriteInt32(External.Count);
            foreach (MTDEntryExternal externalEntry in External)
                externalEntry.Write(bw);

            bw.WriteInt32(unk10);
            bw.WriteInt32(0);
            bw.WriteInt32(unk11);
            bw.WriteInt32(0);
            bw.WriteInt32(unk12);
            bw.WriteInt32(0);

            int position = bw.Position;
            bw.FillInt32("FileSize", position - 8);
            bw.FillInt32("DataSize", position - 0x4C);

            return bw.Finish();
        }

        public class MTDEntryInternal
        {
            private int unk1, unk2, unk3, unk4, unk5, unk6, unk7, unk8, unk9;
            public string Name, Type;
            public object Value;

            public static MTDEntryInternal Read(BinaryReaderEx br)
            {
                return new MTDEntryInternal(br);
            }

            private MTDEntryInternal(BinaryReaderEx br)
            {
                unk1 = br.ReadInt32();
                unk2 = br.ReadInt32();
                unk3 = br.ReadInt32();
                unk4 = br.ReadInt32();
                unk5 = br.ReadInt32();
                Name = br.ReadShiftJISLengthPrefixed(0xA3);
                Type = br.ReadShiftJISLengthPrefixed(0x04);
                br.AssertInt32(1);
                br.AssertInt32(0);
                unk6 = br.ReadInt32();
                unk7 = br.ReadInt32();
                br.AssertInt32(1);
                unk8 = br.ReadInt32();
                unk9 = br.ReadInt32();

                if (Type == "int")
                    Value = br.ReadInt32();
                else if (Type == "bool")
                    Value = br.ReadBoolean();
                else if (Type == "float")
                    Value = br.ReadSingle();
                else if (Type == "float2")
                    Value = br.ReadSingles(2);
                else if (Type == "float3")
                    Value = br.ReadSingles(3);
                else if (Type == "float4")
                    Value = br.ReadSingles(4);

                br.AssertByte(4);
                br.Pad(4);
                br.AssertInt32(0);
            }

            public void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(unk1);
                bw.WriteInt32(unk2);
                bw.WriteInt32(unk3);
                bw.WriteInt32(unk4);
                bw.WriteInt32(unk5);
                bw.WriteShiftJISLengthPrefixed(Name, 0xA3);
                bw.WriteShiftJISLengthPrefixed(Type, 0x04);
                bw.WriteInt32(1);
                bw.WriteInt32(0);
                bw.WriteInt32(unk6);
                bw.WriteInt32(unk7);
                bw.WriteInt32(1);
                bw.WriteInt32(unk8);
                bw.WriteInt32(unk9);

                if (Type == "int")
                    bw.WriteInt32((int)Value);
                else if (Type == "bool")
                    bw.WriteBoolean((bool)Value);
                else if (Type == "float")
                    bw.WriteSingle((float)Value);
                else if (Type == "float2")
                    bw.WriteSingles((float[])Value);
                else if (Type == "float3")
                    bw.WriteSingles((float[])Value);
                else if (Type == "float4")
                    bw.WriteSingles((float[])Value);

                bw.WriteByte(4);
                bw.Pad(4);
                bw.WriteInt32(0);
            }
        }

        public class MTDEntryExternal
        {
            private int unk1, unk2, unk3, unk4, unk5, unk6, unk7;
            public string Name;
            public int ShaderDataIndex;

            public static MTDEntryExternal Read(BinaryReaderEx br)
            {
                return new MTDEntryExternal(br);
            }

            public MTDEntryExternal(BinaryReaderEx br)
            {
                unk1 = br.ReadInt32();
                unk2 = br.ReadInt32();
                unk3 = br.ReadInt32();
                unk4 = br.ReadInt32();
                unk5 = br.ReadInt32();
                Name = br.ReadShiftJISLengthPrefixed(0x35);
                unk6 = br.ReadInt32();
                unk7 = br.ReadInt32();
                ShaderDataIndex = br.ReadInt32();
            }

            public void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(unk1);
                bw.WriteInt32(unk2);
                bw.WriteInt32(unk3);
                bw.WriteInt32(unk4);
                bw.WriteInt32(unk5);
                bw.WriteShiftJISLengthPrefixed(Name, 0x35);
                bw.WriteInt32(unk6);
                bw.WriteInt32(unk7);
                bw.WriteInt32(ShaderDataIndex);
            }
        }
    }
}
