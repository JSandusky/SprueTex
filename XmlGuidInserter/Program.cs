using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XmlGuidInserter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                System.Console.WriteLine("Error! Expected 'xmlguidinserter <src_file> <export_file>");
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(args[0]);

            AttachGUIDsTo("//node", doc);
            AttachGUIDsTo("//in", doc);
            AttachGUIDsTo("//out", doc);

            doc.Save(args[1]);
        }

        static void AttachGUIDsTo(string name, XmlDocument doc)
        {
            XmlNodeList nodes = doc.SelectNodes(name);
            foreach (XmlNode node in nodes)
            {
                XmlElement elem = node as XmlElement;
                if (elem.HasAttribute("guid"))
                    continue;
                var attr = doc.CreateAttribute("guid");
                attr.Value = Guid.NewGuid().ToString();
                elem.Attributes.Append(attr);
            }
        }
    }
}
