#pragma once

#include <SprueEngine/Graph/MatGraph/MatGraphBaseNodes.h>

namespace SprueEngine
{

    /* MatGraph Compiler

    Socket value grabbing:

        Function nodes:
            If a single output (all but template nodes) has more than one connection then a variable will be created for it

        Math nodes:
            Follow the same rules as function nodes, the distinction is for class writing ease. A macro language could coalesce
            the differences but would likely not be worth the increase in shader compiler time, and it makes more sense for the
            MatGraph -> shader process to do as much work as it can.

        Template nodes:
            All output sockets with any connections are output as variables
            Template nodes will substitue the values from upstream connections where there are connections
                note: all templates require 'default' values for their variable input parameters
        
        Constant nodes are always inserted as is when used:
            ie. vec2(0.5, 0.1)
            They do not turn into variables regardless of how many connections exist

        System nodes:
            Complete no man's land. All operations have to be resolved by the node. In a legacy forward rendering setup
            this would be the 1-4 lights varying blocks of shader code. In PBR sampling this would be a node to turn arbitrary
            Rough-Metal -> Gloss-Spec, etc.
    */

    class MatGraphCompiler
    {
    public:
        MatGraphCompiler(const MatGraphNode* forNode, MatGraphTargetLanguage lang);

        void Compile();

    private:
        const MatGraphNode* node_;
        MatGraphTargetLanguage language_;
    };
}