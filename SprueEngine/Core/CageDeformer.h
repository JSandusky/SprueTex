#pragma once

#include <SprueEngine/Core/SceneObject.h>
#include <SprueEngine/BlockMap.h>

namespace SprueEngine
{

class Context;

/// Cage deformer's use a lattice cage in order to deform sampling that occurs within them
class CageDeformer : public SceneObject
{
    BASECLASSDEF(CageDeformer, SceneObject);
    NOCOPYDEF(CageDeformer);
    SPRUE_EDITABLE(CageDeformer);
public:
    CageDeformer();
    virtual ~CageDeformer();

    static void Register(Context*);

    virtual void Deform(Vec3&) const override;

    void GetGridSize(unsigned& x, unsigned& y, unsigned& z) const { x = gridSizeX_; y = gridSizeY_; z = gridSizeZ_; }
    unsigned GetCellCount() const { return (gridSizeX_ - 1) * (gridSizeY_ - 1) * (gridSizeZ_ - 1); }
    void SetGridSize(unsigned x, unsigned y, unsigned z);

    unsigned GetNumberOfPoints() const { return gridSizeX_ * gridSizeY_ * gridSizeZ_; }

    virtual bool AcceptsChildren() const { return false; }

    virtual BoundingBox ComputeBounds() const override;

protected:
    virtual void GetGizmos(std::vector<std::shared_ptr<Gizmo> >& gizmos) override;

private:
    struct CageVertexGizmo;
    struct Tetrahedron
    {
        unsigned Indices[4];

        void Set(unsigned v0, unsigned v1, unsigned v2, unsigned v3)
        {
            Indices[0] = v0;
            Indices[1] = v1;
            Indices[2] = v2;
            Indices[3] = v3;
        }

        bool Contains(const CageDeformer* pointSource, const Vec3& pt) const;
        void CalculateWeights(const CageDeformer* pointSource, const Vec3& pt, float* weights) const;
    };

    void ComputeTetrahedra() const;

    unsigned gridSizeX_;
    unsigned gridSizeY_;
    unsigned gridSizeZ_;
    std::vector<Vec3> initialRig_;
    FilterableBlockMap<Vec3> initialBlockMap_;
    FilterableBlockMap<Vec3> deformedBlockMap_;
    std::vector<Vec3> deformedRig_;
    /// Will be precomputed as necessary
    mutable std::vector<Tetrahedron> tetrahedrons_;
};
    

}