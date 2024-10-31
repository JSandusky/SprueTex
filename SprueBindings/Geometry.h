#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace Microsoft::Xna::Framework;

namespace SprueEngine
{
    class MeshData;
}

namespace SprueBindings
{
    public delegate long UVCallback(float);

    public ref class MeshMorphTarget
    {
    public:
        System::String^ Name;
        array<Vector3>^ Positions;
        array<Vector3>^ Normals;
        array<Vector4>^ Tangents;
        array<int>^ Indices;
    };

    public ref class MeshData
    {
    public:
        MeshData();
        MeshData(SprueEngine::MeshData*);
        ~MeshData();

        String^ Name = gcnew String("<unnamed>");
        array<Vector3>^ Positions;
        array<Vector3>^ Normals;
        array<Vector4>^ Tangents;
        array<Vector2>^ UVCoordinates;
        array<int>^ Indices;
        array<Vector4>^ BoneWeights;
        array<PluginLib::IntVector4>^ BoneIndices;
        array<MeshMorphTarget^>^ MorphTargets;
        PluginLib::SkeletonData^ Skeleton;

        void WriteToAPI();
        void ReadFromAPI();

        bool CalculateNormals();
        bool CalculateTangents();
        bool NormalizeBoneWeights();
        bool Subdivide(bool smooth);
        bool Decimate(float intensity);
        bool Smooth(float power);
        bool TransformUV(Vector2 offset, Vector2 scale);
        bool ComputeNormalUVs();
        bool ComputeUVCoordinates(int width, int height, int quality, float stretch, float gutter, UVCallback^ callback, PluginLib::IErrorPublisher^ errors);

        SprueEngine::MeshData* GetInternalMeshData() { return meshData_; }

    private:
        SprueEngine::MeshData* meshData_;
    };

    public ref class ModelData
    {
    public:
        ModelData() { }
        ~ModelData();
        List<MeshData^>^ Meshes;

        /// Determines what type of model to load based on file extension
        static ModelData^ LoadModel(System::String^ path, PluginLib::IErrorPublisher^ errorOutput);
        static ModelData^ LoadFBX(System::String^ path, PluginLib::IErrorPublisher^ errorOutput);
        static ModelData^ LoadOBJ(System::String^ path, PluginLib::IErrorPublisher^ errorOutput);

        static bool SaveFBX(ModelData^ model, System::String^ path, bool newest, PluginLib::IErrorPublisher^ errorOutput);
        static bool SaveDAE(ModelData^ model, System::String^ path, PluginLib::IErrorPublisher^ errorOutput);
        static bool SaveOBJ(ModelData^ model, System::String^ path, PluginLib::IErrorPublisher^ errorOutput);
        static bool SaveGEX(ModelData^ model, System::String^ path, PluginLib::IErrorPublisher^ errorOutput);
    };
}