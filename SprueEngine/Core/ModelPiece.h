#pragma once

#include <SprueEngine/Math/IntersectionInfo.h>
#include <SprueEngine/Core/SpruePieces.h>

namespace SprueEngine
{

    class DistanceField;
    class MeshResource;

    enum ModelPieceMergeMode
    {
        MPMM_Merge = 0,
        MPMM_Independent = 1,
        MPMM_CSGAdd = 2,
        MPMM_CSGSubstract = 3,
        MPMM_CSGIntersect = 4,
        MPMM_ClipThenMerge = 5,
        MPMM_ClipIndependently = 6
    };

    /// Uses a signed distance field for a piece of geometry (such as an OBJ file etc)
    SHARP_BIND class SPRUE ModelPiece : public SkeletalPiece
    {
        BASECLASSDEF(ModelPiece, SkeletalPiece);
        NOCOPYDEF(ModelPiece);
        SPRUE_EDITABLE(ModelPiece);
    public:
        ModelPiece();
        virtual ~ModelPiece();

        static void Register(Context*);

        ResourceHandle GetMeshResourceHandle() const { return meshResourceHandle_; }
        void SetMeshResourceHandle(const ResourceHandle& handle) { meshResourceHandle_ = handle; }

        std::shared_ptr<MeshResource> GetMeshData() const { return meshResource_; }
        void SetMeshData(const std::shared_ptr<MeshResource>& data) { meshResource_ = data; }

        ModelPieceMergeMode GetMergeMode() const { return mergeMode_; }
        void SetMergeMode(ModelPieceMergeMode mode) { mergeMode_ = mode; }

        bool UseUVs() const { return useUVs_; }
        void SetUseUVs(bool value) { useUVs_ = value; }

        float GetCSGSmoothing() const { return smoothCSG_; }
        void SetCSGSmoothing(float value) { smoothCSG_ = value; }

        virtual BoundingBox ComputeBounds() const override;

        virtual bool TestRayAccurate(const Ray& ray, IntersectionInfo* info) const override;

        virtual void DrawDebug(IDebugRender* renderer, unsigned flags = 0) const override;

    protected:
        virtual float GetDensity(const Vec3&) const override;
        virtual unsigned CalculateStructuralHash() const override;

    private:
        /// Mesh data used for the distance field construction, also found in the distance field itself
        std::shared_ptr<MeshResource> meshResource_;
        /// Handle for the mesh resource
        ResourceHandle meshResourceHandle_;
        /// How to combine this model into the aggregate SprueModel
        ModelPieceMergeMode mergeMode_ = MPMM_Merge;
        /// Whether to use the UV coordinates included in the model or not, only effective if the UVs are present
        bool useUVs_ = true;
        /// If merged via CSG then the seams will be smoothed by this power.
        float smoothCSG_ = 0.5f;
    };

}