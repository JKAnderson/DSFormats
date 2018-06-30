using System;
using System.Collections.Generic;

namespace DSFormats
{
    public class DRBRaw
    {
        public RawSection str;
        public RawSection texi;
        public RawSection shpr;
        public RawSection ctpr;
        public RawSection anip;
        public RawSection intp;
        public RawSection scdp;
        public RawSection shap;
        public RawSection ctrl;
        public RawSection anik;
        public RawSection anio;
        public RawSection anim;
        public RawSection scdk;
        public RawSection scdo;
        public RawSection scdl;
        public RawSection dlgo;
        public RawSection dlg;

        public static DRBRaw Unpack(byte[] bytes)
        {
            return new DRBRaw(bytes);
        }

        private DRBRaw(byte[] bytes)
        {
            BinaryReaderEx br = new BinaryReaderEx(bytes, false);
            br.AssertASCII("DRB\0");
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);

            str = new RawSection(br, "STR\0");
            texi = new RawSection(br, "TEXI");
            shpr = new RawSection(br, "SHPR");
            ctpr = new RawSection(br, "CTPR");
            anip = new RawSection(br, "ANIP");
            intp = new RawSection(br, "INTP");
            scdp = new RawSection(br, "SCDP");
            shap = new RawSection(br, "SHAP");
            ctrl = new RawSection(br, "CTRL");
            anik = new RawSection(br, "ANIK");
            anio = new RawSection(br, "ANIO");
            anim = new RawSection(br, "ANIM");
            scdk = new RawSection(br, "SCDK");
            scdo = new RawSection(br, "SCDO");
            scdl = new RawSection(br, "SCDL");
            dlgo = new RawSection(br, "DLGO");
            dlg = new RawSection(br, "DLG\0");

            br.AssertASCII("END\0");
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
        }

        public byte[] Write()
        {
            BinaryWriterEx bw = new BinaryWriterEx(false);
            bw.WriteASCII("DRB\0");
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);

            str.Write(bw);
            texi.Write(bw);
            shpr.Write(bw);
            ctpr.Write(bw);
            anip.Write(bw);
            intp.Write(bw);
            scdp.Write(bw);
            shap.Write(bw);
            ctrl.Write(bw);
            anik.Write(bw);
            anio.Write(bw);
            anim.Write(bw);
            scdk.Write(bw);
            scdo.Write(bw);
            scdl.Write(bw);
            dlgo.Write(bw);
            dlg.Write(bw);

            bw.WriteASCII("END\0");
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            return bw.Finish();
        }

        public class RawSection
        {
            public readonly string Name;
            public int Count;
            public byte[] Bytes;

            public RawSection(BinaryReaderEx br, string name)
            {
                if (name.Length != 4)
                    throw null;
                br.AssertASCII(name);
                Name = name;
                int size = br.ReadInt32();
                Count = br.ReadInt32();
                br.AssertInt32(0);
                Bytes = br.ReadBytes(size);
            }

            public void Write(BinaryWriterEx bw)
            {
                bw.WriteASCII(Name);
                bw.WriteInt32(Bytes.Length);
                bw.WriteInt32(Count);
                bw.WriteInt32(0);
                bw.WriteBytes(Bytes);
            }
        }
    }
}
