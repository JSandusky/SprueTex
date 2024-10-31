#include "SprueEngine/Core/SpruePieces.h"

#include "SprueEngine/IEditableProperty.h"
#include "SprueEngine/Core/Context.h"
#include "SprueEngine/BlockMap.h"
#include "SprueEngine/Deserializer.h"
#include "SprueEngine/Core/SpaceDeformer.h"
#include "SprueEngine/Core/DensityFunctions.h"
#include "SprueEngine/Voxel/DistanceField.h"
#include "SprueEngine/Math/MathDef.h"
#include "SprueEngine/Serializer.h"

#include <limits>

namespace SprueEngine
{

    const char* ShapeNames[] = {
        "Additive",       // CSG Union
        "Subtractive",    // CSG Subtract
        "Intersection",   // CSG Intersection
        "Displace",
        "Blend",
        0x0,
    };

    void DoStuff()
    {
        FilterableBlockMap<RGBA> img(128, 128);
        img.get(0, 0);
    }

SpruePiece::SpruePiece() :
    parent_(0x0),
    model_(0x0),
    mode_(SM_Additive)
{

}

void SpruePiece::Register(Context* context)
{
    context->CopyBaseProperties(StringHash("SceneObject"), StringHash("SpruePiece"));

    // TODO: implement an ENUM property
    REGISTER_ENUM(SpruePiece, ShapeMode, GetMode, SetMode, SM_Additive, "ShapeMode", "", PS_VisualConsequence, ShapeNames);
}

SpruePiece::~SpruePiece()
{
}

float SpruePiece::AdjustDensity(float currentDensity, const Vec3& pos) const
{
    if (!IsMeshed())
        return currentDensity;

    return CombineDensity(mode_, currentDensity, GetDensity(
        inverseWorldTransform_  * pos));
}

float SpruePiece::CalculateDensity(const Vec3& position) const
{  
    return AdjustDensity(1000.0f, position);
}

//=================================================

void FolderPiece::Register(Context* context)
{
    context->RegisterFactory<FolderPiece>("FolderPiece", "Folder pieces can be used to structurally organize the scene");

    // DO NOT COPY BASE ATTRIBUTES
    REGISTER_PROPERTY_CONST_SET(FolderPiece, std::string, GetName, SetName, "", "Name", "", PS_Default);
}

//=================================================

SkeletalPiece::SkeletalPiece() : base()
{
    
}

SkeletalPiece::~SkeletalPiece()
{
    
}

void SkeletalPiece::Register(Context* context)
{
    context->CopyBaseProperties(StringHash("SpruePiece"), StringHash("SkeletalPiece"));
}

//=================================================

SimplePiece::SimplePiece(DensityHandler* densityFunction) : base(),
    densityHandler_(densityFunction)
{
    
}

SimplePiece::SimplePiece() : base()
{
    SphereFunction* func = new SphereFunction();
    densityHandler_ = func;
    func->setRadius(1.0f);
    
}

SimplePiece::~SimplePiece()
{
    
}

static const char* DENSITY_NAMES[] = {
    "Sphere",
    "Box",
    "Rounded Box",
    "Capsule",
    "Cylinder",
    "Cone",
    "Capped Cone",
    "Plane",
    "Ellipsoid",
    "Torus",
    "Super Ellipsoid",
    0x0,
};

static const StringHash DENSITY_TYPES[] = {
    StringHash("SphereFunction"),
    StringHash("CubeFunction"),
    StringHash("RoundedBoxFunction"),
    StringHash("CapsuleFunction"),
    StringHash("CylinderFunction"),
    StringHash("ConeFunction"),
    StringHash("CappedConeFunction"),
    StringHash("PlaneFunction"),
    StringHash("EllipsoidFunction"),
    StringHash("TorusFunction"),
    StringHash("SuperEllipseFunction")
};

void SimplePiece::Register(Context* context)
{
    context->RegisterFactory<SimplePiece>("SimplePiece", "Provides meshing capability for several different types of basic primitive shapes");
    context->CopyBaseProperties(StringHash("SpruePiece"), StringHash("SimplePiece"));
    REGISTER_EDITABLE(SimplePiece, GetDensityHandlerProperty, SetDensityHandlerProperty, "Function", "", PS_VisualConsequence | PS_IEditableObject, DENSITY_NAMES, DENSITY_TYPES);
}

#define RAY_EPSILON 0.001f
#define MAX_RAY_STEPS 32
bool SimplePiece::TestRayAccurate(const Ray& ray, IntersectionInfo* info) const
{
    // Check if we can even collide
    if (densityHandler_ && TestRayFast(ray, 0x0))
    {
        Ray testRay = ray;
        testRay.Transform(GetWorldTransform().Inverted());
        float t = 0.0f;
        for (int i = 0; i < MAX_RAY_STEPS; ++i) {
            const Vec4 testPoint = testRay.pos + testRay.dir * t;
            float d = densityHandler_->CalculateDensity(testPoint.xyz());
            if (d < RAY_EPSILON) {
                if (info)
                {
                    info->hit = testPoint;
                    info->parent = const_cast<SceneObject*>(GetParent());
                    info->object = const_cast<SimplePiece*>(this);
                    info->t = (testPoint - testRay.pos).Length();
                }
                return true;
            }
            t += d;
        }
    }
    return false;
}

void SimplePiece::DrawDebug(IDebugRender* debug, unsigned flags) const
{
    if (densityHandler_)
    {
        RGBA color = flags & SPRUE_DEBUG_HOVER ? RGBA::Gold : RGBA::Green;
        color.a = 0.75f;
        if (flags & SPRUE_DEBUG_PASSIVE)
            color.a = 0.1f;
        densityHandler_->DrawDebug(debug, GetWorldTransform(), color);
    }
}

}