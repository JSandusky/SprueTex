#pragma once

#include <SprueEngine/Compute/ComputeShader.h>

#include <d3d11.h>

namespace SprueEngine
{

    /// ComputeShader for DX11, the name is assumed to be the entry-point.
    class SPRUE DX11ComputeShader : public ComputeShader
    {
        NOCOPYDEF(DX11ComputeShader);
    public:
        /// Construct.
        DX11ComputeShader(const std::string& name, ComputeDevice* device);
        /// Destruct.
        virtual ~DX11ComputeShader();

        virtual bool CompileShader(const std::vector<std::string>& sources, const std::string& defines = std::string()) override;
        virtual bool CompileShader(const std::string& source, const std::string& defines = std::string()) override;
        virtual ComputeKernel* GetKernel(const std::string& name) override;
        virtual bool IsCompiled() const override { return isCompiled_; }

    protected:
        /// Bytecode for the compiled shader.
        ID3DBlob* shaderByteCode_ = 0x0;
        /// Compiled D3D11 compute shader for this shader abstraction.
        ID3D11ComputeShader* shader_ = 0x0;
    };

}