#pragma once

#include <SprueEngine/Graph/MatGraph/MatGraphBaseNodes.h>

namespace SprueEngine
{

#define SIMPLE_FUNCTION_DEF(CLAZZ, FUNC) class SPRUE CLAZZ : public MathGraphFunctionNode { \

    /// Functions that potentially take vector values have different needs
    enum MatGraphMathFunctionVectorType
    {
        MGMFVT_Float,
        MGMFVT_Vec2,
        MGMFVT_Vec3,
        MGMFVT_Vec4,
        MGMFVT_Integer,
        MGMFVT_IntVec2,
        MGMFVT_IntVec3,
        MGMFVT_IntVec4
    };

    /// Derivatives each specializing for different types of Vectors (from not a vector to a 4 component vector)
    class SPRUE MatGraphVaryingVectorFunctionNode : public MatGraphFunctionNode
    {
    public:
    };

    class SPRUE MatGraphCosNode : public MatGraphFunctionNode
    {
    public:
        virtual int Execute(const Variant& param) override;

        virtual std::string WriteCode(MatGraphTargetLanguage) const override;

        virtual std::string GetFunctionName(MatGraphTargetLanguage lang) const override;
    };

    class SPRUE MatGraphSinNode : public MatGraphFunctionNode
    {
    public:
        virtual int Execute(const Variant& param) override;
    };

    class SPRUE MatGraphTanNode : public MatGraphFunctionNode
    {
    public:
    };

    class SPRUE MatGraphACosNode : public MatGraphFunctionNode
    {
    public:
    };

    class SPRUE MatGraphASinNode : public MatGraphFunctionNode
    {
    public:
    };

    class SPRUE MatGraphATanNode : public MatGraphFunctionNode
    {
    public:
    };

    class SPRUE MatGraphATan2Node : public MatGraphFunctionNode
    {
    public:
    };
}