#pragma once

#include <SprueEngine/Graph/GraphNode.h>

namespace SprueEngine
{
#define MATGRAPH_ANY_NUMERIC 1
#define MATGRAPH_FLOAT 2
#define MATGRAPH_INT 3
#define MATGRAPH_VEC2 4
#define MATGRAPH_VEC3 5
#define MATGRAPH_VEC4 6
#define MATGRAPH_INTVEC2 7
#define MATGRAPH_INTVEC3 8
#define MATGRAPH_INTVEC4 9
#define MATGRAPH_BYTE 10
#define MATGRAPH_MAT3 11
#define MATGRAPH_MAT4 12

    class MatGraphNode;

    enum MatGraphFlags
    {
        MGF_None = 0,
        MGF_Required = 1
    };

    /// Specification received from the compiler on what and how to produce code fragments.
    /// Favor per-target tables as much as possible (ie. cos vs. cosf)
    enum MatGraphTargetLanguage
    {
        MGTL_GLSL = 0,      // Obvious
        MGTL_HLSL = 1,      // Obvious
        MGTL_CG = 2,        // Target CG's specifics, likely reducible to a post-process of HLSL
        MGTL_OpenCL = 3,    // Intended for image processing and bulk simulation purposes
        MGTL_CPP = 4,       // Write C++ code for CPU, intended for image processing and simulation purposes, prefer CodeGraph's for general purpose code as they have more logical features.
    };

    /// Responsible for retrieving constitent unique names for nodes and their output socket values.
    struct MatGraphNameSource {
        virtual std::string NodeName(const MatGraphNode*) = 0;
        virtual std::string SocketName(const MatGraphNode*) = 0;
    };

    /// Graph nodes have interesting issues in generation.
    /// node *Execute* is really just *simulation*, the natural of that simulation depends on the target
    class MatGraphNode : public GraphNode
    {
    public:
    // Both execution and compilation related functions
        /// Is this node configured in a valid fashion? All required inputs are set?
        virtual bool NodeIsValid() const;

    // Compilation related functions
        /// Only used if the node needs to write code or socket code, use to output forward declarations, #includes, and the like before the main fragment body
        virtual std::string WriteHeader(MatGraphTargetLanguage) const { return std::string(); }
        /// Return true if this node needs to write freestanding code
        virtual bool NeedsToWriteCode(MatGraphTargetLanguage) const = 0;
        /// Individual sockets may have specific output needs, most nodes however should only have a single output.
        virtual bool NeedsToWriteSocketUsageCode(MatGraphTargetLanguage, const GraphSocket* forSocket) const = 0;
        /// Write the main-body code.
        virtual std::string WriteCode(MatGraphTargetLanguage) const = 0;
        /// Write the code for socket usage.
        virtual std::string WriteCode(MatGraphTargetLanguage, const GraphSocket* forSocket) const = 0;
    };

    /// Constant nodes do not change, allows for an opportunity in generation.
    class MatGraphConstantNode : public MatGraphNode
    {
    public:
    };

    /// Function nodes deal with _FUNC_(_ARG1_, _ARG2_) issues.
    class MatGraphFunctionNode : public MatGraphNode
    {
    public:
        virtual bool NeedsToWriteCode() const { return true; }
        virtual std::string GetFunctionName(MatGraphTargetLanguage lang) const = 0;
    };

    /// Math nodes form LHS _OP_ RHS relationships in produced code.
    class MatGraphMathNode : public MatGraphNode
    {
    public:
    };
}