using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SprueKit.Data
{
    [Serializable]
    public class UriList
    {
        public UriList()
        {
        }

        public bool IsFolders { get; set; } = true;

        public ObservableCollection<string> Paths { get; set; } = new ObservableCollection<string>();

        //[PropertyIgnore]
        //[XmlElement("Paths")]
        //public List<string> SerializedPaths
        //{
        //    get
        //    {
        //        List<string> ret = new List<string>();
        //        foreach (var uri in Paths)
        //        {
        //            if (uri != null)
        //                ret.Add(uri.ToString());
        //        }
        //        return ret;
        //    }
        //    set
        //    {
        //        foreach (var str in value)
        //        {
        //            Uri uri = new Uri(str);
        //            Paths.Add(uri);
        //        }
        //    }
        //}
    }
}
