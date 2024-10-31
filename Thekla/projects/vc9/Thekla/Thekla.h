// Thekla.h

#pragma once

using namespace System;
using namespace Microsoft::Xna::Framework;
using namespace Microsoft::Xna::Framework::Graphics;


namespace Thekla {

    public ref class AtlasOutput
    {
    public:
        array<VertexPositionNormalTexture>^ vertices;
        array<int>^ indices;
    };

    public ref class AtlasBuilder
    {
    public:
        static AtlasOutput^ ComputeTextureCoordinates(System::Collections::Generic::List<VertexPositionNormalTexture>^ meshVertices, System::Collections::Generic::List<int>^ meshIndices);
    };
	
}
