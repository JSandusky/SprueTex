using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Data.Sculpt
{
    public class PaintLayer : BaseClass
    {
        TextureChannel channel_ = TextureChannel.Diffuse;
        TextureBlend blend_ = TextureBlend.Overwrite;

        public TextureChannel Channel
        {
            get { return channel_; }
            set { channel_ = value;  OnPropertyChanged(); }
        }

        public TextureBlend Blend
        {
            get { return blend_; }
            set { blend_ = value;  OnPropertyChanged(); }
        }
    }
}
