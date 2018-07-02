using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DSFormats
{
    public class BinaryWriterEx
    {
        private static readonly Encoding ASCII = Encoding.ASCII;
        private static readonly Encoding ShiftJIS = Encoding.GetEncoding("shift-jis");
        private static readonly Encoding UTF16 = Encoding.Unicode;

        private MemoryStream ms;
        private BinaryWriter bw;
        private Dictionary<string, long> reservations;

        public bool BigEndian = false;
        public int Position
        {
            get { return (int)ms.Position; }
            set { ms.Position = value; }
        }

        public BinaryWriterEx(bool bigEndian)
        {
            ms = new MemoryStream();
            bw = new BinaryWriter(ms);
            reservations = new Dictionary<string, long>();
            BigEndian = bigEndian;
        }

        public byte[] Finish()
        {
            byte[] result = ms.ToArray();
            bw.Close();
            return result;
        }

        public void Pad(int align)
        {
            while (ms.Position % align > 0)
                WriteByte(0);
        }

        private void writeEndian(byte[] bytes)
        {
            if (BigEndian)
                Array.Reverse(bytes);
            bw.Write(bytes);
        }

        public void WriteByte(byte value)
        {
            bw.Write(value);
        }

        public void WriteBytes(byte[] bytes)
        {
            bw.Write(bytes);
        }

        public void WriteBoolean(bool value)
        {
            bw.Write(value);
        }

        public void WriteInt16(short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            writeEndian(bytes);
        }

        public void WriteInt32(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            writeEndian(bytes);
        }

        public void WriteInt32s(int[] values)
        {
            foreach (int value in values)
                WriteInt32(value);
        }

        public void ReserveInt32(string name)
        {
            if (reservations.ContainsKey(name))
                throw new ArgumentException("Key already reserved: " + name);

            reservations[name] = ms.Position;
            WriteInt32(0);
        }

        public void FillInt32(string name, int value)
        {
            if (!reservations.ContainsKey(name))
                throw new ArgumentException("Key was not reserved: " + name);

            long pos = ms.Position;
            ms.Position = reservations[name];
            WriteInt32(value);
            ms.Position = pos;
        }

        public void WriteSingle(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            writeEndian(bytes);
        }

        public void WriteSingles(float[] values)
        {
            foreach (float value in values)
                WriteSingle(value);
        }

        private void writeChars(string text, Encoding encoding, bool terminate)
        {
            byte[] bytes = encoding.GetBytes(text);
            bw.Write(bytes);
            if (terminate)
                bw.Write((byte)0);
        }

        public void WriteASCII(string text, bool terminate = false)
        {
            writeChars(text, ASCII, terminate);
        }

        public void WriteShiftJIS(string text, bool terminate = false)
        {
            writeChars(text, ShiftJIS, terminate);
        }

        public void WriteShiftJISLengthPrefixed(string text, byte delimiter)
        {
            byte[] bytes = ShiftJIS.GetBytes(text);
            WriteInt32(bytes.Length);
            if (bytes.Length > 0)
                WriteBytes(bytes);
            WriteByte(delimiter);
            Pad(4);
        }

        public void WriteUTF16(string text, bool terminate = false)
        {
            byte[] bytes = UTF16.GetBytes(text);
            bw.Write(bytes);
            if (terminate)
            {
                WriteBytes(new byte[] { 0, 0 });
            }
        }
    }
}
