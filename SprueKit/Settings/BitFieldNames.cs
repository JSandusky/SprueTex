using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Settings
{
    public class BitFieldNames : BaseClass
    {
        public string[] CapabilityNames { get; set; } = new string[32] {
            "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", "",

            "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", ""
        };

        public string[] FlagNames { get; set; } = new string[32] {
            "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", "",

            "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", ""
        };

        public void ScanCompatibleNames(List<string> target, uint value, string[] nameSet)
        {
            for (int i = 0; i < 32; ++i)
            {
                int val = 1 << i;
                if ((value & val) != 0)
                    target.Add(nameSet[i]);
            }
        }

        public static string MakeFlagName(string[] fields, uint value)
        {
            StringBuilder sb = new StringBuilder();
            bool anyAdded = false;
            for (int i = 0; i < 32; ++i)
            {
                int val = 1 << i;
                if ((value & val) != 0)
                {
                    if (!string.IsNullOrWhiteSpace(fields[i]))
                    {
                        if (anyAdded)
                            sb.AppendFormat(", {0}", fields[i]);
                        else
                            sb.AppendFormat("{0}", fields[i]);
                        anyAdded = true;
                    }
                    else
                    {
                        if (anyAdded)
                            sb.AppendFormat(", {0}", (i + 1).ToString());
                        else
                            sb.AppendFormat("{0}", (i + 1).ToString());
                        anyAdded = true;
                    }
                }
            }
            return sb.ToString();
        }

        public string GetFieldName(string bitSet, int index)
        {
            if (bitSet.Equals("Capabilities"))
                return CapabilityNames[index];
            else if (bitSet.Equals("Flags"))
                return FlagNames[index];
            return null;
        }
    }
}
