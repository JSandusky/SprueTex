#include "SQMMeshGenerator.h"

#include <SprueEngine/Core/Bone.h>
#include <SprueEngine/Core/Context.h>

namespace SprueEngine
{
    /// Visits the skeletal bones at and beneath an SQMMeshGenerator component's parent.
    class SQMVisitor : public TypeLimitedSceneObjectVisitor<Bone>
    {
    public:
        SQMVisitor(Bone* rootBone, SQMMeshGenerator* generator) :
            rootBone_(rootBone),
            generator_(generator)
        {
        }

        virtual bool Visit(Bone* child) override
        {
            // Get the properties for this bone, if null the default settings in the generator will be used.
            SQMBoneProperties* properties = child->GetFirstChildOfType<SQMBoneProperties>();
        }

        Bone* rootBone_;
        SQMMeshGenerator* generator_;
    };


    SQMMeshGenerator::SQMMeshGenerator()
    {

    }
    
    SQMMeshGenerator::~SQMMeshGenerator()
    {

    }

    void SQMMeshGenerator::Register(Context* context)
    {
        context->RegisterFactory<SQMMeshGenerator>("SQMMeshGenerator", "Constructs a mesh from a hierarchy of bones using the SQM technique");
        context->CopyBaseProperties(StringHash("Component"), StringHash("SQMMeshGenerator"));
        REGISTER_PROPERTY(SQMMeshGenerator, float, GetDefaultBoneRadius, SetDefaultBoneRadius, 1.0f, "Default Bone Radius", "If a bone lacks an SQM Bone Properties component this value will be used for radius", PS_Default);
        REGISTER_PROPERTY(SQMMeshGenerator, unsigned, GetSmoothingIterations, SetSmoothingIterations, 1, "Smoothing Iterations", "How many mesh smoothing/subdivision passes will be performed on the generated mesh", PS_Default);
        REGISTER_PROPERTY(SQMMeshGenerator, float, GetDefaultSmoothingPower, SetDefaultSmoothingPower, 1.0f, "Default Smoothing Power", "How intensely smoothing may occur on bones without SQM Bone Properties components", PS_Default);
    }

    bool SQMMeshGenerator::AcceptAsParent(const SceneObject* possible) const
    {
        return dynamic_cast<const Bone*>(possible) != 0x0;
    }

    SQMBoneProperties::SQMBoneProperties()
    {

    }
    
    SQMBoneProperties::~SQMBoneProperties()
    {

    }

    void SQMBoneProperties::Register(Context* context)
    {
        context->RegisterFactory<SQMMeshGenerator>("SQMBoneProperties", "Provides bone properties for use with an SQM Mesh Generator");
        context->CopyBaseProperties(StringHash("Component"), StringHash("SQMBoneProperties"));
        REGISTER_PROPERTY(SQMBoneProperties, float, GetRadius, SetRadius, 1.0f, "Radius", "The radius of the node used in constructing the meshes between bones", PS_Default);
        REGISTER_PROPERTY(SQMBoneProperties, float, GetSmoothingPower, SetSmoothingPower, 1.0f, "Smoothing Power", "How intensely smoothing iterations may smooth vertices created by the bone", PS_Default);
    }
}