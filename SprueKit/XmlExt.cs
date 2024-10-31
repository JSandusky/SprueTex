using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SprueKit
{
    public static class XmlExt
    {
        public static XmlElement CreateChild(this XmlElement element, string elemName)
        {
            var elem = element.OwnerDocument.CreateElement(elemName);
            element.AppendChild(elem);
            return elem;
        }

        public static void AddStringElement(this XmlElement element, string elemName, string text)
        {
            var elem = element.OwnerDocument.CreateElement(elemName);
            element.AppendChild(elem);
            elem.InnerText = text;
        }

        public static void AddImageElement(this XmlElement element, string elemName, System.Drawing.Bitmap img)
        {
            try
            {
                System.IO.MemoryStream stream = new System.IO.MemoryStream();
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                byte[] data = stream.ToArray();
                var elem = element.OwnerDocument.CreateElement(elemName);
                element.AppendChild(elem);
                elem.InnerText = Convert.ToBase64String(data);
            }
            catch (Exception ex)
            {

            }
        }

        public static void AddEnumElement<T>(this XmlElement element, string elemName, T value)
        {
            var elem = element.OwnerDocument.CreateElement(elemName);
            element.AppendChild(elem);
            elem.InnerText = value.ToString();
        }

        public static string GetStringElement(this XmlElement element, string elemName, string defaultText = null)
        {
            var found = element.SelectSingleNode(elemName);
            if (found != null)
                return found.InnerText;
            return defaultText;
        }

        public static bool GetBoolElement(this XmlElement element, string elemName, bool defaultValue = false)
        {
            var found = element.SelectSingleNode(elemName);
            if (found != null)
                return bool.Parse(found.InnerText);
            return defaultValue;
        }

        public static float GetFloatElement(this XmlElement element, string elemName, float defaultValue = 0)
        {
            var found = element.SelectSingleNode(elemName);
            if (found != null)
                return float.Parse(found.InnerText);
            return defaultValue;
        }

        public static int GetIntElement(this XmlElement element, string elemName, int defaultValue = 0)
        {
            var found = element.SelectSingleNode(elemName);
            if (found != null)
                return int.Parse(found.InnerText);
            return defaultValue;
        }

        public static uint GetUIntElement(this XmlElement element, string elemName, uint defaultValue = 0)
        {
            var found = element.SelectSingleNode(elemName);
            if (found != null)
                return uint.Parse(found.InnerText);
            return defaultValue;
        }

        public static uint Value_GetUInt(this XmlElement element)
        {
            uint ret = 0;
            if (uint.TryParse(element.InnerText, out ret))
                return ret;
            return ret;
        }

        public static T GetEnumElement<T>(this XmlElement element, string elemName, T defaultValue) where T : struct
        {
            var found = element.SelectSingleNode(elemName);
            if (found != null)
                return (T)Enum.Parse(typeof(T), found.InnerText);
            return defaultValue;
        }

        public static T TryFetch<T>(this Dictionary<string, object> src, string key, T defVal)
        {
            if (src.ContainsKey(key))
            {
                object outVal = null;
                if (src.TryGetValue(key, out outVal))
                {
                    if (outVal != null && outVal.GetType() == defVal.GetType())
                        return (T)outVal;
                }
            }
            return defVal;
        }

        public static KeyValuePair<XmlDocument, List<XmlElement> > FetchElements(string xmlPath, params string[] nodes)
        {
            List<XmlElement> elements = new List<XmlElement>();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlPath);
                foreach (string str in nodes)
                {
                    XmlNodeList list = doc.DocumentElement.SelectNodes(string.Format("//{0}", str));
                    if (list.Count > 0)
                    {
                        foreach (var elem in list)
                        {
                            XmlElement asElem = elem as XmlElement;
                            if (asElem != null)
                                elements.Add(asElem);
                        }
                    }
                }
                return new KeyValuePair<XmlDocument, List<XmlElement>>(doc, elements);
            } catch (Exception ex)
            {
                // just eat this one, high probability
            }
            return new KeyValuePair<XmlDocument, List<XmlElement>>();
        }

        public static int GetIntAttribute(this XmlElement elem, string attr, int defaultval = 0)
        {
            int val = defaultval;
            if (elem.HasAttribute(attr))
                int.TryParse(elem.GetAttribute(attr), out val);
            return val;
        }
    }
}
