#include "API.h"

#include <SprueEngine/Core/Context.h>
#include <SprueEngine/Geometry/MeshData.h>

using namespace SprueEngine;

#define MARSHAL_VECTOR(TARGET, FIELD, TYPE, COUNT) TARGET.resize(FIELD != 0x0 ? COUNT : 0); if (COUNT > 0) memcpy(TARGET.data(), FIELD, sizeof(TYPE) * COUNT)

void ToMeshData(GeometryInterop* interopData, MeshData* targetMesh)
{
    MARSHAL_VECTOR(targetMesh->GetPositionBuffer(), interopData->Positions, Vec3, interopData->VertexCount);
    MARSHAL_VECTOR(targetMesh->GetNormalBuffer(), interopData->Normals, Vec3, interopData->VertexCount);
    MARSHAL_VECTOR(targetMesh->GetTangentBuffer(), interopData->Tangents, Vec3, interopData->VertexCount);
    MARSHAL_VECTOR(targetMesh->GetUVBuffer(), interopData->UV, Vec2, interopData->VertexCount);
    MARSHAL_VECTOR(targetMesh->GetIndexBuffer(), interopData->indices, unsigned, interopData->IndexCount);
    MARSHAL_VECTOR(targetMesh->GetBoneWeightBuffer(), interopData->BoneWeights, Vec4, interopData->VertexCount);
    MARSHAL_VECTOR(targetMesh->GetBoneIndexBuffer(), interopData->BoneIndices, IntVec4, interopData->VertexCount);
}

struct TestObject_Stuff
{
    float Array[5] = { 0.0f, 0.25f, 0.5f, 0.75f, 1.0f };
};

const auto testFunc = [](int stuff, int tim = 1) -> float {
    return 3;
};

void SprueEngine_Init()
{

    struct SubObject {
        float x;
        float y;
        float z;

        float thing() {
            return x * y * z;
        }
    };

    SubObject bob;
    bob.x = 3;
    bob.y = 2;
    bob.z = 1.0f;

    testFunc(bob.thing());
}

void SprueEngine_StoreGeometry(const char* name, ModelInterop* data)
{

}

void SprueEngine_DeleteModelData(ModelInterop* data)
{

}

// Contouring API
void SprueEngine_ContourNaiveSurfaceNets()
{

}

void SprueEngine_ContourDC()
{

}

// Processing API
ModelInterop* SprueEngine_MergeCSGMeshes(ModelInterop* models, int* opCodes, int modelCount)
{

    return 0x0;
}

void SprueEngine_GenerateUVCoordinates(ModelInterop* model, float gutter, float stretch, int quality)
{

}

void SprueEngine_GenerateBoneWeights(ModelInterop* model)
{

}

// Filtering API
void SprueEngine_Smooth(ModelInterop* model, float power)
{

}

void SprueEngine_Tesselate(ModelInterop* model, int levels)
{

}