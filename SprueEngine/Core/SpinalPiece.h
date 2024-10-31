#pragma once

#include <SprueEngine/Math/IntersectionInfo.h>
#include <SprueEngine/Core/SpruePieces.h>
#include <SprueEngine/Core/PolyObject.h>

namespace SprueEngine
{
    /// A stretchable piece that behaves like a spine/tail constructing bones automatically along it's length
    /// The length and thickness of intervals is configurable
    SHARP_BIND class SPRUE SpinalPiece : public SHARP_BASE SkeletalPiece, public PolyObject
    {
        BASECLASSDEF(SpinalPiece, SkeletalPiece);
        NOCOPYDEF(SpinalPiece);
        SPRUE_EDITABLE(SpinalPiece);
    public:
        SpinalPiece();
        virtual ~SpinalPiece();

        static void Register(Context*);

        /// If all dims are zero then the shape is a cone point
        /// If two dims are zero than it's a broad spade
        /// If one dim is zero than it's, a leaf-shaped spade
        struct Vertebrae
        {
            Vec3 dim_;
            Vec3 pos_;
        };

        /// Add a vertebrae to the front
        Vertebrae* AddHeadBone();
        /// Add a vertebrae to the reat
        Vertebrae* AddTailBone();
        /// Remove a vertebrae if allowed
        bool RemoveBone(Vertebrae* vert);

        /// Modifications should not be performed on this vector
        std::vector<Vertebrae*>& GetVertebrae() { return vertebrae_; }
        /// Get the list of vertebrae
        const std::vector<Vertebrae*>& GetVertebrae() const { return vertebrae_; }

        virtual BoundingBox ComputeBounds() const override;

        virtual bool TestRayAccurate(const Ray& ray, IntersectionInfo* info) const override;

        virtual void DrawDebug(IDebugRender* debug, unsigned flags = 0) const override;

        VariantVector GetPoints() const;
        void SetPoints(const VariantVector& pts);

        float GetBoneMinLength() const { return boneMinLength_; }
        void SetBoneMinLength(float value) { boneMinLength_ = value; }
        float GetAutoInsertionThreshold() const { return autoInsertionThreshold_; }
        void SetAutoInsertionThreshold(float value) { autoInsertionThreshold_ = value; }

        bool IsMarkAsSpine() const { return markAsSpine_; }
        void SetMarkAsSpine(bool value) { markAsSpine_ = value; }

        unsigned GetNearestVertebrae(const Vec3& point) const;

        virtual void WriteParameters(Serializer* serializer);

        virtual bool IsCircular() const override { return false; }

    protected:
        virtual float GetDensity(const Vec3&) const override;
        virtual unsigned CalculateStructuralHash() const override;
        virtual void GetGizmos(std::vector<std::shared_ptr<Gizmo> >& gizmos) override;

    private:
        class SpinalGizmo;

        std::vector<Vertebrae*> vertebrae_;

        float autoInsertionThreshold_;
        float boneMinLength_;
        bool markAsSpine_ = true;
    };

    /// A vertex in SpinalPiece. SpinalPieces use vertebrae for the points in the chain so that they can have child objects (muscles, etc) that will be properly attached.
    SHARP_BIND class SPRUE SpinalVertebrae : public SpruePiece
    {
        BASECLASSDEF(SpinalVertebrae, SpruePiece);
        NOCOPYDEF(SpinalVertebrae);
        SPRUE_EDITABLE(SpinalVertebrae);
    public:
        /// Construct.
        SpinalVertebrae();
        /// Destruct.
        virtual ~SpinalVertebrae();

        /// Registery factory and properties.
        static void Register(Context*);

        /// Returns whether or not this bone turns into a real skeleton bone.
        bool IsOutputBone() const { return outputBone_; }
        /// Sets whether or not this bone turns into a real skeleton bone.
        void SetOutputBone(bool value) { outputBone_ = value; }
        /// Returns true if this bone was created as part of an automatic insertion method.
        bool WasAutoPlaced() const { return autoPlaced_; }
        /// Sets whether this bone is flagged as having been created as part of an automatic insertion method.
        void SetAutoPlaced(bool value) { autoPlaced_ = value; }

        /// Returns the XYZ ellipsoid dimensions for this bone.
        Vec3 GetDimensions() const { return dim_; }
        /// Sets the XYZ ellipsoid dimensions for this bone.
        void SetDimensions(const Vec3&  dim) { dim_ = dim; }

        virtual BoundingBox ComputeBounds() const override;

        /// Only SpinalPiece objects may receive SpinalVertebrae objects.
        virtual bool AcceptAsParent(const SceneObject* possible) const override;
        /// Cannot be rearranged or moved in the tree, period.
        virtual bool CanChangeHierarchy() const override { return false; }

    protected:
        virtual float GetDensity(const Vec3& pos) const override;

    private:
        /// Dimensions for this bone.
        Vec3 dim_;
        /// Whether this vert will produce a bone.
        bool outputBone_ = true;
        /// Was this vertex inserted as part of an automatic insertion method?
        bool autoPlaced_ = false;
    };
}