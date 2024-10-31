using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit
{
    public abstract class DocumentRecord : BaseClass
    {
        /// <summary>
        /// Keyed for GUI for the button
        /// </summary>
        public abstract string DocumentName { get; }
        /// <summary>
        /// "My File Name (*.xml)|*.xml
        /// </summary>
        public abstract string OpenFileMask { get; }

        public abstract Document CreateNewDocument();
        public abstract Document CreateFromTemplate(Uri templateUri);
        public abstract Document OpenDocument(Uri documentUri);

        public abstract KeyValuePair<string,string>[] GetSignificantReports();

        public abstract void DoReport(string value);
    }
}
