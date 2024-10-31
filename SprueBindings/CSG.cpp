#include "Stdafx.h"

#include "CSG.h"
#include "Geometry.h"

#include <SprueEngine/Core/Context.h>
#include <SprueEngine/MessageLog.h>
#include <SprueEngine/FString.h>
#include <SprueEngine/Logging.h>
#include <SprueEngine/Geometry/MeshData.h>
#include <SprueEngine/Meshing/MeshMerge.h>
#include <SprueEngine/StringHash.h>

#undef EPSILON

#include <Carve/carve.hpp>
#include <Carve/csg.hpp>
#include <Carve/csg_triangulator.hpp>
#include <Carve/interpolator.hpp>
#include <Carve/mesh.hpp>
#include <Carve/mesh_simplify.hpp>

using namespace SprueEngine;

namespace SprueBindings
{
    struct CSGVertex
    {
        Vec3 pos;
        Vec3 nor;
        Vec2 uv;

        inline bool operator<(const CSGVertex& rhs) const
        {
            return std::tuple<Vec3, Vec3, Vec2>(pos, nor, uv) <  std::tuple<Vec3, Vec3, Vec2>(rhs.pos, rhs.nor, rhs.uv);
        }
    };
}

namespace std
{
    template<> struct hash<SprueBindings::CSGVertex>
    {
        uint32_t operator()(const CSGVertex & v) const {
            return SprueEngine::StringHash::fnv1a(&v, sizeof(SprueBindings::CSGVertex));
        }
    };
}

namespace SprueBindings
{

#define VERTEX_TO_INDEX(VERTEX, MESH) (VERTEX) - MESH->vertex_storage.data()

#define GET_NORMAL_ATTRIBUTE(VERT_INDEX, INDEX) internalMesh->normalBuffer_[VERT_INDEX] = attr->NormalsAttr.getAttribute(fa, INDEX, Vec3())
#define GET_UV_ATTRIBUTE(VERT_INDEX, INDEX) internalMesh->uvBuffer_[VERT_INDEX] = attr->UVAttr.getAttribute(fa, INDEX, Vec2(FLT_MIN, FLT_MIN))

#define MESH_DIM 3

    typedef carve::mesh::MeshSet<MESH_DIM> CSGMeshType;
    typedef std::map<CSGVertex, unsigned> CSGColocalMap;

    struct CSGSmoothingHook : carve::csg::CSG::Hook
    {

    };

    struct CSGAttributes
    {
        carve::interpolate::FaceVertexAttr<Vec3> NormalsAttr;
        carve::interpolate::FaceVertexAttr<Vec2> UVAttr;

        void Hook(carve::csg::CSG& csg)
        {
            NormalsAttr.installHooks(csg);
            UVAttr.installHooks(csg);
        }
    };

    struct CSGMeshData
    {
        CSGMeshType* mesh = 0x0;

        bool IsValid() const { return mesh != 0x0; }

        void SetMesh(CSGMeshType* mesh)
        {
            if (this->mesh)
                delete this->mesh;
            this->mesh = mesh;
        }

        ~CSGMeshData()
        {
            if (mesh)
                delete mesh;
        }
    };

    std::shared_ptr<CSGMeshData> FillMesh(CSGOperand^ operand, CSGAttributes& attrs)
    {
        std::shared_ptr<CSGMeshData> ret;
        if (operand->Geometry != nullptr)
        {
            if (auto meshData = operand->Geometry->GetInternalMeshData())
            {
                std::vector<CSGMeshType::face_t*> faces = std::vector<CSGMeshType::face_t *>();
                std::vector<CSGMeshType::vertex_t*> vertexStorage(meshData->positionBuffer_.size());

                for (int vertIdx = 0; vertIdx < meshData->positionBuffer_.size(); ++vertIdx)
                {
                    auto position = meshData->positionBuffer_[vertIdx];
                    CSGMeshType::vertex_t* vertData = new CSGMeshType::vertex_t(carve::geom::VECTOR(position.x, position.y, position.z));
                    vertexStorage[vertIdx] = vertData;
                }

                for (int idx = 0; idx < meshData->indexBuffer_.size(); idx += 3)
                {
                    const int indices[] = {
                        meshData->indexBuffer_[idx],
                        meshData->indexBuffer_[idx+1],
                        meshData->indexBuffer_[idx+2],
                    };

                    auto a = vertexStorage[indices[0]];
                    auto b = vertexStorage[indices[1]];
                    auto c = vertexStorage[indices[2]];

                    auto face = new CSGMeshType::face_t(a, b, c);

                    // normals
#define WRITE_ATTRIBUTE(ATTR, BUFFER, INDEX, OINDEX) if (indices[INDEX] < meshData-> BUFFER .size()) ATTR.setAttribute(face, OINDEX, meshData-> BUFFER [indices[OINDEX]])

                    WRITE_ATTRIBUTE(attrs.NormalsAttr, normalBuffer_, 0, 0);
                    WRITE_ATTRIBUTE(attrs.NormalsAttr, normalBuffer_, 1, 1);
                    WRITE_ATTRIBUTE(attrs.NormalsAttr, normalBuffer_, 2, 2);
                    WRITE_ATTRIBUTE(attrs.UVAttr, uvBuffer_, 0, 0);
                    WRITE_ATTRIBUTE(attrs.UVAttr, uvBuffer_, 1, 1);
                    WRITE_ATTRIBUTE(attrs.UVAttr, uvBuffer_, 2, 2);
                    faces.push_back(face);
                }

                auto retMesh = new CSGMeshType(faces);
                ret.reset(new CSGMeshData());
                //retMesh->canonicalize();
                ret->SetMesh(retMesh);
            }
        }
        return ret;
    }

    // add a vertex if new, otherwise return the existing index
    unsigned CSGAddVertex(CSGVertex& vert, CSGColocalMap& colocalTable, std::vector<CSGVertex>& allVerts, SprueEngine::MeshData* meshData)
    {
        auto found = colocalTable.find(vert);
        if (found == colocalTable.end())
        {
            unsigned idx = allVerts.size();
            allVerts.push_back(vert);
            colocalTable[vert] = idx;

            meshData->positionBuffer_.push_back(vert.pos);
            meshData->normalBuffer_.push_back(vert.nor);
            meshData->uvBuffer_.push_back(vert.uv);

            return idx;
        }
        return found->second;
    }

    CSGVertex CSGGetFaceVertex(CSGMeshType::face_t* face, unsigned vertIdx, CSGMeshType::vertex_t* vert, CSGAttributes* attr)
    {
        CSGVertex ret;
        ret.pos = Vec3(vert->v[0], vert->v[1], vert->v[2]);
        Vec3 nor = attr->NormalsAttr.getAttribute(face, vertIdx);
        if (nor.Dot(Vec3(face->plane.N[0], face->plane.N[1], face->plane.N[2]).Normalized()) < 0)
            nor *= -1;
        ret.nor = nor.Normalized();
        ret.uv = attr->UVAttr.getAttribute(face, vertIdx);
        return ret;
    }

    SprueBindings::MeshData^ FromCSGMeshCompact(CSGMeshType* meshData, CSGAttributes* attr)
    {
        SprueBindings::MeshData^ outData = gcnew SprueBindings::MeshData(new SprueEngine::MeshData());
        auto internalMesh = outData->GetInternalMeshData();
        CSGColocalMap vertexTable;
        std::vector<CSGVertex> outVertices;
        if (meshData)
        {
            auto mesh = meshData->meshes[0];
            //for (auto faceIter = mesh->faces.begin(); faceIter != mesh->faces.end(); ++faceIter)
            for (auto faceIter = meshData->faceBegin(); faceIter != meshData->faceEnd(); ++faceIter)
            {
                CSGMeshType::face_t* face = *faceIter;
                std::vector<CSGMeshType::vertex_t *> verts;
                face->getVertices(verts);

                // construct a vertex for each face vertex
                CSGVertex va = CSGGetFaceVertex(face, 0, verts[0], attr);
                CSGVertex vb = CSGGetFaceVertex(face, 1, verts[1], attr);
                CSGVertex vc = CSGGetFaceVertex(face, 2, verts[2], attr);

                // attempt to add the vertices to our output mesh
                // or get lookup IDs for colocal vertices with matching attributes
                unsigned iA = CSGAddVertex(va, vertexTable, outVertices, internalMesh);
                unsigned iB = CSGAddVertex(vb, vertexTable, outVertices, internalMesh);
                unsigned iC = CSGAddVertex(vc, vertexTable, outVertices, internalMesh);

                internalMesh->indexBuffer_.push_back(iA);
                internalMesh->indexBuffer_.push_back(iB);
                internalMesh->indexBuffer_.push_back(iC);
            }
        }
        internalMesh->CalculateNormals();
        return outData;
    }

    SprueBindings::MeshData^ FromCSGMesh(CSGMeshType* meshData, CSGAttributes* attr)
    {
        SprueBindings::MeshData^ outData = gcnew SprueBindings::MeshData(new SprueEngine::MeshData());
        auto internalMesh = outData->GetInternalMeshData();
        std::unordered_map<CSGVertex, unsigned> vertexTable;
        std::vector<CSGVertex> outVertices;

        if (meshData)
        {
            internalMesh->positionBuffer_.resize(meshData->vertex_storage.size());
            internalMesh->normalBuffer_.resize(meshData->vertex_storage.size());
            internalMesh->uvBuffer_.resize(meshData->vertex_storage.size());
            int idx = 0;
            for (auto vert : meshData->vertex_storage)
            {
                internalMesh->positionBuffer_[idx].Set(vert.v[0], vert.v[1], vert.v[2]);
                idx++;
            }

            // count face indices
            unsigned indexCount = 0;
            for (auto face = meshData->faceBegin(); face != meshData->faceEnd(); ++face)
                indexCount += 3;

            internalMesh->indexBuffer_.resize(indexCount);
            int currentIndex = 0;
            for (auto face = meshData->faceBegin(); face != meshData->faceEnd(); ++face, currentIndex += 3)
            {
                CSGMeshType::face_t*fa = *face;
                std::vector<CSGMeshType::vertex_t *> verts;
                fa->getVertices(verts);

                unsigned a = VERTEX_TO_INDEX(verts[0], meshData);
                unsigned b = VERTEX_TO_INDEX(verts[1], meshData);
                unsigned c = VERTEX_TO_INDEX(verts[2], meshData);

                internalMesh->indexBuffer_[currentIndex] = a;
                internalMesh->indexBuffer_[currentIndex+1] = b;
                internalMesh->indexBuffer_[currentIndex+2] = c;

                // do we have normals?
                GET_NORMAL_ATTRIBUTE(currentIndex, 0);
                GET_NORMAL_ATTRIBUTE(currentIndex + 1, 1);
                GET_NORMAL_ATTRIBUTE(currentIndex + 2, 2);
                
                // do we have UV?   
                GET_UV_ATTRIBUTE(currentIndex, 0);
                GET_UV_ATTRIBUTE(currentIndex + 1, 1);
                GET_UV_ATTRIBUTE(currentIndex + 2, 2);
            }
        }
        return outData;
    }

    carve::csg::CSG::OP ConvertToCarveOp(SprueBindings::CSGTask task)
    {
        switch (task)
        {
        case SprueBindings::CSGTask::CSG_Add:
            return carve::csg::CSG::OP::UNION;
        case SprueBindings::CSGTask::CSG_Subtract:
            return carve::csg::CSG::OP::A_MINUS_B;
        case SprueBindings::CSGTask::CSG_Clip:
        case SprueBindings::CSGTask::ClipIndependent:
            return carve::csg::CSG::OP::B_MINUS_A_ONLY_B;
        }
        return carve::csg::CSG::OP::UNION;
    }

    MeshData^ CSGCarve(List<CSGOperand^>^ meshes)
    {
        CSGMeshType* workingMesh = 0x0;
        std::shared_ptr<CSGMeshData> firstData;
        std::vector<std::shared_ptr<CSGMeshData> > csgMeshList;
        CSGAttributes csgAttributes;
        for (int i = 0; i < meshes->Count; ++i)
        {
            auto meshOperand = (*meshes)[i];
            if (meshOperand == nullptr)
                continue;
            auto meshData = FillMesh(meshOperand, csgAttributes);
            bool operate = false;
            if (meshData.get() && meshData->IsValid())
            {
                if (firstData.get() == 0x0)
                    firstData = meshData;
                else
                    operate = true;

                csgMeshList.push_back(meshData);
            }

            if (operate)
            {
                carve::csg::CSG csg;
                csg.hooks.registerHook(new carve::csg::CarveTriangulatorWithImprovement(), carve::csg::CSG::Hooks::PROCESS_OUTPUT_FACE_BIT);
                csgAttributes.Hook(csg);
                carve::csg::V2Set sharedEdges;
                std::set<CSGMeshType::vertex_t*> sharedVertsSet;
                std::map<Vec3, Vec3> sharedVertNormals;
                std::map<Vec3, Vec2> smoothFactors;
                CSGMeshType* resultMesh = 0x0;
                try 
                {    
                    resultMesh = csg.compute(firstData->mesh, meshData->mesh, carve::csg::CSG::A_MINUS_B_ONLY_A/*ConvertToCarveOp(meshOperand->Task)*/, 0x0);
                }
                catch (carve::exception except)
                {

                }
                if (resultMesh != 0x0)
                {
                    std::shared_ptr<CSGMeshData> resultMeshData(new CSGMeshData());
                    // Smooth the shared edges of the CSG result
                    if (meshOperand->UseSmoothing)
                    {
                        const float smoothPower = meshOperand->Task == CSGTask::MergeSmoothNormals ? meshOperand->SmoothingPower : -meshOperand->SmoothingPower;
                        for (auto sharedEdge : sharedEdges)
                        {
                            auto a = Vec3(sharedEdge.first->v[0], sharedEdge.first->v[1], sharedEdge.first->v[2]);
                            auto b = Vec3(sharedEdge.second->v[0], sharedEdge.second->v[1], sharedEdge.second->v[2]);
                            sharedVertsSet.insert(sharedEdge.first);
                            sharedVertsSet.insert(sharedEdge.second);

                            // use this to calculate smoothing power
                            Vec2 smoothFactor(1, a.DistanceSq(b));
                            smoothFactors[a] += smoothFactor;
                            smoothFactors[b] += smoothFactor;
                        }

                        for (auto faceIter = resultMesh->faceBegin(); faceIter != resultMesh->faceEnd(); ++faceIter)
                        {
                            auto face = *faceIter;
                            std::vector<CSGMeshType::vertex_t*> vertices;
                            face->getVertices(vertices);

                            for (int i = 0; i < vertices.size(); ++i)
                            {
                                if (sharedVertsSet.find(vertices[i]) != sharedVertsSet.end())
                                {
                                    Vec3 v(vertices[i]->v[0], vertices[i]->v[1], vertices[i]->v[2]);
                                    sharedVertNormals[v] += Vec3(face->plane.N[0], face->plane.N[1], face->plane.N[2]);
                                }
                            }

                            std::unordered_set<CSGMeshType::vertex_t*> closedList;
                            for (auto& vert : resultMesh->vertex_storage)
                            {
                                // don't multiprocess
                                if (closedList.find(&vert) != closedList.end())
                                    continue;

                                if (sharedVertsSet.find(&vert) != sharedVertsSet.end())
                                {
                                    const Vec3 v(vert.v[0], vert.v[1], vert.v[2]);
                                    const Vec3 normal = sharedVertNormals[v].Normalized();
                                    const auto sourceFactorData = smoothFactors[v];
                                    const float baseSmoothPower = sqrtf(sourceFactorData.y / sourceFactorData.x);

                                    vert.v[0] += normal.x * baseSmoothPower * smoothPower;
                                    vert.v[1] += normal.y * baseSmoothPower * smoothPower;
                                    vert.v[2] += normal.z * baseSmoothPower * smoothPower;
                                    closedList.insert(&vert);
                                }
                            }

                            for (auto mesh : resultMesh->meshes)
                                mesh->recalc();
                        }
                    }

                    resultMeshData->SetMesh(resultMesh);
                    firstData = resultMeshData;
                    csgMeshList.push_back(resultMeshData);
                }

            }
        }

        if (firstData.get() && firstData->IsValid())
            return FromCSGMeshCompact(firstData->mesh, &csgAttributes);
        return nullptr;
    }

    MeshData^ CSGProcessor::ProcessUnions(List<CSGOperand^>^ meshes, PluginLib::IErrorPublisher^ publisher)
    {
        if (meshes == nullptr)
            return nullptr;

        auto val = CSGCarve(meshes);
        if (val != nullptr)
        {
            SprueBindings::ModelData^ modelMdl = gcnew SprueBindings::ModelData();
            modelMdl->Meshes = gcnew List<MeshData^>();
            modelMdl->Meshes->Add(val);
            SprueBindings::ModelData::SaveOBJ(modelMdl, "TestCarveCSG.obj", publisher);
        }
        MeshData^ ret = nullptr;

        SprueEngine::MeshMerger* merger = new SprueEngine::MeshMerger();
        SprueEngine::MeshData* firstMeshData = 0x0;
        for (int i = 0; i < meshes->Count; ++i)
        {
            CSGOperand^ current = (*meshes)[i];
            if (firstMeshData == 0x0 && current != nullptr)
            {
                firstMeshData = current->Geometry->GetInternalMeshData();
                ret = gcnew MeshData(firstMeshData->CloneRaw());
            }
            else if (firstMeshData != 0x0 && current != nullptr)
            {
                if (SprueEngine::MeshData* rhs = current->Geometry->GetInternalMeshData())
                {
                    auto origMat = current->Transform;
                    Microsoft::Xna::Framework::Vector3 pos, scl;
                    Microsoft::Xna::Framework::Quaternion rot;
                    origMat.Decompose(scl, rot, pos);
                    SprueEngine::Quat q = SprueEngine::Quat(rot.X, rot.Y, rot.Z, rot.W);
                    Mat3x4 mat = SprueEngine::Mat3x4::FromTRS(Vec3(pos.X, pos.Y, pos.Z), q, Vec3(scl.X, scl.Y, scl.Z));

                    //auto transMat = Microsoft::Xna::Framework::Matrix::Transpose(current->Transform);
                    //SprueEngine::Mat3x4 mat;
                    //mat[0][0] = transMat.M11;
                    //mat[0][1] = transMat.M12;
                    //mat[0][2] = transMat.M13;
                    //mat[0][3] = transMat.M14;
                    //
                    //mat[1][0] = transMat.M21;
                    //mat[1][1] = transMat.M22;
                    //mat[1][2] = transMat.M23;
                    //mat[1][3] = transMat.M24;
                    //
                    //mat[2][0] = transMat.M31;
                    //mat[2][1] = transMat.M32;
                    //mat[2][2] = transMat.M33;
                    //mat[2][3] = transMat.M34;

                    //System::Console::WriteLine(System::String::Format("{0} {1} {2} {3}", mat[0][0], mat[0][1], mat[0][2], mat[0][3]));
                    
                    //SprueEngine::Context::GetInstance()->GetLog()->Log("", SprueEngine::LogLevel::LL_WARNING);
                    SPRUE_LOG_DEBUG("SprueEngine::Mat3x4");
                    SPRUE_LOG_WARNING(FString("%1 %2 %3 %4", mat[0][0], mat[0][1], mat[0][2], mat[0][3]).str());
                    SPRUE_LOG_WARNING(FString("%1 %2 %3 %4", mat[1][0], mat[1][1], mat[1][2], mat[1][3]).str());
                    SPRUE_LOG_WARNING(FString("%1 %2 %3 %4", mat[2][0], mat[2][1], mat[2][2], mat[2][3]).str());

                    SPRUE_LOG_DEBUG("Input Matrix");
                    SPRUE_LOG_WARNING(FString("%1 %2 %3 %4", origMat.M11, origMat.M12, origMat.M13, origMat.M14).str());
                    SPRUE_LOG_WARNING(FString("%1 %2 %3 %4", origMat.M21, origMat.M22, origMat.M23, origMat.M24).str());
                    SPRUE_LOG_WARNING(FString("%1 %2 %3 %4", origMat.M31, origMat.M32, origMat.M33, origMat.M34).str());
                    SPRUE_LOG_WARNING(FString("%1 %2 %3 %4", origMat.M41, origMat.M42, origMat.M43, origMat.M44).str());

                    SPRUE_LOG_DEBUG("Transposed Input Matrix");
                    //SPRUE_LOG_WARNING(FString("%1 %2 %3 %4", transMat.M11, transMat.M12, transMat.M13, transMat.M14).str());
                    //SPRUE_LOG_WARNING(FString("%1 %2 %3 %4", transMat.M21, transMat.M22, transMat.M23, transMat.M24).str());
                    //SPRUE_LOG_WARNING(FString("%1 %2 %3 %4", transMat.M31, transMat.M32, transMat.M33, transMat.M34).str());
                    //SPRUE_LOG_WARNING(FString("%1 %2 %3 %4", transMat.M41, transMat.M42, transMat.M43, transMat.M44).str());

                    switch (current->Task)
                    {
                    case CSGTask::CSG_Add:
                        merger->CSGUnion(ret->GetInternalMeshData(), ret->GetInternalMeshData(), rhs, mat, true);
                        break;
                    case CSGTask::CSG_Subtract:
                        merger->CSGSubtract(ret->GetInternalMeshData(), ret->GetInternalMeshData(), rhs, mat, true);
                        break;
                    case CSGTask::Merge:
                        merger->CombineMeshes(ret->GetInternalMeshData(), ret->GetInternalMeshData(), rhs, mat);
                        break;
                    }
                }
            }
        }

        delete merger;
        ret->CalculateNormals();
        return ret;
    }

}