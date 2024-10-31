using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.Xna.Framework;

namespace PluginLib
{
    [Description("Exposes access to a set of density data for a voxel volume")]
    public interface IMeshingInterface
    {
        [Description("Turns 3d indices into a 1d index")]
        int ToCoord(int[] coords);
        [Description("Turns 3d indices into a 1d index")]
        int ToCoord(int x, int y, int z);

        [Description("Sets the density value at the given coordinate")]
        void SetDensity(
            [Description("1d index")] int coord, 
            [Description("Signed distance, positive is outside")] float density);

        [Description("Sets the hermite normals and distance at the given coordinate. XYZ components used for normal and W component used for distance")]
        void SetHermite(
            [Description("1d index")] int coord, 
            [Description("XYZ = normal, W = normalized distance along cell edge")] Vector4[] data);
    }

    [Description("Indicates the desired meshing method to use")]
    public enum MeshingPluginType
    {
        [Description("Will use Naive Surface Nets, IMeshingInterface.SetHermite will have no effect.")]
        MPT_Density,
        [Description("Will use Dual-Contouring, IMeshingInterface.SetHermite will function.")]
        MPT_Hermite
    }

    [Description("Meshing plugins generate voxel-data")]
    public interface IMeshingPlugin
    {
        [Description("Returns the desired meshing method to use for the volume")]
        MeshingPluginType MeshingType { get; }

        // Array will always be 3 elements in size
        [Description("Fills the given meshingData. Writes are performed through the IMeshingInterface")]
        bool GenerateMeshingData(
            IMeshingInterface meshingData, 
            int[] minCoord, 
            int[] maxCoord, 
            float scale, 
            IErrorPublisher reporter);
    }
}
