using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace SprueKit
{
    /// <summary>
    /// Contains the saved userdata about project paths / settings
    /// Saved system 'appdata' location
    /// </summary>
    [Serializable]
    public class UserData : BaseClass
    {
        static UserData inst_;

        public static UserData inst()
        {
            if (inst_ == null)
                inst_ = LoadUserData();
            return inst_;
        }

        public UserData()
        {
        }

        public void AddRecentFile(Uri file)
        {
            RecentFiles.Paths.Remove(file.AbsolutePath);
            RecentFiles.Paths.Add(file.AbsolutePath);
            Save();
        }

        public void ClearRecentFiles()
        {
            Save();
        }

        #region INotifyPropertyChanged
        public override void AllPropertiesChanged()
        {
            base.OnPropertyChanged();
            Save();
        }
        #endregion

        public Data.UriList RecentFiles { get; set; } = new Data.UriList() { IsFolders = false };

        bool firstRun = true;
        public bool IsFirstRun { get { return firstRun; } set { firstRun = value; OnPropertyChanged(); } }

        public Settings.GeneralSettings GeneralSettings { get; set; } = new Settings.GeneralSettings();
        public Settings.TextureGraphSettings TextureGraphSettings { get; set; } = new Settings.TextureGraphSettings();
        public Settings.ViewportSettings ViewportSettings { get; set; } = new Settings.ViewportSettings();
        public Settings.MeshingSettings MeshingSettings { get; set; } = new Settings.MeshingSettings();
        public Settings.UVGenerationSettings UVGenerationSettings { get; set; } = new Settings.UVGenerationSettings();

        public Settings.BitFieldNames BitNames { get; set; } = new Settings.BitFieldNames();

        #region Serialization only

        void PreSerialize()
        {
            
        }
        void PostDeserialize()
        {
            //??if (AssetFolders.Count == 0)
            //??    AssetFolders.Add(new Data.FolderData() { Path = new Uri(App.ProgramPath("StockAssets"), UriKind.Absolute) });
        }
        #endregion

        public void Save()
        {
            string fileName = App.DataPath("UserData.xml");
            try
            {
                System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(UserData));
                PreSerialize();
                using (System.IO.FileStream file = new System.IO.FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    writer.Serialize(file, this);
                    file.Flush();
                    file.Close();
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.inst().Error(ex);
            }
        }

        static UserData LoadUserData()
        {
            string fileName = App.DataPath("UserData.xml");
            try
            {
                if (File.Exists(fileName))
                {
                    System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(UserData));

                    UserData ud = new UserData();
                    using (System.IO.FileStream file = new System.IO.FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        ud = (UserData)reader.Deserialize(file);
                        ud.PostDeserialize();
                        file.Close();
                    }

                    return ud;
                }
                return new UserData();
            }
            catch (Exception ex)
            {
                ErrorHandler.inst().Error(ex);
                return new UserData();
            }
            finally { }
        }
    }

}
