using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace SprueKit.Data
{
    [Serializable]
    public class Folder
    {
        Uri uri_;

        [XmlIgnore]
        public Uri Path { get { return uri_; } set { uri_ = value; } }

        public string FolderPath {
            get {
                return uri_ != null ? uri_.ToString() : "";
            }

            set
            {
                uri_ = new Uri(value);
            }
        }

        [XmlIgnore]
        public bool IsValid
        {
            get
            {
                if (uri_ == null)
                    return false;
                return System.IO.Directory.Exists(uri_.AbsoluteUri);
            }
        }
    }
}
