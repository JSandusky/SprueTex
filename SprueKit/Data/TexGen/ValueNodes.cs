using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SprueKit.Data.Graph;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;
using System.ComponentModel;

namespace SprueKit.Data.TexGen
{
    public class TexGenNode : GraphNode, IPermutable
    {
        public event EventHandler PermutationsChanged;
        public void SignalPermutationChange() { if (PermutationsChanged != null) PermutationsChanged(this, null); }

        System.Windows.Media.Imaging.BitmapSource preview_;

        [PropertyData.PropertyIgnore]
        [Notify.TrackMember(IsExcluded = true)]
        [XmlIgnore]
        public int TaskCounter { get; set; } = 0;

        int prevDepth_ = 0;
        public TexGenNode()
        {
        }

        [Notify.TrackMember(IsExcluded=true)]
        [PropertyData.PropertyIgnore]
        public System.Windows.Media.Imaging.BitmapSource Preview {
            get { return preview_; }
            set {
                //if (preview_ != null)
                //    preview_.Dispose();
                preview_ = value;
                OnPropertyChanged();
            }
        }

        #region Permutations
        Dictionary<string, PermutationSet> permutations_ = new Dictionary<string, PermutationSet>();
        [PropertyData.PropertyIgnore]
        [Notify.TrackMember(IsExcluded =true)]
        public Dictionary<string, PermutationSet> Permutations { get { return permutations_; } }
        [PropertyData.PropertyIgnore]
        [Notify.TrackMember(IsExcluded = true)]
        public IEnumerable<PermutationRecord> FlatPermutations { get { return PermutationRecord.GetRecords(permutations_); } }
        #endregion

        public void RefreshPreview(int width, int height, Func<float, bool> callback = null)
        {
            ++prevDepth_;
            Preview = GeneratePreview(width, height, callback).GetBitmapImage();
            if (Preview != null)
                Preview.Freeze();
            --prevDepth_;
        }

        public virtual System.Drawing.Bitmap GeneratePreview(int width, int height, Func<float,bool> callback = null)
        {
            if (Graph == null)
                return null;
            var bmp = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            int ctx = 0;
            var targetSockets = OutputSockets.Count > 0 ? OutputSockets : InputSockets;
            if (targetSockets.Count > 0)
            {
                for (int y = 0; y < height; ++y)
                {
                    if ((y % 4) == 0 && callback != null)
                    {
                        if (!callback(y / (float)height))
                            return null;
                    }
                    for (int x = 0; x < width; ++x)
                    {
                        ctx = (y + 1) * (x + 1);
                        ExecuteUpstream(ctx, new Vector4(x / ((float)width), y / ((float)height), width, height));
                        bmp.SetPixel(x, y, targetSockets[0].GetColor().ToDrawingColor());
                    }
                }
            }
            return bmp;
        }

        public float CalculateStepSize(float stepSize, Vector4 coordinates)
        {
            return (1.0f / Math.Max(coordinates.Z, coordinates.W)) * stepSize;
        }

        
    }

    [Description("Outputs a grayscale value")]
    public partial class FloatNode : TexGenNode
    {
        float value_ = 0.5f;
        [PropertyData.AllowPermutations]
        [Description("Value to output")]
        public float Value { get { return value_; } set { value_ = value; OnPropertyChanged(); } }
        public FloatNode()
        {
        }

        public override void Construct()
        {
            Name = "Float";
            AddOutput(new GraphSocket(this) {
                IsInput = false,
                IsOutput = true,
                TypeID = SocketTypeID.Channel });
        }

        public override void Execute(object param)
        {
            OutputSockets[0].Data = new Color(Value, Value, Value, 1.0f);
        }
    }

    [Description("Outputs a constant color")]
    public partial class ColorNode : TexGenNode
    {
        Color color_ = Color.Red;
        [PropertyData.AllowPermutations]
        [Description("Value to output")]
        public Color Color { get { return color_; } set { color_ = value; OnPropertyChanged(); } }

        public ColorNode() {
        }

        public override void Construct()
        {
            Name = "Color";
            AddOutput(new GraphSocket(this)
            {
                TypeID = SocketTypeID.Color,
                IsInput = false,
                IsOutput = true
            });
        }

        public override void Execute(object param)
        {
            OutputSockets[0].Data = Color;
        }
    }

    [Description("Outputs values from a raster image")]
    public partial class TextureNode : TexGenNode
    {
        [PropertyData.PropertyIgnore]
        [Notify.TrackMember(IsExcluded =true)]
        public System.Windows.Media.Imaging.BitmapSource Thumbnail { get; set; }

        SprueBindings.ImageData data_;
        Uri imageFile_;
        bool bilinearFilter_ = true;
        bool gradientFilter_ = false;

        [PropertyData.ResourceTag(Type = PropertyData.ResourceTagType.RasterTexture)]
        [Description("Source texture to use for sampling")]
        public Uri Texture
        {
            get { return imageFile_; }
            set {
                imageFile_ = value;
                data_ = null;
                if (value != null)
                {
                    data_ = SprueBindings.ImageData.Load(value.AbsolutePath, ErrorHandler.inst());
                    if (data_ == null)
                    {
                        imageFile_ = null;
                        Thumbnail = null;
                    }
                    else
                    {
                        Thumbnail = BindingUtil.ToBitmap(data_, 512);
                    }
                }
                OnPropertyChanged("Thumbnail");
                OnPropertyChanged();
            }
        }

        [Description("Whether bilinear filtering (smoothing) will be used when sampling the image")]
        public bool BilinearFilter { get { return bilinearFilter_; } set { bilinearFilter_ = value; OnPropertyChanged(); } }

        [Description("Y coordinates will not be filtered, only X coordinates will. Useful for gradient lookup tables where it is undesirable to filter between different LUTs")]
        public bool GradientFilter { get { return gradientFilter_; } set { gradientFilter_ = value; OnPropertyChanged(); } }

        public TextureNode()
        {
        }

        public override void Construct()
        {
            Name = "Texture";
            AddOutput(new GraphSocket(this) { Name = "Out", IsOutput = true, IsInput = false, TypeID = SocketTypeID.Channel });
            AddOutput(new GraphSocket(this) { Name = "R", IsOutput = true, IsInput = false, TypeID = SocketTypeID.Grayscale });
            AddOutput(new GraphSocket(this) { Name = "G", IsOutput = true, IsInput = false, TypeID = SocketTypeID.Grayscale });
            AddOutput(new GraphSocket(this) { Name = "B", IsOutput = true, IsInput = false, TypeID = SocketTypeID.Grayscale });
            AddOutput(new GraphSocket(this) { Name = "A", IsOutput = true, IsInput = false, TypeID = SocketTypeID.Grayscale });
        }

        ~TextureNode()
        {
        }

        public override void Execute(object param)
        {
            Vector4 retColor = Vector4.One;
            if (data_ != null)
            {
                Vector4 p = (Vector4)param;
                Vector2 coord = new Vector2(p.X, p.Y);
                if (bilinearFilter_ && !gradientFilter_)
                    retColor = data_.GetPixelBilinear(coord.X, coord.Y).ToVector4();
                else if (bilinearFilter_ && gradientFilter_)
                    retColor = data_.GetPixelBilinearX(coord.X, Mathf.Wrap((int)(coord.Y * data_.Height), 0, data_.Height - 1)).ToVector4();
                else
                    retColor = data_.GetPixel(Mathf.Wrap((int)(coord.X * data_.Width), 0, data_.Width - 1), Mathf.Wrap((int)(coord.Y * data_.Height), 0, data_.Height - 1)).ToVector4();
            }

            OutputSockets[0].Data = retColor;
            OutputSockets[1].Data = retColor.X;
            OutputSockets[2].Data = retColor.Y;
            OutputSockets[3].Data = retColor.Z;
            OutputSockets[4].Data = retColor.W;
        }
    }

    [Description("Splits apart 'ID' or 'cluster' map images into unique channels")]
    public partial class IDMapGenerator : TexGenNode
    {
        [PropertyData.PropertyIgnore]
        [Notify.TrackMember(IsExcluded = true)]
        public System.Windows.Media.Imaging.BitmapSource Thumbnail { get; set; }

        SprueBindings.ImageData data_;
        List<Color> colors_ = new List<Color>();
        Uri imageFile_;

        [PropertyData.ResourceTag(Type = PropertyData.ResourceTagType.RasterTexture)]
        [Description("Unique colors in the image will be isolated into unique masks")]
        public Uri Texture
        {
            get { return imageFile_; }
            set
            {
                imageFile_ = value;
                data_ = null;
                if (value != null)
                {
                    data_ = SprueBindings.ImageData.Load(value.AbsolutePath, ErrorHandler.inst());
                    if (data_ == null)
                    {
                        imageFile_ = null;
                        Thumbnail = null;
                    }
                    else
                    {
                        Thumbnail = BindingUtil.ToBitmap(data_, 512);
                    }
                }
                // WARNING: this is dependent on the current order of serialization
                // Deserializing paths before other fields could be good ... investigate
                if (NodeID != -1)
                    UpdateSockets();
                OnPropertyChanged("Thumbnail");
                OnPropertyChanged();
            }
        }

        public IDMapGenerator()
        {
        }
        public override void Construct()
        {
            base.Construct();
            Name = "ID Map";
        }

        ~IDMapGenerator()
        {
        }

        public override void PrimeBeforeExecute(object param)
        {
            
        }

        void UpdateSockets()
        {
            colors_.Clear();
            if (data_ != null)
            {
                for (int y = 0; y < data_.Height; ++y)
                {
                    for (int x = 0; x < data_.Width; ++x)
                    {
                        Color col = data_.GetPixel(x, y);
                        if (!colors_.Contains(col))
                            colors_.Add(col);
                    }
                }
            }
            foreach (var socket in OutputSockets)
                socket.DisconnectAll();
            OutputSockets.Clear();

            foreach (var col in colors_)
                AddOutput(new GraphSocket(this) { Name = col.ToString(), IsInput = false, IsOutput = true, TypeID = SocketTypeID.Grayscale });
        }

        public override void Execute(object param)
        {
            foreach (var sock in OutputSockets)
                sock.Data = 0.0f;

            if (data_ != null)
            {
                Vector4 p = (Vector4)param;
                Vector2 coord = new Vector2(p.X, p.Y) * new Vector2(data_.Width, data_.Height);
                var sampled = data_.GetPixel((int)coord.X, (int)coord.Y);
                int index = colors_.IndexOf(sampled);
                if (index != -1)
                    OutputSockets[index].Data = 1.0f;
            }
        }

        public override void PostDeserialize()
        {
            for (int i = 1; i < OutputSockets.Count; ++i)
                colors_.Add(OutputSockets[i].Name.ToColor());
        }

        public override void PostClone(GraphNode source)
        {
            //??
        }
    }

    [Description("Outputs a rasterization of an SVG vector graphics file")]
    public partial class SVGNode : TexGenNode
    {
        [PropertyData.PropertyIgnore]
        [Notify.TrackMember(IsExcluded = true)]
        public System.Windows.Media.Imaging.BitmapSource Thumbnail { get; set; }

        int width_ = 256;
        int height_ = 256;
        SprueBindings.ImageData data_;
        Uri imageFile_;

        [Description("Width in pixels to rasterize the SVG file to")]
        public int Width { get { return width_; } set { width_ = value; SVGDataChanged();  OnPropertyChanged(); } }
        [Description("Heigh tin pixels to rasterize the SVG file to")]
        public int Height { get { return height_; } set { height_ = value; SVGDataChanged(); OnPropertyChanged(); } }

        [PropertyData.ResourceTag(Type = PropertyData.ResourceTagType.SVGTexture)]
        [Description("Source SVG file to use")]
        public Uri Texture
        {
            get { return imageFile_; }
            set
            {
                imageFile_ = value;
                data_ = null;
                if (value != null)
                {
                    SVGDataChanged();
                }
                OnPropertyChanged("Thumbnail");
                OnPropertyChanged();
            }
        }

        void SVGDataChanged()
        {
            if (imageFile_ != null && System.IO.File.Exists(imageFile_.AbsolutePath))
            {
                data_ = SprueBindings.ImageData.LoadSVG(imageFile_.AbsolutePath, Math.Max(width_, 1), Math.Max(height_, 1), ErrorHandler.inst());
                if (data_ == null)
                {
                    imageFile_ = null;
                    Thumbnail = null;
                }
                else
                {
                    Thumbnail = BindingUtil.ToBitmap(data_, 512);
                }
                OnPropertyChanged("Thumbnail");
            }
        }

        public override void Construct()
        {
            base.Construct();
            Name = "SVG File";
            AddOutput(new Data.Graph.GraphSocket(this) { Name = "Out", IsInput = false, IsOutput = true, TypeID = SocketTypeID.Color });
        }

        public override void Execute(object param)
        {
            if (data_ != null)
            {
                Vector4 coord = (Vector4)param;
                OutputSockets[0].Data = data_.GetPixelBilinear(coord.X, coord.Y);
            }
            else
                OutputSockets[0].Data = 0.0f;
        }
    }

    [Description("Final output texture to be used")]
    public partial class TextureOutputNode : TexGenNode
    {
        TextureChannel outputChannel_;
        Color defaultColor_ = Color.Black;
        PluginLib.IntVector2 targetSize_ = new PluginLib.IntVector2(256, 256);
        PluginLib.IntVector2 exportSize_ = new PluginLib.IntVector2(512, 512);

        [Description("Purpose of this output in the final render")]
        public TextureChannel OutputChannel { get { return outputChannel_; } set { outputChannel_ = value; OnPropertyChanged(); } }
        [Description("Color that will be used whenever there are no connections")]
        public Color DefaultColor { get { return defaultColor_; } set { defaultColor_ = value; OnPropertyChanged(); } }
        [Description("Image size to use when rendering previews")]
        public PluginLib.IntVector2 PreviewSize { get { return targetSize_; } set { targetSize_ = value; OnPropertyChanged(); } }
        [PropertyData.VisualConsequence(PropertyData.VisualStage.None)]
        [Description("Image size to use when generating the final exported image")]
        public PluginLib.IntVector2 TargetSize { get { return exportSize_; } set { exportSize_ = value; } }

        public TextureOutputNode()
        {
        }

        public override void Construct()
        {
            Name = "Output";
            EntryPoint = true;
            AddInput(new GraphSocket(this) { Name = "Final pixel", TypeID = SocketTypeID.Channel });
        }

        public override void PrimeBeforeExecute(object param)
        {
            InputSockets[0].Data = defaultColor_;
        }

        public override void Execute(object param)
        {
            if (InputSockets[0].Data == null)
                InputSockets[0].Data = defaultColor_;
        }
    }

    public class WarpingNode : TexGenNode
    {

    }

    [Description("Used to connect disparate parts of the graph to aid in a clean graph. Allows it's input to reached by 'Warp In' nodes with the same 'WarpKey'")]
    public partial class WarpOut : WarpingNode
    {
        string warpKey_ = "Default Warp";
        [PropertyData.AllowPermutations]
        [Description("Key used for designating this warp node, should be unique")]
        public string WarpKey { get { return warpKey_; } set { warpKey_ = value; OnPropertyChanged("DisplayName"); OnPropertyChanged(); } }

        [NonSerialized]
        public object data_;

        public override void Construct()
        {
            base.Construct();
            Name = "Warp Out";
            AddInput(new GraphSocket(this) { Name = "In", TypeID = SocketTypeID.Channel });
        }

        public override string DisplayName { get { return string.Format("{0}:\n{1}", Name, WarpKey); } }

        public override void Execute(object param)
        {
            data_ = InputSockets[0].Data;
        }
    }

    [Description("Used to connect disparate parts of the graph to aid in a clean graph. Receives values from a 'Warp Out' node with the same 'Warp Key'")]
    public partial class WarpIn : WarpingNode
    {
        WarpOut warp_;
        string warpKey_ = "Default Warp";
        [PropertyData.AllowPermutations]
        [Description("Used to find which 'Warp out' node to get a value from")]
        public string WarpKey { get { return warpKey_; } set { warpKey_ = value; OnPropertyChanged("DisplayName"); OnPropertyChanged(); } }

        public override void Construct()
        {
            base.Construct();
            Name = "Warp In";
            AddOutput(new GraphSocket(this) { IsOutput = true, IsInput = false, Name = "Out", TypeID = SocketTypeID.Channel });
        }

        public override string DisplayName { get { return string.Format("{0}:\n{1}", Name, WarpKey); } }

        public override void PrimeBeforeExecute(object param)
        {
            string myLower = WarpKey.ToLower();
            var foundNode = Graph.Nodes.FirstOrDefault(n => n is WarpOut && ((WarpOut)n).WarpKey.ToLowerInvariant().Equals(myLower));
            if (foundNode != null)
                warp_ = (WarpOut)foundNode;
        }

        public override void Execute(object param)
        {
            if (warp_ != null)
            {
                warp_.ExecuteUpstream(lastExecutionContext, param);
                OutputSockets[0].Data = warp_.data_;
            }
            else
                OutputSockets[0].Data = null;
        }
    }

    [Description("Enforces the validity of PBR albedo values. Includes an optional alert mode to indicate where values are incorrect")]
    public partial class PBREnforcer : ChannelInOutNode
    {
        bool alertMode_ = false;
        bool strictMode_ = true;

        [Description("Results will display bright red where values are too low and bright green where too high")]
        public bool AlertMode { get { return alertMode_; } set { alertMode_ = value; OnPropertyChanged(); } }
        [Description("Raises the lower bound from 30 to 50")]
        public bool StrictMode { get { return strictMode_; } set { strictMode_ = value; OnPropertyChanged(); } }

        public override void Construct()
        {
            base.Construct();
            Name = "PBR Enforcer";
        }

        public override void Execute(object param)
        {
            var value = InputSockets[0].GetColor().ToVector4();
            bool anyLow = false;
            bool anyHigh = false;

            float lowerBound = StrictMode ? 50.0f / 255.0f : 30.0f / 255.0f;
            float upperBound = 240.0f / 255.0f;

            value.X = TestValue(value.X, lowerBound, upperBound, ref anyLow, ref anyHigh);
            value.Y = TestValue(value.Y, lowerBound, upperBound, ref anyLow, ref anyHigh);
            value.Z = TestValue(value.Z, lowerBound, upperBound, ref anyLow, ref anyHigh);
            if (AlertMode)
            {
                if (anyHigh)
                    value = new Vector4(0, 1, 0, 1);
                else if (anyLow)
                    value = new Vector4(1, 0, 0, 1);
            }
            OutputSockets[0].Data = value;
        }

        float TestValue(float value, float lowerBound, float upperBound, ref bool anyLow, ref bool anyHigh)
        {
            if (value < lowerBound)
            {
                anyLow = true;
                value = lowerBound;
            }
            else if (value > upperBound)
            {
                anyHigh = true;
                value = upperBound;
            }
            return value;
        }
}
}
