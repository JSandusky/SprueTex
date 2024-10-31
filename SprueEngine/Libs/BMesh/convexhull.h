#ifndef CONVEX_HULL_H
#define CONVEX_HULL_H
#include <SprueEngine/Libs/BMesh/3dstruct.h>

typedef struct convexHull convexHull;

typedef struct {
  union {
    face4 q;
    face3 t;
  } u;
  int vertexNum;
  int plane;
} convexHullFace;

typedef struct {
  vec3 pt;
  int plane;
  int orderOnPlane;
} convexHullVertex;

convexHull *convexHullCreate(void);
int convexHullAddVertex(convexHull *hull, vec3 *vertex, int plane,
  int orderOnPlane);
void convexHullDestroy(convexHull *hull);
void convexHullGenerate(convexHull *hull);
void convexHullUnifyNormals(convexHull *hull, vec3 *origin);
void convexHullMergeTriangles(convexHull *hull);
int convexHullGetFaceNum(convexHull *hull);
convexHullFace *convexHullGetFace(convexHull *hull, int faceIndex);
convexHullVertex *convexHullGetVertex(convexHull *hull, int vertexIndex);
void convexHullAddTodo(convexHull *hull, int vertex1, int vertex2, int vertex3);

#endif
