#ifndef SUBDIVIDE_H
#define SUBDIVIDE_H
#include <SprueEngine/Libs/BMesh/array.h>
#include <SprueEngine/Libs/BMesh/3dstruct.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef struct subdivModel subdivModel;

typedef struct subdivLink {
    int index;
    int nextLink;
} subdivLink;

typedef struct subdivVertex {
    vec3 v;
    vec3 avgNorm;
    int index;
    int newVertexIndex;
    int edgeLink;
    int edgeNum;
    int faceLink;
    int faceNum;
    int indexOnNewModel;
} subdivVertex;

typedef struct subdivEdge {
    int index;
    int faceLink;
    int faceNum;
    int v[2];
    int edgePt;
    vec3 avg;
} subdivEdge;

typedef struct subdivFace {
    int index;
    int edgeLink;
    int edgeNum;
    int vertexLink;
    int vertexNum;
    vec3 norm;
    int avg;
} subdivFace;

struct subdivModel {
    array *vertexArray;
    array *faceArray;
    array *edgeArray;
    array *indexArray;
    int faceLink;
    int faceNum;
    int edgeLink;
    int edgeNum;
    int vertexLink;
    int vertexNum;
};

subdivModel *subdivCreateModel(void);
void subdivDestroyModel(subdivModel *model);
int subdivAddVertex(subdivModel *model, vec3 *v);
int subdivAddTriangleFace(subdivModel *model, int p1, int p2, int p3);
int subdivAddQuadFace(subdivModel *model, int p1, int p2, int p3, int p4);
subdivModel *subdivCatmullClark(subdivModel *model);
subdivModel *subdivCatmullClarkWithLoops(subdivModel *model, int loops);
void subdivAddCube(subdivModel *model);
void subdivCalculteNorms(subdivModel *model);
void subdivDrawModel(subdivModel *model);

#ifdef __cplusplus
}
#endif

#endif
