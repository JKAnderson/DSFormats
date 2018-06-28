using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DSFormats
{
    public class BinaryReaderEx
    {
        private static readonly Encoding ASCII = Encoding.ASCII;
        private static readonly Encoding ShiftJIS = Encoding.GetEncoding("shift-jis");
        private static readonly Encoding UTF16 = Encoding.Unicode;

        private MemoryStream ms;
        private BinaryReader br;
        public bool BigEndian = false;

        public int Position
        {
            get { return (int)ms.Position; }
            set { ms.Position = value; }
        }

        public BinaryReaderEx(byte[] input, bool bigEndian)
        {
            ms = new MemoryStream(input);
            br = new BinaryReader(ms);
            BigEndian = bigEndian;
        }

        private byte[] readEndian(int length)
        {
            byte[] bytes = br.ReadBytes(length);
            if (BigEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        public void Pad(int align)
        {
            if (ms.Position % align > 0)
                ms.Position += align - (ms.Position % align);
        }

        public void Skip(int count)
        {
            ms.Position += count;
        }

        public byte ReadByte()
        {
            return br.ReadByte();
        }

        public byte[] ReadBytes(int length)
        {
            return br.ReadBytes(length);
        }

        public byte GetByte(int offset)
        {
            long pos = ms.Position;
            ms.Position = offset;
            byte result = ReadByte();
            ms.Position = pos;
            return result;
        }

        public byte[] GetBytes(int offset, int length)
        {
            long pos = ms.Position;
            ms.Position = offset;
            byte[] result = ReadBytes(length);
            ms.Position = pos;
            return result;
        }

        public bool ReadBoolean()
        {
            return br.ReadBoolean();
        }

        public short ReadInt16()
        {
            return BitConverter.ToInt16(readEndian(2), 0);
        }

        public ushort ReadUInt16()
        {
            return BitConverter.ToUInt16(readEndian(2), 0);
        }

        public short GetInt16(int offset)
        {
            long position = ms.Position;
            ms.Position = offset;
            short result = ReadInt16();
            ms.Position = position;
            return result;
        }

        public int ReadInt32()
        {
            return BitConverter.ToInt32(readEndian(4), 0);
        }

        public int[] ReadInt32s(int count)
        {
            int[] result = new int[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadInt32();
            return result;
        }

        public float ReadSingle()
        {
            return BitConverter.ToSingle(readEndian(4), 0);
        }

        public float[] ReadSingles(int count)
        {
            float[] result = new float[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadSingle();
            return result;
        }

        public float[] GetSingles(int offset, int count)
        {
            long position = ms.Position;
            ms.Position = offset;
            float[] result = ReadSingles(count);
            ms.Position = position;
            return result;
        }

        private string readChars(Encoding encoding, int length)
        {
            byte[] bytes;
            if (length == 0)
            {
                List<byte> byteList = new List<byte>();
                byte b = ReadByte();
                while (b != 0)
                {
                    byteList.Add(b);
                    b = ReadByte();
                }
                bytes = byteList.ToArray();
            }
            else
            {
                bytes = ReadBytes(length);
            }
            return encoding.GetString(bytes);
        }

        public string ReadASCII(int length = 0)
        {
            return readChars(ASCII, length);
        }

        public string ReadShiftJIS(int length = 0)
        {
            return readChars(ShiftJIS, length);
        }

        public string ReadShiftJISLengthPrefixed(byte delimiter)
        {
            int length = ReadInt32();
            string result = "";
            if (length > 0)
                result = readChars(ShiftJIS, length);
            AssertByte(delimiter);
            Pad(4);
            return result;
        }

        public string GetShiftJIS(int offset)
        {
            long pos = ms.Position;
            ms.Position = offset;
            string result = ReadShiftJIS();
            ms.Position = pos;
            return result;
        }

        public string ReadUTF16()
        {
            List<byte> bytes = new List<byte>();
            byte[] pair = ReadBytes(2);
            while (pair[0] != 0 || pair[1] != 0)
            {
                bytes.Add(pair[0]);
                bytes.Add(pair[1]);
                pair = ReadBytes(2);
            }
            return UTF16.GetString(bytes.ToArray());
        }

        public byte AssertByte(params byte[] values)
        {
            byte b = ReadByte();
            bool valid = false;
            foreach (byte value in values)
                if (b == value)
                    valid = true;

            if (!valid)
            {
                StringBuilder sbValues = new StringBuilder();
                for (int i = 0; i < values.Length; i++)
                {
                    if (i != 0)
                        sbValues.Append(", ");
                    sbValues.Append("0x" + values[i].ToString("X"));
                }
                throw new InvalidDataException(string.Format(
                    "Read byte: 0x{0:X} | Expected byte: {1}", b, sbValues.ToString()));
            }

            return b;
        }

        public void AssertBytes(params byte[] values)
        {
            foreach (byte value in values)
            {
                byte b = ReadByte();
                if (b != value)
                {
                    throw new InvalidDataException(string.Format(
                        "Read byte: 0x{0:X} | Expected byte: 0x{1:X}", b, value));
                }
            }
        }

        public void AssertInt16(short value)
        {
            short s = ReadInt16();
            if (s != value)
            {
                throw new InvalidDataException(string.Format(
                    "Read short: 0x{0:X} | Expected short: 0x{1:X}", s, value));
            }
        }

        public void AssertInt32(int value)
        {
            int i = ReadInt32();
            if (i != value)
            {
                throw new InvalidDataException(string.Format(
                    "Read int: 0x{0:X} | Expected int: 0x{1:X}", i, value));
            }
        }

        public void AssertASCII(string value)
        {
            string s = ReadASCII(value.Length);
            if (s != value)
            {
                throw new InvalidDataException(string.Format(
                    "Read string: {0} | Expected string: {1}", s, value));
            }
        }
    }
}
