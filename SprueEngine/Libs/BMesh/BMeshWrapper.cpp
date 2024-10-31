#include "BMeshWrapper.h"

#include "bmesh.h"
#include "subdivide.h"
#include "array.h"

#include <SprueEngine/Geometry/MeshData.h>

namespace SprueEngine
{
    static subdivFace* getFace(subdivModel *model, int index) {
        if (-1 == index) {
            return 0;
        }
        return (subdivFace*)arrayGetItem(model->faceArray, index);
    }

    subdivVertex* getVertex(subdivModel *model, int index) {
        if (-1 == index) {
            return 0;
        }
        return (subdivVertex*)arrayGetItem(model->vertexArray, index);
    }

    MeshData* BuildBMesh(std::vector<BMeshBall> balls, std::vector<BMeshLink> links, int subdivisions)
    {
        auto bMesh = bmeshCreate();

        bmeshBall ball;
        bmeshBone bone;

        for (auto& b : balls)
        {
            memset(&ball, 0, sizeof(ball));
            ball.position.x = b.position_.x;
            ball.position.y = b.position_.y;
            ball.position.z = b.position_.z;
            ball.radius = b.radius_;
            ball.radiusX = b.radiusX_;
            ball.radiusY = b.radiusY_;
            ball.type = b.type_;
            bmeshAddBall(bMesh, &ball);
        }

        for (auto& b : links)
        {
            memset(&bone, 0, sizeof(bone));
            bone.firstBallIndex = b.start_;
            bone.secondBallIndex = b.end_;
            bmeshAddBone(bMesh, &bone);
        }

        bmeshGenerate(bMesh, subdivisions);

        if (subdivModel* model = bmeshSubdivModel(bMesh))
        {
            MeshData* ret = new MeshData();

            subdivLink *linkItem;
            subdivFace *f;
            subdivVertex *v;
            int faceIterator;
            int vertexIterator;
            faceIterator = model->faceLink;

            //int junk = model->vertexLink;
            //while (-1 != junk)
            //{
            //    linkItem = (subdivLink*)arrayGetItem(model->vertexArray, junk);
            //    v = getVertex(model, linkItem->index);
            //    Vec3 pos(&v->v.x);
            //    ret->positionBuffer_.push_back(pos);
            //    junk = linkItem->nextLink;
            //}

            std::map<Vec3, int> indexMap;
            while (-1 != faceIterator) {
                linkItem = (subdivLink*)arrayGetItem(model->indexArray, faceIterator);
                f = getFace(model, linkItem->index);
                faceIterator = linkItem->nextLink;
                vertexIterator = f->vertexLink;
                
                const int indexStart = ret->positionBuffer_.size();
                std::vector<int> indices;
                //for (size_t j = 2; j < f->vertexNum; j++)
                //{
                //    ret->indexBuffer_.push_back(indexStart);
                //    ret->indexBuffer_.push_back(indexStart + j - 1);
                //    ret->indexBuffer_.push_back(indexStart + j);
                //}

                //glBegin(GL_POLYGON);
                int curIndex = indexStart;
                while (-1 != vertexIterator) {
                    linkItem = (subdivLink*)arrayGetItem(model->indexArray, vertexIterator);
                    v = getVertex(model, linkItem->index);
                    vertexIterator = linkItem->nextLink;

                    Vec3 pos(&v->v.x);
                    auto foundVert = indexMap.find(pos);
                    int index = indexStart;
                    if (foundVert != indexMap.end())
                        index = foundVert->second;
                    else
                    {
                        indexMap[pos] = index = curIndex++;
                        ret->positionBuffer_.push_back(pos);
                    }

                    indices.push_back(index);
                    //Vec3 nor(&f->norm.x);
                }

                for (size_t j = 2; j < indices.size(); j++)
                {
                    ret->indexBuffer_.push_back(indices[0]);
                    ret->indexBuffer_.push_back(indices[j - 1]);
                    ret->indexBuffer_.push_back(indices[j]);
                }
                //glEnd();
            }

            if (ret->positionBuffer_.size())
            {
                ret->CalculateNormals();
                bmeshDestroy(bMesh);
                return ret;
            }
            delete ret;
        }
        

        bmeshDestroy(bMesh);
        return nullptr;
    }

}