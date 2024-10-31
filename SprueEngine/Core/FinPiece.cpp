#include "FinPiece.h"

#include "Context.h"
#include <SprueEngine/IDebugRender.h>
#include <SprueEngine/Geometry/MeshData.h>

namespace SprueEngine
{

    struct FinPiece::FinGizmo : public Gizmo
    {
        unsigned index_;
        Node* vert_;

        FinGizmo(FinPiece* spine, unsigned index) :
            Gizmo(spine, GIZ_Translate | GIZ_Rotate | GIZ_Scale | GIZ_InitialControlPoint),
            index_(index)
        {
            gizmoId_ = index + 1;
            vert_ = spine->nodes_[index];
            transform_ = Mat3x4::FromTRS(spine->GetWorldTransform() * vert_->position_, vert_->rotation_, Vec3(1,1,1));
        }

        virtual bool UpdateValue() override
        {
            FinPiece* spine = (FinPiece*)source_;
            Mat3x4 inverse = spine->GetWorldTransform().Inverted();

            vert_->position_ = inverse * transform_.TranslatePart();
            vert_->rotation_ = transform_.RotatePart().ToQuat().Normalized();
            vert_->width_ = vert_->width_ * transform_.GetScale().MaxElement();

            return true;
        }

        virtual void RefreshValue() override
        {
            FinPiece* spine = (FinPiece*)source_;
            transform_ = Mat3x4::FromTRS(spine->GetWorldTransform() * vert_->position_, vert_->rotation_, Vec3(1,1,1));
        }

        virtual void GetCommands(std::vector<unsigned>& commandIDs) override
        {
            commandIDs.push_back(SPRUE_GIZ_CMD_INSERT_AFTER);
            commandIDs.push_back(SPRUE_GIZ_CMD_INSERT_BEFORE);
            commandIDs.push_back(SPRUE_GIZ_CMD_REMOVE);
        }

        virtual bool ExecuteCommand(unsigned commandID, const Variant& commandParam) override
        {
            if (commandID == SPRUE_GIZ_CMD_INSERT_AFTER)
            {
                if (FinPiece* spine = (FinPiece*)source_)
                {
                    Node* v = new Node();
                    Node* current = *(spine->nodes_.begin() + index_);
                    Node* next = 0x0;
                    if (index_ < spine->nodes_.size() - 1)
                        next = *(spine->nodes_.begin() + index_ + 1);

                    if (next)
                    {
                        v->position_ = Lerp(current->position_, next->position_, 0.5f);
                        v->rotation_ = current->rotation_.Slerp(next->rotation_, 0.5f);
                        v->width_ = Lerp(current->width_, next->width_, 0.5f);
                    }
                    else
                    {
                        next = *(spine->nodes_.begin() + index_ - 1);
                        if (next)
                        {
                            v->position_ = current->position_ + (current->position_ - next->position_);
                            v->rotation_ = current->rotation_;
                            v->width_ = current->width_;
                        }
                        else
                        {
                            v->position_ = current->position_ + Vec3(1, 1, 1);
                            v->rotation_ = current->rotation_;
                            v->width_ = current->width_;
                        }
                    }

                    if (index_ == spine->nodes_.size() - 1)
                        spine->nodes_.push_back(v);
                    else
                        spine->nodes_.insert(spine->nodes_.begin() + index_ + 1, v);

                    return true;
                }
            }
            else if (commandID == SPRUE_GIZ_CMD_INSERT_BEFORE)
            {
                if (FinPiece* spine = (FinPiece*)source_)
                {
                    Node* v = new Node();
                    Node* current = *(spine->nodes_.begin() + index_);
                    Node* previous = 0x0;
                    if (index_ > 0)
                        previous = *(spine->nodes_.begin() + index_ - 1);

                    if (previous)
                    {
                        v->position_ = Lerp(previous->position_, current->position_, 0.5f);
                        v->rotation_ = previous->rotation_.Slerp(current->rotation_, 0.5f);
                        v->width_ = Lerp(previous->width_, current->width_, 0.5f);
                    }
                    else
                    {
                        delete v;
                        return false;
                    }

                    spine->nodes_.insert(spine->nodes_.begin() + index_, v);

                    return true;
                }
            }
            else if (commandID == SPRUE_GIZ_CMD_REMOVE)
            {
                if (FinPiece* spine = (FinPiece*)source_)
                {
                    if (spine->nodes_.size() <= 2)
                        return false;
                    Node* erased = *(spine->nodes_.begin() + index_);
                    spine->nodes_.erase(spine->nodes_.begin() + index_);
                    delete erased;
                    return true;
                }
            }
            return false;
        }

        virtual bool Equals(Gizmo* rhs) const override
        {
            if (auto other = dynamic_cast<FinGizmo*>(rhs))
                return other->source_ == source_ && other->vert_ == vert_;
            return false;
        }
    };

    FinPiece::FinPiece()
    {
        Node* v = new Node();
        v->position_ = Vec3(0, 0, 0);
        nodes_.push_back(v);

        v = new Node();
        v->position_ = Vec3(0, 3, 5);
        nodes_.push_back(v);

        v = new Node();
        v->position_ = Vec3(0, 7, 15);
        nodes_.push_back(v);
    }

    FinPiece::~FinPiece()
    {

    }

    void FinPiece::Register(Context* context)
    {
        context->RegisterFactory<FinPiece>("FinPiece", "Quad strip that can be used for different effects such as fins and hair-strips");
        context->CopyBaseProperties("SceneObject", "FinPiece");
        REGISTER_PROPERTY(FinPiece, bool, IsClippedToMesh, SetClippedToMesh, false, "Clip To Mesh", "Fin strip will be clipped so that it does not intersect the surface of any other mesh", PS_VisualConsequence);
        REGISTER_PROPERTY(FinPiece, bool, IsTwoSided, SetTwoSided, true, "Two Sided", "Fin strip will render without backface culling", PS_VisualConsequence);
        REGISTER_PROPERTY_CONST_SET(FinPiece, VariantVector, GetPoints, SetPoints, VariantVector(), "Points", "Secret list of points", PS_Secret);
    }

    MeshData* FinPiece::CreateMesh() const
    {
        auto trans = GetWorldTransform();

        MeshData* ret = new MeshData();

        for (unsigned i = 0; i < nodes_.size(); ++i)
        {
            const auto cur = nodes_[i];

            const Vec3 startPos = cur->position_;
            const Vec3 startUp = cur->rotation_ * Vec3::PositiveY;
            const Vec3 startRight = cur->rotation_ * Vec3::PositiveX;

            ret->positionBuffer_.push_back(trans * (startPos + -startRight * cur->width_));
            ret->positionBuffer_.push_back(trans * (startPos + startRight * cur->width_));

            ret->normalBuffer_.push_back((trans.RotatePart() * startUp).Normalized());
            ret->normalBuffer_.push_back((trans.RotatePart() * startUp).Normalized());
        }

        unsigned vertexIndex = 0;
        unsigned stripsLen = nodes_.size() - 1;

        ret->indexBuffer_.resize((nodes_.size() - 1) * 6);
        unsigned* dest = ret->indexBuffer_.data();

        while (stripsLen--)
        {
            dest[0] = vertexIndex;
            dest[1] = (vertexIndex + 2);
            dest[2] = (vertexIndex + 1);

            dest[3] = (vertexIndex + 1);
            dest[4] = (vertexIndex + 2);
            dest[5] = (vertexIndex + 3);

            dest += 6;
            vertexIndex += 2;
        }

        return ret;
    }

    void FinPiece::DrawDebug(IDebugRender* debug, unsigned flags) const
    {
        RGBA color = flags & SPRUE_DEBUG_HOVER ? RGBA::Gold : RGBA::Green;
        color.a = 0.75f;
        if (flags & SPRUE_DEBUG_PASSIVE)
            color.a = 0.1f;

        auto trans = GetWorldTransform();
        for (unsigned i = 0; i < nodes_.size() - 1; ++i)
        {
            const auto cur = nodes_[i];
            const auto next = nodes_[i + 1];

            const Vec3 startPos = cur->position_;
            const Vec3 startUp = cur->rotation_ * Vec3::PositiveY;
            const Vec3 startRight = cur->rotation_ * Vec3::PositiveX;
            const Vec3 nextPos = next->position_;
            const Vec3 nextUp = next->rotation_ * Vec3::PositiveY;
            const Vec3 nextRight = next->rotation_ * Vec3::PositiveX;

            debug->DrawLine(trans * (startPos + -startRight * cur->width_), trans * (startPos + startRight * cur->width_), color);
            debug->DrawLine(trans * (nextPos + -nextRight * next->width_), trans * (nextPos + nextRight * next->width_), color);

            debug->DrawLine(trans * (startPos + -startRight * cur->width_), trans * (nextPos + -nextRight * next->width_), color);
            debug->DrawLine(trans * (startPos + startRight * cur->width_),  trans * (nextPos + nextRight * next->width_), color);
        }
    }

    void FinPiece::GetGizmos(std::vector<std::shared_ptr<Gizmo> >& gizmos)
    {
        base::GetGizmos(gizmos);
        for (unsigned i = 0; i < nodes_.size(); ++i)
            gizmos.push_back(std::shared_ptr<Gizmo>(new FinGizmo(this, i)));
    }

    VariantVector FinPiece::GetPoints() const
    {
        VariantVector ret;
        for (auto vert : nodes_)
        {
            ret.push_back(vert->position_);
            ret.push_back(vert->rotation_);
            ret.push_back(vert->width_);
        }
        return ret;
    }

    void FinPiece::SetPoints(const VariantVector& pts)
    {
        unsigned ptCt = pts.size() / 3;
        if (ptCt != nodes_.size())
            nodes_.resize(ptCt);
        for (unsigned i = 0; i < nodes_.size(); ++i)
            if (!nodes_[i])
                nodes_[i] = new Node();

        for (unsigned i = 0, vert = 0; i < pts.size() && vert < nodes_.size(); i += 3, vert++)
        {
            nodes_[vert]->position_ = pts[i].getVec3();
            nodes_[vert]->rotation_ = pts[i + 1].getQuat();
            nodes_[vert]->width_ = pts[i + 2].getFloat();
        }
    }
}