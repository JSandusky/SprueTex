using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Color = Microsoft.Xna.Framework.Color;

namespace MapLib
{
    public enum UniType
    {
        Integer,
        Float,
        String,
        Boolean,
        LinedefType, //int
        SectorEffect, //int
        Texture, //string
        Flat, //string
        AngleDegrees, //int
        AngleRadians, //float
        Color, //string RRGGBB missing AA
        EnumOption, //???probably an int?
        EnumBits, //???probably an int?
        SectorTag, //int
        ThingTag, //int
        LinedefTag, //int
        EnumStrings, //string
        AngleDegreesFloat, //float
        ThingType, //int
        ThingClass //int, don't think this one works in tools?
    }

    public class UniField
    {
        UniType type;
        string data;
        object rawData;

        public UniField(int type, String data)
        {
            this.data = data;
            this.type = (UniType)type;
            switch (this.type)
            {
                case UniType.Integer:
                case UniType.LinedefType:
                case UniType.SectorEffect:
                case UniType.AngleDegrees:
                case UniType.SectorTag:
                case UniType.LinedefTag:
                case UniType.ThingType:
                case UniType.ThingClass:
                    rawData = GetInt();
                    break;
                case UniType.Float:
                case UniType.AngleRadians:
                    rawData = GetFloat();
                    break;
                case UniType.Boolean:
                    rawData = GetBool();
                    break;
                case UniType.Color:
                    rawData = GetColor();
                    break;
                case UniType.String:
                case UniType.Texture:
                case UniType.Flat:
                case UniType.EnumStrings:
                    break;
            }
        }

        private UniField() { }

        public int GetTypeCode() { return (int)type; }
        public UniType GetFieldType() { return type; }
        public string GetTypeName()
        {
            return type.ToString();
        }

        public string GetText() { return data; }

        public T Get<T>()
        {
            return (T)rawData;
        }

        public float GetFloat()
        {
            if (rawData != null)
                return (float)rawData;
            return float.Parse(data);
        }
        public int GetInt()
        {
            if (rawData != null)
                return (int)rawData;

            return int.Parse(data);
        }

        public Color GetColor()
        {
            if (rawData != null)
                return (Color)rawData;
            int r = 0;
            int g = 0;
            int b = 0;
            r = HexToInt(data[0]) << 4 + HexToInt(data[1]);
            g = HexToInt(data[2]) << 4 + HexToInt(data[3]);
            b = HexToInt(data[4]) << 4 + HexToInt(data[5]);

            return new Color(r, g, b);
        }

        int HexToInt(char hexChar)
        {
            hexChar = char.ToUpper(hexChar);  // may not be necessary

            return (int)hexChar < (int)'A' ?
                ((int)hexChar - (int)'0') :
                10 + ((int)hexChar - (int)'A');
        }

        public bool GetBool()
        {
            if (rawData != null)
                return (bool)rawData;
            return bool.Parse(data);
        }
        public LineDef GetLine(Map map)
        {
            return map.Lines[GetInt()];
        }
        public SideDef GetSide(Map map)
        {
            return map.Sides[GetInt()];
        }
        public Sector GetSector(Map map)
        {
            return map.Sectors[GetInt()];
        }
    }
}
