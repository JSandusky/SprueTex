#include "DX11ComputeShader.h"

#include "DX11ComputeDevice.h"
#include "DX11ComputeKernel.h"

#include <SprueEngine/GeneralUtility.h>

#include <d3d.h>
#include <d3d11.h>
#include <d3dcompiler.h>

#include <sstream>

namespace SprueEngine
{

    DX11ComputeShader::DX11ComputeShader(const std::string& name, ComputeDevice* device) :
        ComputeShader(name, device)
    {
        
    }

    DX11ComputeShader::~DX11ComputeShader() 
    { 
        if (shaderByteCode_)
            shaderByteCode_->Release();
        if (shader_)
            shader_->Release();
    }

    bool DX11ComputeShader::CompileShader(const std::vector<std::string>& sources, const std::string& defines)
    {        
        std::stringstream ss;
        for (auto& str : sources)
        {
            ss << str;
            ss << std::endl;
        }

        return CompileShader(ss.str(), defines);
    }

    bool DX11ComputeShader::CompileShader(const std::string& source, const std::string& defines)
    {
        isCompiled_ = false;
        DX11ComputeDevice* device = (DX11ComputeDevice*)device_;
        if (!device || !device->GetD3DDevice())
            return false;

        unsigned flags = D3DCOMPILE_OPTIMIZATION_LEVEL3;
        
        ID3DBlob* errors = 0x0;
        
        // Defines are expected to appear in OpenCL form as "-D Name=Definition"
        std::vector<D3D_SHADER_MACRO> shaderDefines;
        std::vector< std::pair<std::string, std::string> > defNames;
        if (!defines.empty())
        {
            std::vector<std::string> defs = Split(defines, ' ');
            for (auto def : defs)
            {
                if (StartsWith(def, "-D"))
                    continue;
                std::vector<std::string> terms = Split(def, '=');
                if (terms.size() == 2)
                {
                    std::pair<std::string, std::string> defName(terms[0], terms[1]);
                    defNames.push_back(defName);
                    D3D_SHADER_MACRO macro;
                    macro.Name = defName.first.c_str();
                    macro.Definition = defName.second.c_str();
                    shaderDefines.push_back(macro);
                }
                else if (terms.size() == 1)
                {
                    std::pair<std::string, std::string> defName(terms[0], "1");
                    defNames.push_back(defName);
                    D3D_SHADER_MACRO macro;
                    macro.Name = defName.first.c_str();
                    macro.Definition = defName.second.c_str();
                    shaderDefines.push_back(macro);
                }
            }
        }
        D3D_SHADER_MACRO endMacro;
        endMacro.Name = 0;
        endMacro.Definition = 0;
        shaderDefines.push_back(endMacro);

        const int errCode = D3DCompile(source.c_str(), source.length(), name_.c_str(), shaderDefines.data(), 0x0, "", "", flags, 0, &shaderByteCode_, &errors);
        if (errCode < 0) // failed to compile
        {
            // TODO: parse shader info and dump the parameters
        }

        if (errors)
            errors->Release();

        if (errCode >= 0)
            isCompiled_ = true;

        return errCode >= 0;
    }

    ComputeKernel* DX11ComputeShader::GetKernel(const std::string& name)
    {
        if (device_ && shader_)
            return new DX11ComputeKernel(name, device_, shader_);
        return 0x0;
    }

}