#pragma once

#include <SprueEngine/ClassDef.h>
#include <SprueEngine/IEditable.h>
#include <SprueEngine/IHaveGizmos.h>
#include <SprueEngine/IMeshable.h>
#include <SprueEngine/Math/MathDef.h>
#include <SprueEngine/Math/IntersectionInfo.h>
#include <SprueEngine/MathGeoLib/AllMath.h>

#include <functional>
#include <vector>
#include <string>

namespace SprueEngine
{
    enum ShapeMode;
    class Context;

    class IDebugRender;
    class MeshData;
    class SceneObjectVisitor;

    /// Set when drawing for mouse hovering
    #define SPRUE_DEBUG_HOVER 1
    /// Set when drawing for passive display in the scene (extremely low lapha)
    #define SPRUE_DEBUG_PASSIVE 2

    /*
    Implementation guidelines:
        Add handling to SprueBind FactoryMethods.cpp
    */
    class SPRUE SceneObject : public IEditable, public IMeshable, public IHaveGizmos
    {
        BASECLASSDEF(SceneObject, IEditable);
    protected:
        SceneObject() : 
            parent_(0x0),
            scale_(1.0f,1.0f,1.0f),
            disabled_(false),
            symmetric_(false),
            locked_(false)
        {
        }
        SceneObject(SceneObject* parent) : 
            parent_(parent)
        {
        }

    public:
        virtual ~SceneObject();

        static void Register(Context*);

        std::vector<SceneObject*>& GetChildren() { return children_; }
        const std::vector<SceneObject*>& GetChildren() const { return children_; }

        virtual bool CanMove() const { return true; }
        virtual bool CanRotate() const { return true; }
        virtual bool CanScale() const { return true; }

        unsigned IndexOfChild(SceneObject* child) const;
        bool HasChild(SceneObject* child, bool recurse = false) const;

        template<class T>
        T* FindParentOfType() const
        {
            SceneObject* parent = const_cast<SceneObject*>(this)->GetParent();
            if (parent)
            {
                do {
                    if (T* ret = dynamic_cast<T*>(parent))
                        return ret;
                } while (parent = parent->GetParent());
            }
            return 0x0;
        }

        template<class T>
        T* FindChild(const char* name, bool recurse = true) const
        {
            for (SceneObject* child : children_)
            {
                if (child->GetName().compare(name) == 0)
                    return dynamic_cast<T*>(child);
                if (recurse)
                {
                    SceneObject* found = FindChild<T>(name, recurse);
                    if (found != 0x0)
                        return dynamic_cast<T*>(found);
                }
            }
            return 0x0;
        }

        virtual bool AcceptsChildren() const { return true; }

        virtual void AddChild(SceneObject* child);
        virtual void AddChildAt(SceneObject*child, unsigned index);
        virtual void RemoveChild(SceneObject* child, bool recurse = false);
        virtual void RemoveChild(const char* name, bool recurse = false);
        
        template<class T>
        std::vector<T*> GetChildrenOfType(bool recurse = true) const
        {
            std::vector<T*> ret;
            GetChildrenOfType<T>(ret, recurse);
            return ret;
        }

        /// Returns the first child of the template type found in this object.
        template<class T>
        T* GetFirstChildOfType() const
        {
            for (auto child : children_)
                if (T* obj = dynamic_cast<T*>(child))
                    return obj;
            return 0x0;
        }

        template<class T>
        void GetChildrenOfType(std::vector<T*>& container, bool recurse = true) const
        {
            for (SceneObject* child : children_)
            {
                T* casted = dynamic_cast<T*>(child);
                if (casted)
                    container.push_back(casted);

                if (recurse)
                    child->GetChildrenOfType<T>(container, recurse);
            }
        }

        std::vector<SceneObject*> GetFlatList() const;

        void GetFlatList(std::vector<SceneObject*>& container) const;

        virtual void MoveChildBefore(SceneObject* child, SceneObject* before);

        virtual void MoveChildBefore(SceneObject* child, int index);

        virtual void MoveChildAfter(SceneObject* child, SceneObject* after);

        virtual void MoveChildAfter(SceneObject* child, unsigned index);

        virtual bool IsChildOfThis(SceneObject* possibleChild, bool recurse = false) const;

        /// Retrieve the index of the given child
        virtual int IndexOf(SceneObject* child) const;
        
        SceneObject* GetParent() { return parent_; }
        
        const SceneObject* GetParent() const { return parent_; }
        
        void SetParent(SceneObject* editable);

        void Unparent();

        /// Override to transform the sampling coordinates
        virtual void Deform(Vec3& pos) const { }

        /// Override to implement density calculation
        virtual float AdjustDensity(float currentDensity, const Vec3& pos) const { return currentDensity; }

        /// Shared method for merging together density values based on a shape blending mode
        float CombineDensity(ShapeMode shapeMode, float currentDensity, float newDensity) const;

        virtual float CalculateDensity(const Vec3& position) const override;

        bool IsDisabled() const { return disabled_; }
        void SetDisabled(bool state) { disabled_ = state; }

        bool IsSymmetric() const { return symmetric_; }
        void SetSymmetric(bool state) { symmetric_ = state; }

        bool IsLocked() const { return locked_; }
        void SetLocked(bool state) { locked_ = state; }

        std::string GetName() const { return name_; }
        void SetName(const std::string& name) { name_ = name; }

        // Local Transform functions
        Vec3 GetPosition() const { return position_; }
        Quat GetRotation() const { return rotation_; }
        Vec3 GetScale() const { return scale_; }
        Mat3x4 GetLocalTransform() const { return localTransform_; }
        void SetPosition(const Vec3& value) { position_ = value; GetParent() ? GetParent()->UpdateTransformsRecurse() : UpdateTransformsRecurse(); }
        void SetRotation(const Quat& value) { rotation_ = value; GetParent() ? GetParent()->UpdateTransformsRecurse() : UpdateTransformsRecurse(); }
        void SetScale(const Vec3& value) { scale_ = value; GetParent() ? GetParent()->UpdateTransformsRecurse() : UpdateTransformsRecurse(); }

        // World Transform Functions
        Vec3 GetWorldPosition() const { return GetWorldTransform().TranslatePart(); }
        Quat GetWorldRotation() const { return parent_ ? parent_->rotation_ * rotation_ : rotation_; }
        Vec3 GetWorldScale() const { return scale_; }
        Mat3x4 GetWorldTransform() const { return worldTransform_; }
        
        void SetWorldPosition(const Vec3& value);
        void SetWorldRotation(const Quat& value);
        void SetWorldScale(const Vec3& value);
        void SetWorldTransform(const Mat3x4& value);

        // Local and global direction functions
        Vec3 GetForward() const;
        Vec3 GetRight() const;
        Vec3 GetUp() const;
        Vec3 GetWorldForward() const;
        Vec3 GetWorldRight() const;
        Vec3 GetWorldUp() const;

        virtual bool Deserialize(Deserializer* src, const SerializationContext& context) override;
        virtual bool Serialize(Serializer* dest, const SerializationContext& context) const override;
#ifndef SPRUE_NO_XML
        virtual bool Deserialize(tinyxml2::XMLElement*, const SerializationContext& context) override;
        virtual bool Serialize(tinyxml2::XMLElement*, const SerializationContext& context) const override;
#endif

        /// Checks the object first, then the children. Returns true if it visited everything (was not terminated by the visitor).
        bool VisitParentFirst(SceneObjectVisitor* visitor);
        /// Checks the children first, then the object itself. Returns true if it visited everything (was not terminated by the visitor).
        bool VisitChildFirst(SceneObjectVisitor* visitor);
        /// Visits parents up along the tree. Returns false if it visited everything (was not terminated by the visitor).
        bool VisitUpward(SceneObjectVisitor* visitor);

        void Visit(std::function<void(SceneObject*)> function) {
            function(const_cast<SceneObject*>(this));
            for (int i = 0; i < children_.size(); ++i)
                children_[i]->Visit(function);
        }

        virtual bool ContainsPoint(const Vec3& pt) const {
            if (bounds_.minPoint.Equals(bounds_.maxPoint))
                ComputeBounds();
            return bounds_.Contains((GetWorldTransform().Inverted() * pt).xyz());
        }
        
        // Calculate the bounding box for this specific object
        virtual BoundingBox ComputeBounds() const = 0;

        BoundingBox GetLocalBounds() { return bounds_; }
        OrientedBoundingBox GetWorldBounds() const { return bounds_.Transform(GetWorldTransform()); }

        /// Check a raycast against our bounds
        virtual bool TestRayFast(const Ray& ray, IntersectionInfo* info) const;
        /// Perform a reasonable accurate ray test (default is to just use the TestRayFast against the bounds)
        virtual bool TestRayAccurate(const Ray& ray, IntersectionInfo* info) const { return TestRayFast(ray, info); }

        // Calculate the bounding box for this including all child bounding boxes, returns the combined world transformed bounding box
        virtual BoundingBox ComputeRecursiveBounds() const;

        virtual void AttributeUpdated(const StringHash& attr) override { ComputeBounds(); }

        virtual std::vector<std::shared_ptr<Gizmo> > GetGizmos() override {
            std::vector<std::shared_ptr<Gizmo> > ret;
            GetGizmos(ret);
            return ret;
        }

        virtual std::shared_ptr<MeshData> GeneratePreviewMesh() const { return 0x0; }

        /// Render line primitives into a debug renderer, flags are used to direct additional options (draw deformed, etc)
        virtual void DrawDebug(IDebugRender* renderer, unsigned flags = 0) const { }

        unsigned StructuralHash(unsigned currentHash = 17) const;

        unsigned GetCapabilityBits() const { return capabilities_; }
        void SetCapabilityBits(unsigned bits) { capabilities_ = bits; }
        unsigned GetUserBits() const { return userFlags_; }
        void SetUserBits(unsigned bits) { userFlags_ = bits; }

    // Symmetry applications
        virtual bool MakeSymmetric();

    // Hierarchy restrictions
        /// Returns true if this object can be moved around in the scene hierarchy.
        virtual bool CanChangeHierarchy() const { return true; }
        /// Returns true if this object will accept the given child.
        virtual bool AcceptAsChild(const SceneObject* child) const { return true; }
        /// Returns true if this object will accept having the given object as a parent.
        virtual bool AcceptAsParent(const SceneObject* possible) const { return true; }
        /// Returns true if this object is allowed to be posited in front of the given child. Restricts tree reordering.
        virtual bool CanPositionBefore(const SceneObject* child) const { return true; }

        unsigned GetTriangleCount();

    protected:
        struct SceneObjectGizmo;

        float RecurseDensity(SceneObject* current, float currentDensity, Vec3 pos) const;
        virtual unsigned CalculateStructuralHash() const { return 0; } 

        void UpdateTransforms();
        void UpdateTransformsRecurse();

        virtual void GetGizmos(std::vector<std::shared_ptr<Gizmo> >& gizmos);

        std::vector<SceneObject*> children_;
        SceneObject* parent_;
        std::string name_;
        Vec3 position_;
        Quat rotation_;
        Vec3 scale_;
        unsigned capabilities_ = 0;
        unsigned userFlags_ = 0;
        Mat3x4 worldTransform_; // Cached when position is changed
        Mat3x4 inverseWorldTransform_; // Cached when position is changed (inverting was 10% of density time)
        Mat3x4 localTransform_; // Cached when position is changed
        Mat3x4 inverseLocalTransform_; // Cached when position is changed (inverting was 10% of density time)
        Quat worldRotation_;    // Cached when position is changed
        mutable BoundingBox bounds_;
        bool disabled_ = false;
        bool locked_ = false;
        bool symmetric_ = true;
    };

    /// Visitor pattern object used during tree traversals.
    class SPRUE SceneObjectVisitor
    {
    public:
        /// Return true if the child in question should be visited.
        virtual bool ShouldVisit(SceneObject* child) = 0;
        /// Returns FALSE if the visitor should halt visiting the tree.
        virtual bool Visit(SceneObject* child) = 0;

        virtual bool ShouldNotVisitChildren(SceneObject* object) { return false; }
    };

    /// Templated type specific base implementation of the above SceneObjectVisitor.
    template<class T>
    class SPRUE TypeLimitedSceneObjectVisitor : public SceneObjectVisitor
    {
    public:
        virtual bool ShouldVisit(SceneObject* child) override {
            return dynamic_cast<T*>(child) != 0x0;
        }
        virtual bool Visit(SceneObject* child) override {
            return Visit(dynamic_cast<T*>(child));
        }
        virtual bool Visit(T* child) = 0;
    };
}