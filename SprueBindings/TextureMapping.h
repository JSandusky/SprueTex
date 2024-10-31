#pragma once

using namespace System;
using namespace Microsoft::Xna::Framework;

#include "Geometry.h"

namespace SprueBindings
{

    public ref class TextureMappingData
    {
    public:
        int Width;
        int Height;
        array<Vector3>^ Positions;
        array<Vector3>^ Normals;
    };

    public ref class TextureMapping
    {
    public:
        static TextureMappingData^ CalculateSampleMap(SprueBindings::MeshData^ meshData, Matrix transform, int width, int height);
    };

}
