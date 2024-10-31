using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SprueKit.Data
{
    public class ReportDatabase
    {
        public ObservableCollection<ReportSettings> Reports { get; private set; } = new ObservableCollection<ReportSettings>();

        public void Serialize()
        {
            string reportsXML = App.DataPath("reports.xml");
            try
            {
                XmlDocument doc = new XmlDocument();
                var root = doc.CreateElement("reports");
                doc.AppendChild(root);

                //foreach (var rpt in ModelReports)
                //    rpt.Serialize(root);
                foreach (var rpt in Reports)
                    rpt.Serialize(root);

                doc.Save(reportsXML);
            }
            catch (Exception ex)
            {
                ErrorHandler.inst().Error(ex);
            }
        }

        public void Deserialize()
        {
            string reportsXML = App.DataPath("reports.xml");
            if (System.IO.File.Exists(reportsXML))
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(reportsXML);

                    //var modelReports = doc.DocumentElement.SelectNodes("//model_report");
                    //foreach (XmlElement rpt in modelReports)
                    //{
                    //    ModelReportSettings r = new ModelReportSettings();
                    //    r.Deserialize(rpt);
                    //    ModelReports.Add(r);
                    //}
                    var textureReports = doc.DocumentElement.SelectNodes("//texture_report");
                    foreach (XmlElement rpt in textureReports)
                    {
                        Data.Reports.TextureReportSettings r = new Data.Reports.TextureReportSettings();
                        r.Deserialize(rpt);
                        Reports.Add(r);
                    }
                }
                catch (Exception ex)
                {
                    ErrorHandler.inst().Error(ex);
                }
            }
            else
            {
                // create some default reports
            }
        }
    }

    public abstract class ReportSettings : BaseClass
    {
        string reportTitle_ = "";
        [PropertyData.PropertyPriority(0)]
        [Description("Will appear in the heading")]
        public string ReportTitle { get { return reportTitle_; } set { reportTitle_ = value; OnPropertyChanged("DisplayName"); OnPropertyChanged(); } }

        [PropertyData.PropertyIgnore]
        public string DisplayName { get { return string.IsNullOrWhiteSpace(reportTitle_) ? "<unnamed report>" : reportTitle_; } set { } }

        [Description("All files in these folders will be included in the report")]
        public UriList FolderList { get; set; } = new UriList();
        [PropertyData.IsFileList("Texture Graph File (*.txml, *.texg)|*.txml;*.texg")]
        [Description("All of these files will be included in the report")]
        public UriList FileList { get; set; } = new UriList();
        [Description("Included folders will be recursively scanned for files")]
        public bool RecurseFolders { get; set; } = false;

        public abstract void Serialize(XmlElement intoElem);
        public abstract void Deserialize(XmlElement fromElem);
    }
}
