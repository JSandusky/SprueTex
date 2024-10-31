using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SprueKit.Data.Sprue
{
    public class SprueSelectionCommands : DependencyObject
    {
        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(Commands.CommandInfo[]), typeof(SprueSelectionCommands));

        public Commands.CommandInfo[] Items { get { return (Commands.CommandInfo[])GetValue(ItemsProperty); } set { SetValue(ItemsProperty, value); } }

        static Dictionary<Type, Commands.CommandInfo[]> ItemsTable = new Dictionary<Type, Commands.CommandInfo[]>
        {
            { typeof(ChainPiece), new Commands.CommandInfo[] {
                new Commands.CommandInfo { Name="Add Bone", ToolTip = "Add bone", Icon = WPFExt.GetEmbeddedImage("Images/Commands/add_bone.png")  },
            } },

            { typeof(ChainPiece.ChainBone), new Commands.CommandInfo[] {
                new Commands.CommandInfo { Name = "Add Bone Before", ToolTip = "Add bone before", Icon = WPFExt.GetEmbeddedImage("Images/Commands/add_bone_before.png")  },
                new Commands.CommandInfo { Name = "Add Bone After",  ToolTip = "Add bone after",  Icon = WPFExt.GetEmbeddedImage("Images/Commands/add_bone_after.png") },
                new Commands.CommandInfo { Name = "Weighting Properties",  ToolTip = "Weighting properties",  Icon = WPFExt.GetEmbeddedImage("Images/Commands/add_bone_after.png") },
            } },

            { typeof(MeshBone), new Commands.CommandInfo[] {
                new Commands.CommandInfo { Name = "Weighting Properties" }
            } }
        };
    }
}
