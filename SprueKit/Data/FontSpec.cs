using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Data
{
    public class FontSpec
    {
        public string FontFace { get; set; }
        public float FontSize { get; set; }
        public bool Underline { get; set; }
        public bool Bold { get; set; }

        public FontSpec()
        {
            FontFace = "Arial";
            FontSize = 10;
        }

        public FontSpec(System.Drawing.Font font)
        {
            FontFace = font.FontFamily.Name;
            FontSize = font.SizeInPoints;
            Underline = font.Underline;
            Bold = font.Bold;
        }

        public System.Drawing.Font GetFont()
        {
            System.Drawing.FontStyle style = System.Drawing.FontStyle.Regular;
            if (Bold)
                style |= System.Drawing.FontStyle.Bold;
            if (Underline)
                style |= System.Drawing.FontStyle.Underline;

            return new System.Drawing.Font(FontFace, FontSize, style, System.Drawing.GraphicsUnit.Point);
        }

        public override string ToString()
        {
            return String.Format("{0}|{1}|{2}|{3}", FontFace, FontSize, Bold, Underline);
        }

        public string ToDisplayString()
        {
            return String.Format("{0}, {1}pt {2} {3}", FontFace, FontSize, Bold ? "Bold" : "", Underline ? "Underline" : "");
        }

        public static FontSpec FromString(string text)
        {
            string[] terms = text.Split('|');
            if (terms.Length != 4)
                return new Data.FontSpec();
            return new Data.FontSpec
            {
                FontFace = terms[0],
                FontSize = float.Parse(terms[1]),
                Bold = bool.Parse(terms[2]),
                Underline = bool.Parse(terms[3])
            };
        }

        public FontSpec Clone()
        {
            return new FontSpec
            {
                FontFace = this.FontFace,
                FontSize = this.FontSize,
                Bold = this.Bold,
                Underline = this.Underline
            };
        }
    }

    public static class FontSpecExt
    {
        public static void Write(this System.IO.BinaryWriter strm, FontSpec spec)
        {
            strm.Write(spec.FontFace);
            strm.Write(spec.FontSize);
            strm.Write(spec.Bold);
            strm.Write(spec.Underline);
        }

        public static FontSpec ReadFontSpec(this System.IO.BinaryReader rdr)
        {
            string face = rdr.ReadString();
            float size = rdr.ReadSingle();
            bool bold = rdr.ReadBoolean();
            bool underline = rdr.ReadBoolean();
            return new Data.FontSpec { FontFace = face, FontSize = size, Bold = bold, Underline = underline };
        }
    }
}
