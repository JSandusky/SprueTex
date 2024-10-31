using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;

namespace SprueKit.Settings
{
    [EnumNames("Naive Surface Nets,Dual Contouring,Dual Marching Cubes")]
    public enum MeshingMode
    {
        NaiveSurfaceNets,
        DualContouring,
        DualMarchingCubes
    }

    [EnumNames("64^3,128^3")]
    public enum MeshingResolution
    {
        [Description("64^3")]
        Small,
        [Description("128^3")]
        Large,
    }

    [Serializable]
    public class MeshingSettings
    {
        [Description("The voxel surface algorithm to use, list is ordered by speed")]
        public MeshingMode MeshingMode { get; set; } = MeshingMode.NaiveSurfaceNets;
        [Description("Size of the voxel grid to compute, larger = slower")]
        public MeshingResolution VoxelGridSize { get; set; } = MeshingResolution.Small;
        [Description("Unit dimensions of each voxel")]
        public float VoxelSize { get; set; } = 1.0f;
        [Description("OpenCL will be used to accelerate voxel meshing")]
        public bool GPUAcceleration { get; set; } = true;
    }

    public enum UVQuality
    {
        Automatic,
        Fast,
        Best,
        TurboSloppy
    }

    [Serializable]
    public class UVGenerationSettings
    {
        [Description("Determines how long to process charts for")]
        public UVQuality ChartQuality { get; set; } = UVQuality.Automatic;
        [Description("Spacing to include between charts to allow for mipmapping bleed")]
        public float Gutter { get; set; } = 2.0f;
        [Description("How much stretch is tolerated in the resulting UVs")]
        public float StretchTolerance { get; set; } = 1.2f;
    }
}
