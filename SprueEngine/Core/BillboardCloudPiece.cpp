#include "BillboardCloudPiece.h"

#include "Context.h"

namespace SprueEngine
{

    BillboardCloudPiece::BillboardCloudPiece()
    {

    }

    BillboardCloudPiece::~BillboardCloudPiece()
    {

    }

    void BillboardCloudPiece::Register(Context* context)
    {
        context->RegisterFactory<BillboardCloudPiece>("BillboardCloudPiece", "Creates a cloud of camera facing billboards (similar to tree leaf billboards)");
        context->CopyBaseProperties("SceneObject", "BillboardCloudPiece");
        REGISTER_PROPERTY(BillboardCloudPiece, unsigned, GetBillboardCount, SetBillboardCount, 10, "Billboard Count", "Number of billboards in the cloud", PS_VisualConsequence);
    }

}