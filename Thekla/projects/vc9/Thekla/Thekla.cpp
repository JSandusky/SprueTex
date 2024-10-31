// This is the main DLL file.

#include "stdafx.h"

#include "Thekla.h"

#include <thekla_atlas.h>

#include <algorithm>
#include <vector>

namespace Thekla
{

#define NORMALIZE(val, max) (val / max)

    AtlasOutput^ AtlasBuilder::ComputeTextureCoordinates(System::Collections::Generic::List<VertexPositionNormalTexture>^ meshVertices, System::Collections::Generic::List<int>^ meshIndices)
    {
        if (meshVertices == nullptr || meshIndices == nullptr)
            return nullptr;
        if (meshVertices->Count == 0 || meshIndices->Count == 0)
            return nullptr;

        Atlas_Input_Mesh input_mesh;
        std::vector<Atlas_Input_Vertex> vertices(meshVertices->Count);
        std::vector<Atlas_Input_Face> faces(meshIndices->Count / 3);

        //pin_ptr<VertexPositionNormalTexture> vertPtr = &meshVertices[0];

        //TODO
        for (int i = 0; i < meshVertices->Count; ++i)
        {
            auto& atlasVert = vertices[i];
            auto srcvert = meshVertices[i];

            atlasVert.position[0] = srcvert.Position.X;
            atlasVert.position[1] = srcvert.Position.Y;
            atlasVert.position[2] = srcvert.Position.Z;
            atlasVert.normal[0] = srcvert.Normal.X;
            atlasVert.normal[1] = srcvert.Normal.Y;
            atlasVert.normal[2] = srcvert.Normal.Z;
            //memcpy(atlasVert.position, &srcvert.Position, sizeof(float) * 3);
            //memcpy(atlasVert.normal, &srcvert.Normal, sizeof(float) * 3);
            atlasVert.uv[0] = 0;
            atlasVert.uv[1] = 0;
            atlasVert.first_colocal = i;
        }

        for (int i = 0; i < meshIndices->Count; i += 3)
        {
            auto& atlasFace = faces[i / 3];
            atlasFace.vertex_index[0] = meshIndices[i];
            atlasFace.vertex_index[1] = meshIndices[i + 1];
            atlasFace.vertex_index[2] = meshIndices[i + 2];
            atlasFace.material_index = 0;
        }

        input_mesh.vertex_count = meshVertices->Count;
        input_mesh.vertex_array = vertices.data();
        input_mesh.face_count = meshIndices->Count / 3;
        input_mesh.face_array = faces.data();

        // Generate Atlas_Output_Mesh.
        Atlas_Options atlas_options;
        atlas_set_default_options(&atlas_options);

        atlas_options.packer_options.witness.block_align = true;
        atlas_options.packer_options.witness.conservative = false;

        atlas_options.charter_options.witness.max_chart_area = 60;
        atlas_options.charter_options.witness.roundness_metric_weight = 0.05f;
        atlas_options.charter_options.witness.max_boundary_length = 50;

        // Avoid brute force packing, since it can be unusably slow in some situations.
        atlas_options.packer_options.witness.packing_quality = 1;
        Atlas_Error error = Atlas_Error_Success;
        Atlas_Output_Mesh* output_mesh = 0x0;
        try {
            output_mesh = atlas_generate(&input_mesh, &atlas_options, &error);
        }
        catch (...)
        {
            return nullptr;
        }

        // TODO
        if (output_mesh != 0x0)
        {
            AtlasOutput^ ret = gcnew AtlasOutput();

            float maxDim = std::max(output_mesh->atlas_height, output_mesh->atlas_width);
            float width = output_mesh->atlas_width;
            float height = output_mesh->atlas_height;
            ret->vertices = gcnew array<VertexPositionNormalTexture>(output_mesh->vertex_count);
            ret->indices = gcnew array<int>(output_mesh->index_count);

            for (int i = 0; i < output_mesh->vertex_count; ++i)
            {
                Atlas_Output_Vertex& outVert = output_mesh->vertex_array[i];
                // probably could just reference the function input data
                auto originalVert = (*meshVertices)[outVert.xref];
                //Atlas_Input_Vertex& originalVert = input_mesh.vertex_array[outVert.xref];

                ret->vertices[i].Position = originalVert.Position;
                ret->vertices[i].Normal = originalVert.Normal;
                //ret->vertices[i].Position.X = originalVert.position[0];
                //ret->vertices[i].Position.Y = originalVert.position[1];
                //ret->vertices[i].Position.Z = originalVert.position[2];
                //ret->vertices[i].Normal.X = originalVert.normal[0];
                //ret->vertices[i].Normal.Y = originalVert.normal[1];
                //ret->vertices[i].Normal.Z = originalVert.normal[2];
                ret->vertices[i].TextureCoordinate.X = NORMALIZE(outVert.uv[0], maxDim);
                ret->vertices[i].TextureCoordinate.Y = NORMALIZE(outVert.uv[1], maxDim);
            }

            for (int i = 0; i < output_mesh->index_count; ++i)
                ret->indices[i] = output_mesh->index_array[i];

            atlas_free(output_mesh);
            return ret;
        }

        return nullptr;
    }

}