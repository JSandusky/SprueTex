#pragma once

#include <SprueEngine/ClassDef.h>

namespace SprueEngine
{

    /// Base-class for sources that generate voxel data.
    /// Implementations include:
    ///     Procedural2DSource (generates a heightfield)
    ///     ProceduralStrata (generates a strata)
    ///     Procedural3DSource (similar to Procedural2D, just 3d instead of 2d)
    ///     SVOGenerator (uses a sparse voxel octree, mostly used for user modifications)
    ///     GraphGenerator (evaluates an L-system like structure, should always be ephemeral and cooked into an SVO)
    ///     PrimitivesGenerator (evaluates a list of primitives)
    ///     
    class SPRUE VoxelSource
    {
    public:
    };

    class SPRUE VoxelWorld
    {

    };

}