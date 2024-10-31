using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = Microsoft.Xna.Framework.Color;

namespace SprueKit.Data.Graph
{
    /// <summary>
    /// A graph box is meaningless as far graph evaluation and execution is concerned.
    /// It is sole a visual piece of data though in theory it could be used to 
    /// cluster nodes for reporting and analysis purposes.
    /// </summary>
    public class GraphBox : BaseClass
    {
        string name_ = string.Empty;
        string note_ = string.Empty;
        double visualX_ = 0.0;
        double visualY_ = 0.0;
        double visualWidth_ = 128.0;
        double visualHeight_ = 128.0;
        Color boxColor_ = Color.DarkBlue;

        public string Name { get { return name_; } set { name_ = value; OnPropertyChanged(); } }
        public string Note { get { return note_; } set { note_ = value; OnPropertyChanged(); } }
        public Color BoxColor { get { return boxColor_; } set { boxColor_ = value; OnPropertyChanged(); } }

        public double VisualX { get { return visualX_; } set { visualX_ = value; OnPropertyChanged(); } }
        public double VisualY { get { return visualY_; } set { visualY_ = value; OnPropertyChanged(); } }
        public double VisualWidth { get { return visualWidth_; } set { visualWidth_ = value; OnPropertyChanged(); } }
        public double VisualHeight { get { return visualHeight_; } set { visualHeight_ = value; OnPropertyChanged(); } }
    }
}
