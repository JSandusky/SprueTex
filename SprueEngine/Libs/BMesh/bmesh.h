#ifndef B_MESH_H
#define B_MESH_H
#include <SprueEngine/Libs/BMesh/vector3d.h>
#include <SprueEngine/Libs/BMesh/draw.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef enum {
  BMESH_BALL_TYPE_KEY = 0,
  BMESH_BALL_TYPE_ROOT = 1,
  BMESH_BALL_TYPE_INBETWEEN = 2,
} bmeshBallType;

typedef struct bmesh bmesh;

typedef struct {
  int index;
  vec3 position;
  float radius;
  float radiusX;
  float radiusY;
  int type;
  int childrenIndices;
  int firstChildIndex;
  vec3 boneDirection;
  vec3 localYaxis;
  vec3 localZaxis;
  int roundColor;
  int notFitHull;
  int flagForHull;
  int convexHullCount;
  int meshGenerated;
} bmeshBall;

typedef int bmeshBallIterator;

typedef struct {
  int index;
  int firstBallIndex;
  int secondBallIndex;
} bmeshBone;

bmesh *bmeshCreate(void);
void bmeshDestroy(bmesh *bm);
int bmeshGetBallNum(bmesh *bm);
int bmeshGetBoneNum(bmesh *bm);
bmeshBall *bmeshGetBall(bmesh *bm, int index);
bmeshBone *bmeshGetBone(bmesh *bm, int index);
int bmeshAddBall(bmesh *bm, bmeshBall *ball);
int bmeshAddBone(bmesh *bm, bmeshBone *bone);
int bmeshGenerateInbetweenBalls(bmesh *bm);
bmeshBall *bmeshGetBallFirstChild(bmesh *bm, bmeshBall *node,
  bmeshBallIterator *iterator);
bmeshBall *bmeshGetBallNextChild(bmesh *bm, bmeshBall *node,
  bmeshBallIterator *iterator);
bmeshBall *bmeshGetRootBall(bmesh *bm);
int bmeshGetQuadNum(bmesh *bm);
quad *bmeshGetQuad(bmesh *bm, int index);
int bmeshAddQuad(bmesh *bm, quad *q);
int bmeshSweep(bmesh *bm);
int bmeshStitch(bmesh *bm);
int bmeshGenerateInbetweenMesh(bmesh *bm);
int bmeshGenerate(bmesh *bm, int level);
int bmeshDraw(bmesh *bm);

typedef struct subdivModel subdivModel;
subdivModel* bmeshSubdivModel(bmesh* bm);

#ifdef __cplusplus
}
#endif

#endif
