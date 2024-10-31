#include "SceneObject.h"

#include "SprueEngine/Core/Context.h"
#include "SprueEngine/Deserializer.h"
#include "SprueEngine/Core/SpruePieces.h"
#include "SprueEngine/Serializer.h"
#include <SprueEngine/VectorBuffer.h>

#include <SprueEngine/Core/SprueModel.h>

#include <SprueEngine/MathGeoLib/Math/TransformOps.h>

namespace SprueEngine
{

struct SceneObject::SceneObjectGizmo : public Gizmo
{
    SceneObjectGizmo(SceneObject* src) : Gizmo(src, GIZ_Translate | GIZ_Rotate | GIZ_Scale)
    {
        source_ = src;
        transform_ = src->GetWorldTransform();
    }

    virtual bool UpdateValue() override 
    {
        SceneObject* object = ((SceneObject*)source_);
        SceneObject* parent = object->GetParent();
        Mat3x4 operatingTrans;

        if (parent)
        {
            Mat3x4 parentMat = parent->inverseWorldTransform_;
            operatingTrans = parentMat * transform_;
        }
        else
            operatingTrans = transform_;
        
        Vec3 pos; Vec3 scl; Quat rot;
        operatingTrans.Decompose(pos, rot, scl);
        object->SetPosition(pos);
        object->SetRotation(rot);
        object->SetScale(scl);

        transform_ = object->GetWorldTransform();

        return true;
    }

    virtual void RefreshValue() override
    {
        SceneObject* object = ((SceneObject*)source_);
        transform_ = object->GetWorldTransform();
    }

    virtual bool Equals(Gizmo* rhs) const override {
        if (auto other = dynamic_cast<SceneObjectGizmo*>(rhs))
            return other->source_ == source_;
        return false;
    }
};

SceneObject::~SceneObject()
{
    for (auto child : children_)
        delete child;
}

void SceneObject::Register(Context* context)
{
    REGISTER_PROPERTY_CONST_SET(SceneObject, std::string, GetName, SetName, "", "Name", "", PS_Default);
    REGISTER_PROPERTY_CONST_SET(SceneObject, Vec3, GetWorldPosition, SetWorldPosition, Vec3(), "Position", "", PS_Gizmo | PS_VisualConsequence | PS_Permutable);
    REGISTER_PROPERTY_CONST_SET(SceneObject, Quat, GetWorldRotation, SetWorldRotation, Quat(), "Rotation", "", PS_Gizmo | PS_Rotation | PS_VisualConsequence | PS_Permutable);
    REGISTER_PROPERTY_CONST_SET(SceneObject, Vec3, GetWorldScale, SetWorldScale, Vec3(1, 1, 1), "Scale", "", PS_Gizmo | PS_Scale | PS_VisualConsequence | PS_Permutable);
    REGISTER_PROPERTY(SceneObject, bool, IsDisabled, SetDisabled, false, "Disabled", "Will not be included in meshing", PS_IsVisibility | PS_VisualConsequence);
    REGISTER_PROPERTY(SceneObject, bool, IsLocked, SetLocked, false, "Locked", "Prevents changing the transform of this object", PS_IsLock | PS_Permutable);
    REGISTER_PROPERTY(SceneObject, bool, IsSymmetric, SetSymmetric, false, "Symmetric", "Should be mirrored about the symmetry axis", PS_Default | PS_VisualConsequence);
    REGISTER_PROPERTY(SceneObject, unsigned, GetCapabilityBits, SetCapabilityBits, 0, "Capabilities", "User determined capability bits, denote what this object can 'do'", PS_Flags);
    REGISTER_PROPERTY(SceneObject, unsigned, GetUserBits, SetUserBits, 0, "Flags", "User determined flags, used in animation selection", PS_Flags | PS_UserFlags);
}

unsigned SceneObject::IndexOfChild(SceneObject* child) const
{
    for (unsigned i = 0; i < children_.size(); ++i)
        if (children_[i] == child)
            return i;
    return -1;
}

bool SceneObject::HasChild(SceneObject* child, bool recurse) const
{
    if (IndexOfChild(child) != -1)
        return true;
    if (recurse)
    {
        for (auto c : children_)
        {
            if (c->HasChild(child, recurse))
                return true;
        }
    }
    return false;
}

void SceneObject::AddChild(SceneObject* child) 
{ 
    // Check for no child case
    if (child == 0x0)
        return;
    child->SetParent(this); 
    children_.push_back(child); 
    Context::GetInstance()->GetCallbacks().SceneObjectAdded(child);
    UpdateTransformsRecurse(); 
}

void SceneObject::AddChildAt(SceneObject*child, unsigned index)
{
    child->SetParent(this);
    if (index > children_.size() - 1)
        AddChild(child);
    else
    {
        children_.insert(children_.begin() + index, child);
        Context::GetInstance()->GetCallbacks().SceneObjectAdded(child);
    }
}

void SceneObject::RemoveChild(SceneObject* child, bool recurse)
{
    for (auto ch = children_.begin(); ch != children_.end(); ++ch)
    {
        if ((*ch) == child)
        {
            Context::GetInstance()->GetCallbacks().SceneObjectRemoved(child);
            // nullify the parent
            child->parent_ = 0x0;
            children_.erase(ch);
            return;
        }
    }

    if (recurse)
        for (auto ch = children_.begin(); ch != children_.end(); ++ch)
            (*ch)->RemoveChild(child, recurse);
}

void SceneObject::RemoveChild(const char* name, bool recurse)
{
    for (auto child = children_.begin(); child != children_.end(); ++child)
    {
        if ((*child)->GetName().compare(name) == 0)
        {
            Context::GetInstance()->GetCallbacks().SceneObjectRemoved(*child);
            children_.erase(child);
            return;
        }
    }

    if (recurse)
        for (auto child = children_.begin(); child != children_.end(); ++child)
            (*child)->RemoveChild(name, recurse);
}

Vec3 SceneObject::GetForward() const
{
    return rotation_ * Vec3::PositiveZ;
}

Vec3 SceneObject::GetRight() const
{
    return rotation_ * Vec3::PositiveX;
}

Vec3 SceneObject::GetUp() const
{
    return rotation_ * Vec3::PositiveY;
}

Vec3 SceneObject::GetWorldForward() const
{
    return GetWorldRotation() * Vec3::PositiveZ;
}

Vec3 SceneObject::GetWorldRight() const
{
    return GetWorldRotation() * Vec3::PositiveX;
}

Vec3 SceneObject::GetWorldUp() const
{
    return GetWorldRotation() * Vec3::PositiveY;
}

void SceneObject::SetWorldPosition(const Vec3& value)
{
    if (parent_)
        position_ = (parent_->inverseWorldTransform_ * value).xyz();
    else
        SetPosition(value);
    GetParent() ? GetParent()->UpdateTransformsRecurse() : UpdateTransformsRecurse();
}

void SceneObject::SetWorldRotation(const Quat& value)
{
    if (parent_)
        rotation_ = parent_->GetWorldRotation().Inverted() * value;
    else
        SetRotation(value);
    GetParent() ? GetParent()->UpdateTransformsRecurse() : UpdateTransformsRecurse();
}

void SceneObject::SetWorldScale(const Vec3& value)
{
    if (parent_)
        scale_ = (1.0f / parent_->GetWorldScale()) * value;
    else
        SetScale(value);
    GetParent() ? GetParent()->UpdateTransformsRecurse() : UpdateTransformsRecurse();
}

void SceneObject::SetWorldTransform(const Mat3x4& value)
{
    if (parent_)
        (parent_->GetWorldTransform().Inverted() * value).Decompose(position_, rotation_, scale_);
    else
        value.Decompose(position_, rotation_, scale_);
    GetParent() ? GetParent()->UpdateTransformsRecurse() : UpdateTransformsRecurse();
}

float SceneObject::CombineDensity(ShapeMode shapeMode, float currentDensity, float newDensity) const
{
    switch (shapeMode)
    {
    case SM_Additive:
        return SprueMin(currentDensity, newDensity);
    case SM_Intersection:
        return SprueMax(currentDensity, newDensity);
    case SM_Subtractive:
        return SprueMax(currentDensity, -newDensity);
    case SM_Displace:
        return currentDensity + newDensity;
    case SM_Blend:
        return powSmoothMin(currentDensity, newDensity);
    default:
        return SprueMax(currentDensity, newDensity);
    }
}

float SceneObject::CalculateDensity(const Vec3& position) const
{
    //Vec3 samplePos = inverseWorldTransform_ * position;
    float currentDensity = 1000000.0f;

    for (unsigned i = 0; i < children_.size(); ++i)
        if (!children_[i]->IsDisabled())
            currentDensity = RecurseDensity(children_[i], currentDensity, position);

    return currentDensity;
}

float SceneObject::RecurseDensity(SceneObject* current, float currentDensity, Vec3 pos) const
{
    // Warp our sampling coordinates
    std::vector<SceneObject*>& children = current->children_;
    for (unsigned i = 0; i < children.size(); ++i)
        if (!children[i]->IsDisabled())
            children[i]->Deform(pos);

    // Compute our density first 
    // ie. a child may subtract against us, don't want to add on top of a child subtraction
    // that would not be the expected behaviour
    currentDensity = current->AdjustDensity(currentDensity, pos);

    // Compute child density now
    for (unsigned i = 0; i < children.size(); ++i)
        if (!children[i]->IsDisabled())
            currentDensity = RecurseDensity(children[i], currentDensity, pos);

    return currentDensity;
}

void SceneObject::UpdateTransforms()
{
    localTransform_ = Mat3x4::FromTRS(position_, rotation_, scale_);
    if (parent_)
    {
        worldTransform_ = parent_->GetWorldTransform() * localTransform_;
        worldRotation_ = parent_->GetWorldRotation() * rotation_;
    }
    else
    {
        worldTransform_ = localTransform_;
        worldRotation_ = rotation_;
    }

    inverseWorldTransform_ = worldTransform_.Inverted();
    inverseLocalTransform_ = localTransform_.Inverted();
}

void SceneObject::UpdateTransformsRecurse()
{
    UpdateTransforms();
    for (SceneObject* child : children_)
        child->UpdateTransformsRecurse();
}

bool SceneObject::Deserialize(Deserializer* src, const SerializationContext& context)
{
    base::Deserialize(src, context);
    unsigned childCt = src->ReadUInt();
    while (childCt > 0)
    {
        //StringHash typeHash = src->ReadStringHash();
        SceneObject* child = Context::GetInstance()->Deserialize<SceneObject>(src, context);// Create<SceneObject>(typeHash);
        if (child)
        {
            child->parent_ = this;
            children_.push_back(child);
            //child->Deserialize(src);
        }
        else
        {
            //TODO handle error
            return false;
        }
        --childCt;
    }
    return true;
}

bool SceneObject::Serialize(Serializer* dest, const SerializationContext& context) const
{
    bool success = true;
    success &= base::Serialize(dest, context);
    success &= dest->WriteUInt((unsigned)children_.size());
    for (SceneObject* child : children_)
        success &= child->Serialize(dest, context);
    return success;
}

bool SceneObject::Deserialize(tinyxml2::XMLElement* element, const SerializationContext& context)
{
    bool success = base::Deserialize(element, context);
    tinyxml2::XMLElement* child = element->FirstChildElement();
    while (child)
    {
        // Found our child list
        if (strcmp(child->Name(), "children") == 0)
        {
            tinyxml2::XMLElement* childItem = child->FirstChildElement();
            while (childItem)
            {
                StringHash childTypeHash(childItem->Name());
                if (SceneObject* child = Context::GetInstance()->Create<SceneObject>(childTypeHash))
                {
                    success &= child->Deserialize(childItem, context);
                    children_.push_back(child);
                }
                childItem = childItem->NextSiblingElement();
            }                
            break;
        }
        child = child->NextSiblingElement();
    }
    return success;
}

bool SceneObject::Serialize(tinyxml2::XMLElement* element, const SerializationContext& context) const
{
    tinyxml2::XMLElement* myElement = element->GetDocument()->NewElement(GetTypeName());
    element->LinkEndChild(myElement);
    bool success = SerializeProperties(myElement, context);
    if (children_.size() > 0)
    {
        tinyxml2::XMLElement* childList = element->GetDocument()->NewElement("children");
        myElement->LinkEndChild(childList);
        for (auto child : children_)
        {
            success &= child->Serialize(childList, context);
        }
    }
    return success;
}

std::vector<SceneObject*> SceneObject::GetFlatList() const
{
    std::vector<SceneObject*> ret;
    GetFlatList(ret);
    return ret;
}

void SceneObject::GetFlatList(std::vector<SceneObject*>& container) const
{
    container.insert(container.end(), children_.begin(), children_.end());
    for (SceneObject* child : children_)
        child->GetFlatList(container);
}

void SceneObject::MoveChildBefore(SceneObject* child, SceneObject* before)
{
    child->SetParent(this);

    auto iter = std::find(children_.begin(), children_.end(), before);
    if (iter != children_.end())
        children_.insert(iter, child);
    else
        AddChild(child);
}

void SceneObject::MoveChildBefore(SceneObject* child, int index)
{
    child->SetParent(this);

    if (index < 0 || index >= children_.size() - 1)
        children_.push_back(child);
    else
    {
        child->SetParent(this);
        children_.insert(children_.begin() + index, child);
    }
}

void SceneObject::MoveChildAfter(SceneObject* child, SceneObject* after)
{
    child->SetParent(this);
    child->SetParent(this);

    auto iter = std::find(children_.begin(), children_.end(), after);
    if (iter != children_.end())
        ++iter;

    if (iter != children_.end())
        children_.insert(iter, child);
    else
        AddChild(child);
}

void SceneObject::MoveChildAfter(SceneObject* child, unsigned index)
{
    MoveChildBefore(child, ++index);
}

bool SceneObject::IsChildOfThis(SceneObject* possibleChild, bool recurse) const
{
    if (std::find(children_.begin(), children_.end(), possibleChild) != children_.end())
        return true;
    if (recurse)
    {
        for (auto child : children_)
            if (child->IsChildOfThis(possibleChild))
                return true;
    }
    return false;
}

/// Retrieve the index of the given child
int SceneObject::IndexOf(SceneObject* child) const
{
    for (unsigned i = 0; i < children_.size(); ++i)
        if (children_[i] == child)
            return i;
    return -1;
}

void SceneObject::SetParent(SceneObject* editable)
{
    if (parent_ && parent_ != editable)
        Unparent();
    parent_ = editable;
}

void SceneObject::Unparent()
{
    if (parent_)
        parent_->RemoveChild(this);
    parent_ = 0x0;
}

bool SceneObject::VisitParentFirst(SceneObjectVisitor* visitor)
{
    if (!visitor->ShouldNotVisitChildren(this))
    {
        if (visitor->ShouldVisit(this))
            if (!visitor->Visit(this))
                return false;
        for (SceneObject* child : children_)
            if (!child->VisitParentFirst(visitor))
                return false;
    }
    return true; //?? worth anything?
}

bool SceneObject::VisitChildFirst(SceneObjectVisitor* visitor)
{
    if (!visitor->ShouldNotVisitChildren(this))
    {
        for (SceneObject* child : children_)
            if (!child->VisitChildFirst(visitor))
                return false;
        if (visitor->ShouldVisit(this))
            if (!visitor->Visit(this))
                return false;
    }
    return true; //?? is this worth anything?
}

bool SceneObject::VisitUpward(SceneObjectVisitor* visitor)
{
    if (parent_)
    {
        if (visitor->ShouldVisit(parent_))
        {
            if (!visitor->Visit(this))
                return false;
        }
        if (!parent_->VisitUpward(visitor))
            return false;
    }
    return true;
}

BoundingBox SceneObject::ComputeRecursiveBounds() const
{
    BoundingBox bounds = ComputeBounds();// .Transformed(GetWorldTransform());
    for (auto child : children_)
        bounds.Enclose(child->ComputeRecursiveBounds().Transform(child->GetWorldTransform()));
    return bounds;
}

bool SceneObject::TestRayFast(const Ray& ray, IntersectionInfo* info) const
{
    return ray.Intersects(GetWorldBounds());
    //float junk;
    //if (info)
    //    return ray.Intersects(GetWorldBounds(), info->t, junk);
    //else
    //    return ray.Intersects(GetWorldBounds());
}

void SceneObject::GetGizmos(std::vector<std::shared_ptr<Gizmo> >& gizmos)
{
    gizmos.push_back(std::shared_ptr<Gizmo>(new SceneObjectGizmo(this)));
}

unsigned SceneObject::StructuralHash(unsigned hash) const
{
    hash = hash * 31 + CalculateStructuralHash();
    for (auto child : children_)
        hash = hash * 31 + child->StructuralHash();
    return hash;
}

bool SceneObject::MakeSymmetric()
{
    if (IsSymmetric() && parent_)
    {
        if (auto child = (SceneObject*)Duplicate())
        {
            parent_->AddChild(child);
            auto pos = child->GetWorldPosition();
            pos.x *= -1.0f;
            child->SetPosition(pos);
            
            auto rot = child->GetWorldRotation();
            float4 axis;
            float angle = 0.0f;
            rot.ToAxisAngle(axis, angle);
            axis.x *= -1;
            axis.Normalize();
            angle = -angle;
            rot.SetFromAxisAngle(axis, angle);
            child->SetRotation(rot);
            //
            //auto scl = child->GetScale();
            //scl.x *= -1.0f;
            //child->SetScale(scl);

            auto name = child->GetName();
            if (name.find("Left") != std::string::npos)
                name = ReplaceString(name, "Left", "Right");
            else if (name.find("Right") != std::string::npos)
                name = ReplaceString(name, "Right", "Left");
            
            if (name.find("left") != std::string::npos)
                name = ReplaceString(name, "left", "right");
            else if (name.find("right") != std::string::npos)
                name = ReplaceString(name, "right", "left");
            child->SetName(name);

            return true;
        }
    }

    auto originalChildren = children_;
    for (auto child : originalChildren)
        child->MakeSymmetric();
    return false;
}

class TriangleCounterVisitor : public SceneObjectVisitor
{
public:
    virtual bool ShouldVisit(SceneObject* child) {
        return true;
    }
    
    virtual bool Visit(SceneObject* child) override {
        if (SprueModel* mdl = dynamic_cast<SprueModel*>(child))
            ct += mdl->GetMeshedParts()->GetTriangleCount();
        return true;
    }
    unsigned ct = 0;
};

unsigned SceneObject::GetTriangleCount()
{
    TriangleCounterVisitor visitor;
    VisitParentFirst(&visitor);
    return visitor.ct;
}

}