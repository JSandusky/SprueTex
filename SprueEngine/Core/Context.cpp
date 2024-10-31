#include "SprueEngine/Core/Context.h"

#include <SprueEngine/Core/Bone.h>
#include <SprueEngine/Core/CageDeformer.h>
#include <SprueEngine/IContextService.h>
#include <SprueEngine/IDService.h>
#include <SprueEngine/Core/InstancePiece.h>
#include <SprueEngine/MessageLog.h>
#include <SprueEngine/Core/ModelPiece.h>
#include <SprueEngine/ResourceLoader.h>
#include <SprueEngine/ResourceStore.h>
#include <SprueEngine/Core/SpaceDeformer.h>
#include <SprueEngine/Core/SpinalPiece.h>
#include <SprueEngine/Core/SprueModel.h>
#include <SprueEngine/Core/SpruePieces.h>
#include <SprueEngine/Core/WireDeformer.h>
#include <SprueEngine/Core/BillboardCloudPiece.h>
#include <SprueEngine/Core/FinPiece.h>

#include <SprueEngine/Core/Component.h>
#include <SprueEngine/Core/Components/BoxProjector.h>
#include <SprueEngine/Core/Components/ColorCubeProjector.h>
#include <SprueEngine/Core/Components/DecalProjector.h>
#include <SprueEngine/Core/Components/GradientColorProjector.h>
#include <SprueEngine/Core/Components/IKConstraint.h>
#include <SprueEngine/Core/Components/RingProjector.h>
#include <SprueEngine/Core/Components/SnapPoint.h>
#include <SprueEngine/Core/Components/SQMMeshGenerator.h>
#include <SprueEngine/Core/Components/StripProjector.h>
#include <SprueEngine/Core/Components/TexturingComponent.h>

#include <SprueEngine/Geometry/Material.h>

// Graphing
#include <SprueEngine/Graph/Graph.h>
#include <SprueEngine/TextureGen/TextureNode.h>

//Sculpting
#include <SprueEngine/Sculpt/SculptingProject.h>

#include <bitset>

namespace SprueEngine
{

Context* Context::instance_ = 0x0;

Context::Context()
{
    new ResourceStore(this);
    new IDService(this);

    RegisterHashName("Mesh", "Mesh");
    RegisterHashName("Image", "Image");

    RegisterProperties();
    log_ = new MessageLog();
}

Context::~Context()
{
    for (auto res : resourceLoaders_)
        delete res;
    for (auto svc : attachedServices_)
        delete svc;
}

Context* Context::GetInstance() 
{
    if (instance_ == 0x0)
        instance_ = new Context();
    return instance_;
}

void Context::Register(const StringHash& typeName, TypeProperty* prop)
{
    auto found = properties_.find(typeName);
    if (found != properties_.end())
        found->second.push_back(prop);
    else
    {
        PropertyList pl;
        pl.push_back(prop);
        properties_[typeName] = pl;
    }
}

void Context::CopyBaseProperties(const StringHash& fromType, const StringHash& intoType)
{
    auto source = properties_.find(fromType);
    auto dest = properties_.find(intoType);
    if (source == properties_.end())
        return;

    if (dest != properties_.end())
        dest->second.insert(dest->second.end(), source->second.begin(), source->second.end());
    else {
        PropertyList pl;
        pl.insert(pl.end(), source->second.begin(), source->second.end());
        properties_[intoType] = pl;
    }
}

void Context::RemoveProperty(const StringHash& typeName, const char* propertyName)
{
    auto found = properties_.find(typeName);
    if (found != properties_.end())
    {
        for (PropertyList::iterator iter = found->second.begin(); iter != found->second.end(); ++iter)
        {
            if ((*iter)->GetName().compare(propertyName) == 0)
            {
                found->second.erase(iter);
                return;
            }
        }
    }
}

SprueEngine::TypeProperty* Context::GetProperty(const StringHash& fromType, const StringHash& property)
{
    auto found = properties_.find(fromType);
    if (found != properties_.end())
    {
        for (PropertyList::iterator iter = found->second.begin(); iter != found->second.end(); ++iter)
        {
            if ((*iter)->GetHash() == property)
                return (*iter);
        }
    }
    return 0x0;
}

struct ECSBaseComponent
{

};

template<typename TYPE, int IDX>
struct ECSComponent
{
public:
    static const int Index;
};

template<typename TYPE, int IDX>
const int ECSComponent<TYPE, IDX>::Index = IDX;

template<typename...TList>
struct RegisterTypes {
    RegisterTypes(SprueEngine::Context* context) {
        (void)std::initializer_list<int> { (TList::Register(context), 0)... };
    }
};

class JunkA { public: static int Index; };
class JunkB { public: static int Index; };

class JunkC : public ECSComponent<JunkC, 3> { };

int JunkA::Index = 1;
int JunkB::Index = 2;

std::bitset<32> FromBitIndices(const int* bit, int ct)
{
    std::bitset<32> ret;
    for (int i = 0; i < ct; ++i)
        ret.set(*bit++, true);
    return ret;
}

template<typename...TList>
struct ComponentMask {
    static const size_t BitCount = sizeof...(TList);
    static const int Bits[BitCount];
    static const std::bitset<32> BitSet;

    static std::bitset<32> ToBitSet()
    {
        int bits[sizeof...(TList)] = { (TList::Index)... };
        return FromBitIndices(bits, BitCount);
    }
};
template<typename...TList>
const int ComponentMask<TList...>::Bits[ComponentMask<TList...>::BitCount] = { (TList::Index)... };

template<typename...TList>
const std::bitset<32> ComponentMask<TList...>::BitSet = ComponentMask::ToBitSet();


void Context::RegisterProperties()
{
    // Register dependency types first.
    Material::Register(this);

    SceneObject::Register(this);

    // Register the SprueModel
    SprueModel::Register(this);

    Bone::Register(this);

    // Register pieces
    // Do not take chances of the compiler reordering base types
    SpruePiece::Register(this);
    SimplePiece::Register(this);
    //SkeletalPiece::Register(this);
    //SpinalPiece::Register(this);
    //ModelPiece::Register(this);
    //InstancePiece::Register(this);
    //MarkerPiece::Register(this);
    //FolderPiece::Register(this);
    //BillboardCloudPiece::Register(this);
    //FinPiece::Register(this);
    RegisterTypes<SkeletalPiece, SpinalPiece, ModelPiece, InstancePiece, MarkerPiece, FolderPiece, BillboardCloudPiece, FinPiece>(this);

    // Register deformers
    CageDeformer::Register(this);
    SpaceDeformer::Register(this);
    WireDeformer::Register(this);

    // Register components
    Component::Register(this);

        // General components
        SnapPoint::Register(this);

        // Texturing components
        TexturingComponent::Register(this);
        BoxProjector::Register(this);
        ColorCubeProjector::Register(this);
        GradientColorProjector::Register(this);
        DecalProjector::Register(this);
        RingProjector::Register(this);
        StripProjector::Register(this);

        // Physics and IK
        IKConstraint::Register(this);
    
        // Special components
        SQMBoneProperties::Register(this);
        SQMMeshGenerator::Register(this);
    
    // Register density functions
    RegisterDensityFunctions(this);

    Graph::Register(this);
    GraphNode::Register(this);
    RegisterTextureNodes(this);

// Sculpting
    RegisterSculptingTypes(this);
}

bool Context::RegisterResourceLoader(ResourceLoader* loader)
{
    resourceLoaders_.push_back(loader);
    return true;
}

ResourceLoader* Context::GetResourceLoader(const StringHash& resourceType, const char* fileName)
{
    for (auto loader : resourceLoaders_)
    {
        if (loader->GetResourceTypeID() == resourceType && loader->CanLoad(fileName))
            return loader;
    }
    return 0x0;
}

void Context::RegisterService(IContextService* svc)
{
    attachedServices_.push_back(svc);
}

std::string Context::GetHashName(const StringHash& hash)
{
    auto found = hashNames_.find(hash);
    if (found != hashNames_.end())
        return found->second;
    return std::string();
}

std::string Context::GetTypeDescription(const StringHash& hash)
{
    auto found = descriptions_.find(hash);
    if (found != descriptions_.end())
        return found->second;
    return std::string();
}

}