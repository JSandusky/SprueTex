#pragma once

#include <SprueEngine/Core/Component.h>

namespace SprueEngine
{

    /// SQM/BMM based mesh generator.
    /// May only be placed on a bone, child bones will be processed for generating an SQM mesh.
    class SQMMeshGenerator : public Component
    {
        NOCOPYDEF(SQMMeshGenerator);
        BASECLASSDEF(SQMMeshGenerator, Component);
        SPRUE_EDITABLE(SQMMeshGenerator);
    public:
        /// Construct.
        SQMMeshGenerator();
        /// Destruct.
        virtual ~SQMMeshGenerator();

        /// Register factory and properties.
        static void Register(Context* context);

        /// Ensures this component can only be attached to Bones.
        virtual bool AcceptAsParent(const SceneObject* possible) const override;

        /// Returns the default radius to assume if a bone down the tree lacks an SQMBoneProperties component.
        float GetDefaultBoneRadius() const { return defaultBoneRadius_; }
        /// Sets the default radius to assume if a bone down the tree lacks an SQMBoneProperties component.
        void SetDefaultBoneRadius(float value) { defaultBoneRadius_ = value; }

        /// Returns how many subdivision passes will be applied.
        unsigned GetSmoothingIterations() const { return smoothingIterations_; }
        /// Sets how many subdivision passes will be applied.
        void SetSmoothingIterations(unsigned value) { smoothingIterations_ = value; }

        /// Returns the smoothing power that will be used if an SQMBoneProperties component does not exist.
        float GetDefaultSmoothingPower() const { return defaultSmoothingPower_; }
        /// Sets the smoothing power that will be used if an SQMBoneProperties component does not exist.
        void SetDefaultSmoothingPower(float value) { defaultSmoothingPower_ = value; }

        virtual BoundingBox ComputeBounds() const override { return BoundingBox(); }

    private:
        /// If we don't find an SQMBoneProperties component on a bone being processed then this radius will be used.
        float defaultBoneRadius_ = 1.0f;
        float defaultSmoothingPower_ = 1.0f;
        unsigned smoothingIterations_ = 1;
    };

    /// Provides processing parameters to an SQMMeshGenerator component that is upwards in the bone hierarchy.
    /// May only be placed on a bone.
    class SQMBoneProperties : public Component
    {
        NOCOPYDEF(SQMBoneProperties);
        BASECLASSDEF(SQMBoneProperties, Component);
        SPRUE_EDITABLE(SQMBoneProperties);
    public:
        /// Construct.
        SQMBoneProperties();
        /// Destruct.
        virtual ~SQMBoneProperties();

        /// Register factory and properties.
        static void Register(Context* context);

        float GetRadius() const { return radius_; }
        void SetRadius(float value) { radius_ = value; }

        float GetSmoothingPower() const { return smoothingPower_; }
        void SetSmoothingPower(float value) { smoothingPower_ = value; }

        virtual BoundingBox ComputeBounds() const override { return BoundingBox(); }

    private:
        /// The radius of the polyhredral node used for BMM/SQM
        float radius_ = 1.0f;
        /// How intensely smoothing is allowed to work on vertices created by this node.
        float smoothingPower_ = 1.0f;
    };

}