using System;

using IntVector2 = PluginLib.IntVector2;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;
using Color = Microsoft.Xna.Framework.Color;

namespace SprueKit.Data.TexGen
{
    public class CachingNode : TexGenNode
    {
        protected Vector2 cacheScale_ = new Vector2(1, 1);
        public Vector2 CacheScale { get { return cacheScale_; } set { cacheScale_ = value; OnPropertyChanged(); } }

        [NonSerialized]
        protected Vector2 cacheDim_ = Vector2.Zero;
        [NonSerialized]
        protected int graphHashCode_ = -1;
        [NonSerialized]
        protected SprueBindings.ImageData cache_;

        public SprueBindings.ImageData GetCache() { return cache_; }

        ~CachingNode()
        {
        }

        public override void Construct()
        {
            base.Construct();
            AddInput(new Data.Graph.GraphSocket(this) { Name = "In", TypeID = Data.Graph.SocketTypeID.Channel });
            AddOutput(new Data.Graph.GraphSocket(this) { IsOutput = true, IsInput = false, Name = "Out", TypeID = Data.Graph.SocketTypeID.Channel });
        }

        public override void PrimeBeforeExecute(object param)
        {
            int graphHash = Graph.StructuralHash();
            if (graphHashCode_ == -1 || graphHash != graphHashCode_)
            {
                cache_ = null;
                graphHashCode_ = graphHash;
            }
        }

        public override bool WillForceExecute() { return true; }

        protected virtual void PrepareCache(int width, int height)
        {
            if (cache_ != null)
                return;

            width =  Math.Max((int)(width * cacheScale_.X), 1);
            height = Math.Max((int)(height * cacheScale_.Y), 1);
            cache_ = new SprueBindings.ImageData(width, height);
            //cache_ = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            for (int y = 0; y < height; ++y)
            {
                float yy = y / (float)height;
                for (int x = 0; x < width; ++x)
                {
                    float xx = x / (float)width;
                    ForceExecuteUpstream(new Vector4(xx, yy, width, height));
                    cache_.SetPixel(x, y, InputSockets[0].GetColor());
                }
            }
        }
    }

    [PropertyData.PropertyIgnore("CacheScale")]
    public partial class SampleControl : CachingNode
    {
        IntVector2 cacheSize_ = new IntVector2(128, 128);
        public IntVector2 CacheSize { get { return cacheSize_; } set { cacheSize_ = value; OnPropertyChanged(); } }

        public override void Construct()
        {
            base.Construct();
            Name = "Sample Control";
        }

        public override void Execute(object param)
        {
            PrepareCache(CacheSize.X, CacheSize.Y);
            Vector4 p = (Vector4)param;
            OutputSockets[0].Data = cache_.GetPixelBilinear(p.X, p.Y);
        }
    }
}
