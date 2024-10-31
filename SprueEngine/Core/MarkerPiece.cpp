#include "SprueEngine/Core/SpruePieces.h"

#include "SprueEngine/Core/Context.h"
#include "SprueEngine/IDebugRender.h"

namespace SprueEngine
{

MarkerPiece::MarkerPiece() : base()
{

}

MarkerPiece::~MarkerPiece()
{

}

void MarkerPiece::Register(Context* context)
{
    context->RegisterFactory<MarkerPiece>("MarkerPiece", "Helper piece that can be used for animation relative to, procedural placement, attaching things to, etc");
    context->CopyBaseProperties(StringHash("SpruePiece"), StringHash("MarkerPiece"));
}

void MarkerPiece::DrawDebug(IDebugRender* renderer, unsigned flags) const
{
    renderer->DrawCross(GetWorldPosition(), 1.0f, RGBA::Yellow);
}

}