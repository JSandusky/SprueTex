#pragma once

using namespace System;

#include "Geometry.h"
#include "ImageData.h"

namespace SprueBindings
{

    public delegate bool BakingProgressCallback(float);

    public ref class Bakers
    {
    public:
        static ImageData^ AmbientOcclusion(array<MeshData^>^ meshData, int width, int height, BakingProgressCallback^ callback);
        static ImageData^ Thickness(array<MeshData^>^ meshData, int width, int height, BakingProgressCallback^ callback);
        static ImageData^ Curvature(array<MeshData^>^ meshData, int width, int height, BakingProgressCallback^ callback);
        static ImageData^ DominantPlane(array<MeshData^>^ meshData, int width, int height, BakingProgressCallback^ callback);
        static ImageData^ Facet(array<MeshData^>^ meshData, int width, int height, float angleTolerance, bool forceAllEdges, BakingProgressCallback^ callback);
        static ImageData^ ObjectSpaceGradient(array<MeshData^>^ meshData, int width, int height, BakingProgressCallback^ callback);
        static ImageData^ ObjectSpaceNormal(array<MeshData^>^ meshData, int width, int height, BakingProgressCallback^ callback);
        static ImageData^ ObjectSpacePosition(array<MeshData^>^ meshData, int width, int height, BakingProgressCallback^ callback);
    };

}