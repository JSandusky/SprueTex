#pragma once

#include <SprueEngine/MathGeoLib/AllMath.h>
#include <SprueEngine/ClassDef.h>
#include <SprueEngine/Libs/Jzon.h>

namespace SprueEngine
{
    struct SPRUE SpaceRule
    {

    };

    struct SPRUE SpaceSubdivision
    {
        std::vector<float> XAxis_;
        std::vector<float> YAxis_;
        std::vector<float> ZAxis_;
        bool slice9_ = false;
        bool slice27_ = false;
        bool shrink_ = false;
    };

    struct SPRUE SpaceGrammarNode
    {
        SpaceGrammarNode* parent_;
        std::vector<BoundingBox> occlusionAreas_;
        std::vector<SpaceRule*> rules_;
        std::vector<SpaceRule*> occludedRules_;

        bool loftX_ = false;
        bool loftY_ = false;
        bool loftZ_ = false;
    };

    class SPRUE ShapeGrammar
    {
        NOCOPYDEF(ShapeGrammar);
    public:
        ShapeGrammar();
        ShapeGrammar(Jzon::Node node);
        virtual ~ShapeGrammar();

    private:

    };

}