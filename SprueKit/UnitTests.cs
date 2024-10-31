using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SprueKit
{
    public static class UnitTests
    {
        public static void RunSerializationTest()
        {
            System.Console.WriteLine("Begining serialization tests");
            foreach (var grp in Data.TexGen.TextureGenDocument.NodeGroups)
            {
                foreach (var node in grp.Types)
                    TestXMLSerialization(node);
            }
            System.Console.WriteLine("Passed XML serialization");
            foreach (var grp in Data.TexGen.TextureGenDocument.NodeGroups)
            {
                foreach (var node in grp.Types)
                    TestBinarySerialization(node);
            }
            System.Console.WriteLine("Passed binary serialization");
        }

        public static void TestBinarySerialization(Type type)
        {
            Data.Graph.Graph g = new Data.Graph.Graph();
            var node = Activator.CreateInstance(type) as Data.TexGen.TexGenNode;
            node.Construct();
            g.AddNode(node);

            // Write into a document
            var ctx = new Data.SerializationContext(null);
            using (var memStream = new System.IO.MemoryStream())
            {
                using (var writer = new System.IO.BinaryWriter(memStream))
                {
                    g.Serialize(ctx, writer);
                }

                using (var subMemStream = new System.IO.MemoryStream(memStream.ToArray()))
                {
                    using (var reader = new System.IO.BinaryReader(subMemStream))
                    {
                        Data.Graph.Graph newGraph = new Data.Graph.Graph();
                        newGraph.Deserialize(ctx, reader);
                    }
                }
            }
        }

        public static void TestXMLSerialization(Type type)
        {
            Data.Graph.Graph g = new Data.Graph.Graph();
            var node = Activator.CreateInstance(type) as Data.TexGen.TexGenNode;
            node.Construct();
            g.AddNode(node);

            // Write into a document
            var ctx = new Data.SerializationContext(null);
            XmlDocument doc = new XmlDocument();
            var root = doc.CreateElement("texturegraph");
            doc.AppendChild(root);
            g.Serialize(ctx, root);

            // Load from that document
            Data.Graph.Graph newGraph = new Data.Graph.Graph();
            newGraph.Deserialize(ctx, root);
        }
    }
}
