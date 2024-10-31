#include "MatGraphFunctionNodes.h"

namespace SprueEngine
{

    std::string MatGraphCosNode::WriteCode(MatGraphTargetLanguage lang) const
    {
        switch (lang)
        {
        case MGTL_GLSL:
        case MGTL_HLSL:
        case MGTL_OpenCL:
            return "cos";
            break;
        case MGTL_CPP:
            return "cosf";
            break;
        }
        return std::string();
    }

    std::string MatGraphCosNode::GetFunctionName(MatGraphTargetLanguage lang) const
    {
        switch (lang)
        {
        case MGTL_GLSL:
        case MGTL_HLSL:
        case MGTL_OpenCL:
            return "cos";
        case MGTL_CPP:
            return "cosf";
        }
        return std::string();
    }
}