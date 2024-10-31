using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace SprueKit.Data
{
    public class SerializationBrokenPath
    {
        public object TargetObject { get; set; }
        public string LinkType { get; set; }
        public string TargetProperty { get; set; }
        public string BrokenPath { get; set; }
        public string ExtensionMask { get; set; }

        public SerializationBrokenPath(object target, string property, string path, string type, string mask)
        {
            TargetObject = target;
            TargetProperty = property;
            BrokenPath = path;
            LinkType = type;
            ExtensionMask = mask;
        }

        /// <summary>
        /// Shows a file dialog to select a replacement file
        /// </summary>
        /// <returns></returns>
        public bool Fix(string path)
        {
            if (System.IO.File.Exists(path))
            {
                if (ExtensionMask == null || ExtensionMask.Contains(System.IO.Path.GetExtension(path)))
                {
                    var property = TargetObject.GetType().GetProperty(TargetProperty);
                    if (property != null)
                    {
                        // Did this stuff ever work?
                        //object convertedValue = Convert.ChangeType(path, property.PropertyType);
                        //if (convertedValue != null)
                        {
                            property.SetValue(TargetObject, new Uri(path));// convertedValue));
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }

    public class SerializationContext
    {
        public Uri MapRelativeTo { get; set; }

        public Uri GetRelativePath(Uri uri)
        {
            if (MapRelativeTo == null)
                return uri;
            if (uri != null)
            {
                if (uri.IsFile)
                {
                    string path = uri.AbsolutePath;
                    var dirInfo = new DirectoryInfo(System.IO.Path.GetDirectoryName(path));
                    if (dirInfo.Exists && System.IO.File.Exists(path))
                    {
                        if (Settings.GeneralSettings.CorePaths.Contains(dirInfo.Name))
                            return new Uri(string.Format("{0}://{1}", dirInfo.Name, System.IO.Path.GetFileName(path)));
                    }

                    Uri settingsFolder = new IOCDependency<Settings.GeneralSettings>().Object.CheckUri_Save(uri);
                    if (settingsFolder != null)
                        return settingsFolder;
                }
                return MapRelativeTo.MakeRelativeUri(uri);
            }
            return null;
        }

        public string GetRelativePathString(Uri uri)
        { 
            if (uri != null)
            {
                string retString = GetRelativePath(uri).ToString();
                return StripTrailingSlash(retString);
            }
            return "";
        }

        static string StripTrailingSlash(string inStr)
        {
            if (inStr.EndsWith("/"))
                return inStr.Substring(0, inStr.Length - 1);
            return inStr;
        }

        public Uri GetAbsolutePath(Uri relativeUri, object owner, string property, string type, string mask)
        {
            if (MapRelativeTo == null)
                return relativeUri;
            if (relativeUri != null)
            {
                Uri ret = relativeUri;
                if (!relativeUri.IsAbsoluteUri)
                    ret = new Uri(MapRelativeTo, relativeUri);

                foreach (var corePath in Settings.GeneralSettings.CorePaths)
                {
                    if (corePath.ToLowerInvariant().Equals(ret.Scheme))
                    {
                        string trimmed = ret.ToString().Replace(string.Format("{0}://", ret.Scheme), "");
                        if (!string.IsNullOrEmpty(trimmed))
                            ret = new Uri(string.Format("{0}/{1}", App.ProgramPath(corePath), StripTrailingSlash(trimmed)));
                    }
                }

                Uri settingsFolder = new IOCDependency<Settings.GeneralSettings>().Object.CheckUri_Read(ret);
                if (settingsFolder != null)
                    ret = settingsFolder;

                if (!System.IO.File.Exists(ret.AbsolutePath))
                {
                    BrokenPaths.Add(new SerializationBrokenPath(owner, property, Uri.UnescapeDataString(ret.AbsolutePath), type, mask));
                    return null;
                }
                return ret;
            }
            return null;
        }

        public SerializationContext(Uri relativeFolder)
        {
            MapRelativeTo = relativeFolder;
        }

        public ObservableCollection<SerializationBrokenPath> BrokenPaths { get; private set; } = new ObservableCollection<SerializationBrokenPath>();
    }
}
