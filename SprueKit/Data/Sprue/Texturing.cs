using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml;

namespace SprueKit.Data
{
    /// <summary>
    /// An individual image in a material to be used
    /// </summary>
    public class TextureMap : BaseClass
    {
        class TexRecord
        {
            public BitmapSource thumb;
            public SprueBindings.ImageData image;
        }
        static Dictionary<Uri, TexRecord> TextureCache = new Dictionary<Uri, TexRecord>();

        BitmapSource thumb_;
        SprueBindings.ImageData texture_;
        Uri imagePath_;
        TextureChannel channel_ = TextureChannel.Diffuse;
        TextureBlend blending_ = TextureBlend.Overwrite;
        TexturePass pass_ = TexturePass.Standard;
        bool useAlpha_ = false;

        [PropertyData.PropertyIgnore]
        public BitmapSource Thumbnail { get { return thumb_; } }
        [PropertyData.PropertyIgnore]
        public SprueBindings.ImageData Texture { get { return texture_; } }

        public Uri Image
        {
            get { return imagePath_; }
            set {
                imagePath_ = value;
                UpdateTexture();
                SetupGui();
                OnPropertyChanged();
            }
        }

        public TextureChannel Channel {
            get { return channel_; }
            set { channel_ = value;  OnPropertyChanged(); }
        }

        public TextureBlend Blending
        {
            get { return blending_; }
            set { blending_ = value;  OnPropertyChanged(); }
        }

        public TexturePass Pass
        {
            get { return pass_; }
            set { pass_ = value;  OnPropertyChanged(); }
        }

        public bool UseAlpha
        {
            get { return useAlpha_; }
            set { useAlpha_ = value;  OnPropertyChanged(); }
        }

        public void Write(SerializationContext context, XmlElement into)
        {
            var myElem = into.CreateChild("TextureMap");
            myElem.AddStringElement("image", context.GetRelativePath(Image).ToString());
            myElem.AddEnumElement<TextureChannel>("channel", Channel);
            myElem.AddEnumElement<TexturePass>("pass", Pass);
            myElem.AddEnumElement<TextureBlend>("blending", Blending);
            myElem.AddStringElement("use_alpha", UseAlpha.ToString());
        }

        public void Read(SerializationContext context, XmlElement from)
        {
            string imgPath = from.GetStringElement("image");
            if (!string.IsNullOrEmpty(imgPath))
            {
                Uri relUri = new Uri(imgPath);
                relUri = context.GetAbsolutePath(relUri, this, "Texture", "Image", FileData.ImageFileMask);
                if (relUri != null)
                    Image = relUri;
            }
            channel_ = from.GetEnumElement<TextureChannel>("channel", TextureChannel.Diffuse);
            pass_ = from.GetEnumElement<TexturePass>("pass", TexturePass.Standard);
            blending_ = from.GetEnumElement<TextureBlend>("blending", TextureBlend.Overwrite);
            useAlpha_ = from.GetBoolElement("use_alpha", false);
        }

        public void Write(SerializationContext context, BinaryWriter writer)
        {
            writer.Write(context.GetRelativePath(Image).ToString());
            writer.Write((int)Channel);
            writer.Write((int)Blending);
            writer.Write((int)Pass);
            writer.Write(UseAlpha);
        }

        public void Read(SerializationContext context, BinaryReader reader)
        {
            Image = reader.ReadUri();
            Channel = (TextureChannel)reader.ReadInt32();
            Blending = (TextureBlend)reader.ReadInt32();
            Pass = (TexturePass)reader.ReadInt32();
            UseAlpha = reader.ReadBoolean();
        }

        void UpdateTexture()
        {
            if (imagePath_ != null)
            {
                if (TextureCache.ContainsKey(imagePath_))
                {
                    var record = TextureCache[imagePath_];
                    if (record != null)
                    {
                        texture_ = record.image;
                        thumb_ = record.thumb;
                        OnPropertyChanged("Thumbnail");
                        return;
                    }
                }

                texture_ = SprueBindings.ImageData.Load(imagePath_.AbsolutePath, ErrorHandler.inst());
                if (texture_ != null)
                {
                    thumb_ = BindingUtil.ToBitmap(texture_);
                    TextureCache[imagePath_] = new TexRecord { thumb = thumb_, image = texture_ };
                }
                else
                    thumb_ = null;
                OnPropertyChanged("Thumbnail");
            }
            else
            {
                texture_ = null;
                thumb_ = null;
            }
        }

        #region GUI
        FileSystemWatcher fileWatcher_;

        void SetupGui()
        {
            if (fileWatcher_ != null)
                fileWatcher_.Dispose();
            fileWatcher_ = null;
            if (Image == null)
                return;

            string path = Image.AbsolutePath;
            if (System.IO.File.Exists(path))
            {
                fileWatcher_ = new FileSystemWatcher(System.IO.Path.GetDirectoryName(path), System.IO.Path.GetFileName(path)) { IncludeSubdirectories = false };
                // file changed
                fileWatcher_.Changed += (object sender, FileSystemEventArgs args) => {
                    TextureCache.Remove(Image);
                    TextureCache[Image] = null;
                    App.Current.Dispatcher.Invoke(() => {
                        UpdateTexture();
                        OnPropertyChanged("Image");
                    });
                };
                // file was renamed
                fileWatcher_.Renamed += (object sender, RenamedEventArgs args) => {
                    // editing tool hidden tmp files hack
                    if (args.OldName.EndsWith(".tmp"))
                    {
                        if (new Uri(args.FullPath).Equals(Image))
                        {
                            TextureCache.Remove(imagePath_);
                            TextureCache[Image] = null;
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                UpdateTexture();
                                OnPropertyChanged("Image");
                            });
                        }
                    }
                    else
                        Image = new Uri(args.FullPath);
                };
                fileWatcher_.EnableRaisingEvents = true;
            }
        }

        
        #endregion
    }
}
