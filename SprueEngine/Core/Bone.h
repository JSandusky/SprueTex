#pragma once

#include <SprueEngine/Core/SceneObject.h>

namespace SprueEngine
{
    /// Determines how vertices will be weighted to bones.
    enum BoneSkinningMode
    {
        /// Bone will not be involved in vertex weight calculations, either a control bone or a it the weights will be manually/externally determined.
        BSM_None = 0,
        /// Basic envelope radius will be used for vertex weight calculation, basically weight is normalized closest distance to capsule formed by a bone.
        BSM_Envelope = 1,
        /// "Bone Glow: An Improved Method for the Assignment of Weights for Mesh Deformation" method in which light acculation on the surface is used. Slightly smarter envelopes.
        BSM_Automatic = 2
    };

    class SPRUE Bone : public SceneObject
    {
        BASECLASSDEF(Bone, SceneObject);
        NOCOPYDEF(Bone);
        SPRUE_EDITABLE(Bone);
    public:
        Bone();
        virtual ~Bone();
        static void Register(Context*);

        virtual BoundingBox ComputeBounds() const override;

        virtual float CalculateDensity(const Vec3& position) const { 
            SPRUE_ASSERT(false, "Meshing bones should never happen"); 
            return 0.0f; 
        }

        /// Returns the light intensity or radius that will be used for bone weights.
        float GetGlowIntensity() const { return glowIntensity_; }
        /// Sets the light intensity or radius that will be used for bone weights.
        void SetGlowIntensity(float value) { glowIntensity_ = value; }

        /// Returns the vertex weighting method that will be used for this bone.
        BoneSkinningMode GetSkinningMode() const { return skinningMode_; }
        /// Sets the vertex weighting method that will be used for this bone.
        void SetSkinningMode(BoneSkinningMode mode) { skinningMode_ = mode; }

        bool IsBlurringAllowed() const { return !canBlurWeights_; }
        void SetBlurringAllowed(bool state) { canBlurWeights_ = state; }

    private:
        /// How vertex weights will be calculated (if at all) for this bone.
        BoneSkinningMode skinningMode_;
        /// The intensity of the glow for automatic bone weighting.
        float glowIntensity_;
        /// Controls whether weights for this bone are allowed to be blurred. Allows finer control.
        bool canBlurWeights_ = true;
    };

}