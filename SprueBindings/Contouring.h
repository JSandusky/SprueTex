#pragma once

using namespace System;
using namespace System::IO;

namespace SprueBindings
{

    public interface class ContouringMethod
    {
    public:
        bool Contour(System::IO::MemoryStream^ dataStream);
    };

    public ref class NaiveSurfaceNets : ContouringMethod
    {
    public:
        bool Contour(System::IO::MemoryStream^ stream) override;
    };

    public ref class DualContouring : ContouringMethod
    {
    public:
        bool Contour(System::IO::MemoryStream^ stream) override;
    };

}