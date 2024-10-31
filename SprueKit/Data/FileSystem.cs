using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;

namespace SprueKit.Data
{
    public class FolderData : BaseClass
    {
        Uri uri_;
        FileSystemWatcher watcher_;

        public FolderData()
        {

        }

        public static void FromStringList(List<string> strings, ObservableCollection<FolderData> data)
        {
            foreach (var str in strings)
                data.Add(new Data.FolderData { Path = new Uri(str, UriKind.Absolute) });
        }

        public static List<string> ToStringList(ObservableCollection<FolderData> data)
        {
            List<string> ret = new List<string>();
            foreach (var d in data)
                ret.Add(d.Path.AbsolutePath);
            return ret;
        }

        public FolderData(System.IO.DirectoryInfo dir)
        {
            Path = new Uri(dir.FullName, UriKind.Absolute);
        }

        public Uri Path { get { return uri_; }
            set
            {
                uri_ = value;
                ChildFolders.Clear();

                string absPath = uri_.AbsolutePath;
                if (!System.IO.Directory.Exists(absPath))
                    return;

                if (watcher_ != null)
                    watcher_.Dispose();
                watcher_ = new FileSystemWatcher(absPath);
                watcher_.IncludeSubdirectories = false;
                watcher_.Changed += (object sender, FileSystemEventArgs e) => {
                    Uri pathUri = new Uri(e.FullPath);
                    foreach (var file in this.Files)
                    {
                        if (file.Path.Equals(pathUri))
                            file.InvalidateMeta();
                    }
                };
                watcher_.Created += FileSystemChange;
                watcher_.Deleted += FileSystemChange;
                watcher_.Renamed += (object sender, RenamedEventArgs e) => {
                    Uri oldUri = new Uri(e.OldFullPath);
                    foreach (var file in this.Files)
                    {
                        if (file.Path.Equals(oldUri))
                        {
                            file.Path = new Uri(e.FullPath);
                            file.AllPropertiesChanged();
                            break;
                        }
                    }
                };
                watcher_.EnableRaisingEvents = true;
                UpdateFolders();
                OnPropertyChanged();
            }
        }

        void FileSystemChange(object sender, FileSystemEventArgs e)
        {
            UpdateFolders();
        }

        static string[] rejectedExtension =
        {
            ".smeta",
            ".db",
            ".sys"
        };
        void UpdateFolders()
        {
            if (System.Windows.Application.Current == null)
                return;
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    Files.Clear();
                    ChildFolders.Clear();
                    foreach (var fileName in System.IO.Directory.EnumerateFiles(uri_.AbsolutePath))
                    {
                        // skip metafiles and windows thumbnails files
                        if (rejectedExtension.Contains(System.IO.Path.GetExtension(fileName)))
                            continue;
                        Files.Add(new FileData(fileName));
                    }

                    //??foreach (var dirName in System.IO.Directory.EnumerateDirectories(uri_.AbsolutePath))
                    //??{
                    //??    System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(dirName);
                    //??    ChildFolders.Add(new FolderData(di));
                    //??}
                }
                catch (Exception ex)
                {

                }
            });
        }

        public string FolderPath
        {
            get { return Uri.UnescapeDataString(Path.AbsolutePath); }
        }

        public string PathString {
            get
            {
                return Uri.UnescapeDataString(Path.AbsolutePath).Substring(Path.AbsolutePath.LastIndexOf('/') + 1).SplitCamelCase();
            }
        }

        public ObservableCollection<FolderData> ChildFolders { get; private set; } = new ObservableCollection<FolderData>();

        public ObservableCollection<FileData> Files { get; private set; } = new ObservableCollection<FileData>();

        public BitmapSource Thumbnail
        {
            get
            {
                try
                {
                    var folder = Microsoft.WindowsAPICodePack.Shell.ShellFileSystemFolder.FromFolderPath(FolderPath);
                    if (folder != null)
                        return folder.Thumbnail.BitmapSource;
                }
                catch (Exception ex)
                {
                    ErrorHandler.inst().PublishError(ex);
                }
                return null;
            }
        }
    }

    public class MetaData
    {
        static readonly char[] MetaSplit = { ':' };
        public Dictionary<string, string> MetaFields { get; set; } = new Dictionary<string, string>();
        public MetaData(string filePath)
        {
            string[] lines = System.IO.File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                string[] parts = line.Split(MetaSplit, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                    continue;

                MetaFields[parts[0].Trim()] = parts[1].Trim();
            }
        }

        public bool ContainsMetaFieldValue(string value)
        {
            string lCaseTerm = value.ToLowerInvariant();
            foreach (var meta in MetaFields)
            {
                if (meta.Key.Equals("thumbnail"))
                    continue;

                if (meta.Value.ToLowerInvariant().Contains(lCaseTerm))
                    return true;
            }
            return false;
        }
    }

    public class FileData : BaseClass
    {
        public static Regex FileMaskToRegex(string sFileMask)
        {
            String convertedMask = "^" + Regex.Escape(sFileMask).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            return new Regex(convertedMask, RegexOptions.IgnoreCase);
        }

        public static string SprueModelMask = "Sprue Models (*.xml, *.sprm)|*.xml;*.sprm";
        public static string ModelFileMask = "All 3d models (*.fbx, *.obj)|*.fbx;*.obj|Autodesk FBX (*.fbx)|*.fbx|Wavefront OBJ (*.obj)|*.obj|All files (*.*)|*.*";
        public static string ImageFileMask = "All images|*.jpg;*.jpeg;*.png;*.hdr;*.tga;*.psd|All files (*.*)|*.*";
        public static string SVGFileMask = "SVG Images|*.svg";
        public static Regex[] SprueModelRegex = {
            FileMaskToRegex("*.xml"),
            FileMaskToRegex("*.sprm")
        };
        public static Regex[] ModelRegex = {
            FileMaskToRegex("*.fbx"),
            FileMaskToRegex("*.obj")
        };
        public static Regex[] ImageRegex =
        {
            FileMaskToRegex("*.jpg"),
            FileMaskToRegex("*.jpeg"),
            FileMaskToRegex("*.png"),
            FileMaskToRegex("*.hdr"),
            FileMaskToRegex("*.tga"),
            FileMaskToRegex("*.psd"),
        };
        public static Regex[] SVGRegex = { FileMaskToRegex("*.svg") };

        public static bool IsImage(string path) { return Fits(path, ImageRegex); }
        public static bool IsSVG(string path) { return Fits(path, SVGRegex); }
        public static bool IsModel(string path) { return Fits(path, ModelRegex); }
        public static bool IsSprueModel(string path) { return Fits(path, SprueModelRegex); }
        public static bool Fits(string path, Regex[] reg)
        {
            if (reg != null)
            {
                foreach (var check in reg)
                    if (check.IsMatch(path))
                        return true;
            }
            return false;
        }


        Uri uri_;
        BitmapSource thumbnailCache_;

        public FileData(string path)
        {
            uri_ = new Uri(path, UriKind.Absolute);
            ReadMeta();
        }

        public FileData(Uri path)
        {
            uri_ = path;
            ReadMeta();
        }

        public Uri Path { get { return uri_; } set { uri_ = value; OnPropertyChanged(); } }

        public string FilePath { get { return Uri.UnescapeDataString(uri_.AbsolutePath); } }

        public string ShortName { get { return FilePath.Substring(FilePath.LastIndexOf('/') + 1); } }

        public bool ContainsMetaFieldValue(string value)
        {
            foreach (var fld in MetaFields)
            {
                if (fld.ToLowerInvariant().Contains(value.ToLowerInvariant()))
                    return true;
            }
            return false;
        }

        public List<string> MetaFields { get; private set; } = new List<string>();

        public void InvalidateMeta()
        {
            MetaFields.Clear();
            lockThumb = true;
            thumbnailCache_ = null;
            triedThumbnail = false;
            ReadMeta(); // read meta will unlock the thumb
        }

        bool lockThumb = false;
        bool triedThumbnail = false;
        public BitmapSource Thumbnail {
            get {
                if (lockThumb)
                    return null;

                if (thumbnailCache_ != null)
                    return thumbnailCache_;
                else if (!triedThumbnail)
                {
                    triedThumbnail = true;

                    // Using the sync context in order to prevent:
                    // "Must create DependencySource on same Thread as the DependencyObject."
                    // BitmapSource is a freezable object, so it's weird
                    var context = TaskScheduler.FromCurrentSynchronizationContext();
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(thumbBase64))
                            {
                                byte[] data = Convert.FromBase64String(thumbBase64);
                                if (data != null && data.Length > 0)
                                {
                                    var image = new BitmapImage();
                                    using (var mem = new MemoryStream(data))
                                    {
                                        mem.Position = 0;
                                        image.BeginInit();
                                        image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                                        image.CacheOption = BitmapCacheOption.OnLoad;
                                        image.UriSource = null;
                                        image.StreamSource = mem;
                                        image.EndInit();
                                    }
                                    image.Freeze();
                                    Thumbnail = image;
                                }
                                // get our base64 png image
                            }
                            else
                                Thumbnail = Microsoft.WindowsAPICodePack.Shell.ShellFile.FromFilePath(FilePath).Thumbnail.BitmapSource;

                            OnPropertyChanged("Thumbnail");
                        }
                        catch (Exception ex)
                        {
                            ErrorHandler.inst().PublishError(ex);
                        }
                    }, CancellationToken.None, TaskCreationOptions.None, context);
                }
                return null;
            }
            private set
            {
                thumbnailCache_ = value;
                OnPropertyChanged();
            }
        }

        void ReadMeta()
        {
            Task.Run(() =>
            {
                bool hasThumb = false;
                string filePath = uri_.AbsolutePath;
                string metaPath = System.IO.Path.GetFileNameWithoutExtension(filePath);
                metaPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filePath), metaPath);
                metaPath += ".smeta";
                if (System.IO.Path.GetExtension(filePath).Equals(".xml"))
                {
                    var metaResults = XmlExt.FetchElements(filePath, "capabilities", "flags", "thumbnail");
                    if (metaResults.Key != null)
                    {
                        foreach (var elemResult in metaResults.Value)
                        {
                            uint flagValue = elemResult.Value_GetUInt();
                            if (elemResult.Name.Equals("flags"))
                                UserData.inst().BitNames.ScanCompatibleNames(MetaFields, flagValue, UserData.inst().BitNames.FlagNames);
                            else if (elemResult.Name.Equals("capabilities")) // capabilities
                                UserData.inst().BitNames.ScanCompatibleNames(MetaFields, flagValue, UserData.inst().BitNames.CapabilityNames);
                            else if (elemResult.Name.Equals("thumbnail"))
                            {
                                thumbBase64 = elemResult.InnerText;
                                hasThumb = true;
                            }
                        }
                    }
                }

                if (System.IO.File.Exists(metaPath))
                {
                    string[] metaLines = System.IO.File.ReadAllLines(metaPath);
                    foreach (string str in metaLines)
                    {
                        if (str.StartsWith("flags:"))
                            MetaFields.Add(str.Substring(6));
                        else if (str.StartsWith("capabilities:"))
                            MetaFields.Add(str.Substring("capabilities:".Length));
                        else if (str.StartsWith("preview:"))
                        {
                            thumbBase64 = str.Substring("preview:".Length);
                            hasThumb = true;
                        }
                    }
                }

                lockThumb = false;
                if (hasThumb)
                    OnPropertyChanged("Thumbnail");
            });
        }
        string thumbBase64;
    }
}
