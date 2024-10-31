#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <assert.h>
#include <math.h>
#include <time.h>
#include "dmemory.h"
#include "bmesh.h"
#include "array.h"
#include "dict.h"
#include "matrix.h"
#include "convexhull.h"
#include "subdivide.h"

#define BMESH_MAX_PARENT_BALL_DEPTH   1000
#define BMESH_INTVAL_DIST_DIV         10
#define BMESH_MODEL_VERTEX_HASHTABLE_SIZE   100

typedef struct bmeshBallIndex {
    int ballIndex;
    int nextChildIndex;
} bmeshBallIndex;

typedef struct bmeshModelVertex {
    vec3 vertex;
    int indexOnModel;
} bmeshModelVertex;

struct bmesh {
    array *ballArray;
    array *boneArray;
    array *indexArray;
    array *quadArray;
    int rootBallIndex;
    int roundColor;
    bmeshBall *parentBallStack[BMESH_MAX_PARENT_BALL_DEPTH];
    subdivModel *model;
    subdivModel *subdivModel;
    array *modelVertexArray;
    dict *modelVertexDict;
    bmeshModelVertex findModelVertex;
};

static bmeshModelVertex *bmeshFindModelVertex(bmesh *bm, vec3 *vertex) {
    return dictFind(bm->modelVertexDict, vertex);
}

static int bmeshAddModelVertex(bmesh *bm, vec3 *vertex) {
    bmeshModelVertex *v = bmeshFindModelVertex(bm, vertex);
    if (!v) {
        v = dictGetClear(bm->modelVertexDict, vertex);
        v->vertex = *vertex;
        v->indexOnModel = subdivAddVertex(bm->model, &v->vertex);
    }
    return v->indexOnModel;
}

static void bmeshAddQuadToModel(bmesh *bm, quad *q) {
    int indices[4];
    indices[0] = bmeshAddModelVertex(bm, &q->pt[0]);
    indices[1] = bmeshAddModelVertex(bm, &q->pt[1]);
    indices[2] = bmeshAddModelVertex(bm, &q->pt[2]);
    indices[3] = bmeshAddModelVertex(bm, &q->pt[3]);
    subdivAddQuadFace(bm->model, indices[0], indices[1], indices[2], indices[3]);
}

static void bmeshAddTriangleToModel(bmesh *bm, triangle *t) {
    int indices[3];
    indices[0] = bmeshAddModelVertex(bm, &t->pt[0]);
    indices[1] = bmeshAddModelVertex(bm, &t->pt[1]);
    indices[2] = bmeshAddModelVertex(bm, &t->pt[2]);
    subdivAddTriangleFace(bm->model, indices[0], indices[1], indices[2]);
}

static int modelVertexHash(void *userData, const void *node) {
    const bmeshModelVertex *v = node;
    return abs(*((int *)v));
}

static int modelVertexCompare(void *userData, const void *firstNode,
    const void *secondNode) {
    const bmeshModelVertex *v1 = firstNode;
    const bmeshModelVertex *v2 = secondNode;
    if (0 == v1->vertex.x - v2->vertex.x &&
        0 == v1->vertex.y - v2->vertex.y &&
        0 == v1->vertex.z - v2->vertex.z) {
        return 0;
    }
    return 1;
}

bmesh *bmeshCreate(void) {
    bmesh *bm = dcalloc(1, sizeof(bmesh));
    bm->ballArray = arrayCreate(sizeof(bmeshBall));
    bm->boneArray = arrayCreate(sizeof(bmeshBone));
    bm->indexArray = arrayCreate(sizeof(bmeshBallIndex));
    bm->quadArray = arrayCreate(sizeof(quad));
    bm->model = subdivCreateModel();
    bm->modelVertexArray = arrayCreate(sizeof(bmeshModelVertex));
    bm->modelVertexDict = dictCreate(bm->modelVertexArray, BMESH_MODEL_VERTEX_HASHTABLE_SIZE, modelVertexHash, modelVertexCompare, bm);
    bm->rootBallIndex = -1;
    bm->roundColor = 0;
    return bm;
}

void bmeshDestroy(bmesh *bm) {
    arrayDestroy(bm->ballArray);
    arrayDestroy(bm->boneArray);
    arrayDestroy(bm->indexArray);
    arrayDestroy(bm->quadArray);
    subdivDestroyModel(bm->model);
    subdivDestroyModel(bm->subdivModel);
    arrayDestroy(bm->modelVertexArray);
    dictDestroy(bm->modelVertexDict);
    dfree(bm);
}

int bmeshGetBallNum(bmesh *bm) {
    return arrayGetLength(bm->ballArray);
}

int bmeshGetBoneNum(bmesh *bm) {
    return arrayGetLength(bm->boneArray);
}

bmeshBall *bmeshGetBall(bmesh *bm, int index) {
    return arrayGetItem(bm->ballArray, index);
}

bmeshBone *bmeshGetBone(bmesh *bm, int index) {
    return arrayGetItem(bm->boneArray, index);
}

int bmeshAddBall(bmesh *bm, bmeshBall *ball) {
    int index = arrayGetLength(bm->ballArray);
    ball->index = index;
    ball->firstChildIndex = -1;
    ball->childrenIndices = 0;
    ball->notFitHull = 0;
    arraySetLength(bm->ballArray, index + 1);
    memcpy(arrayGetItem(bm->ballArray, index), ball, sizeof(bmeshBall));
    if (BMESH_BALL_TYPE_ROOT == ball->type) {
        bm->rootBallIndex = index;
    }
    return index;
}

static void bmeshAddChildBallRelation(bmesh *bm, int parentBallIndex, int childBallIndex) {
    bmeshBall *parentBall = bmeshGetBall(bm, parentBallIndex);
    bmeshBallIndex *indexItem;
    int newChildIndex = arrayGetLength(bm->indexArray);
    arraySetLength(bm->indexArray, newChildIndex + 1);
    indexItem = arrayGetItem(bm->indexArray, newChildIndex);
    indexItem->ballIndex = childBallIndex;
    indexItem->nextChildIndex = parentBall->firstChildIndex;
    parentBall->firstChildIndex = newChildIndex;
    parentBall->childrenIndices++;
}

int bmeshAddBone(bmesh *bm, bmeshBone *bone) {
    int index = arrayGetLength(bm->boneArray);
    bone->index = index;
    arraySetLength(bm->boneArray, index + 1);
    memcpy(arrayGetItem(bm->boneArray, index), bone, sizeof(bmeshBone));
    bmeshAddChildBallRelation(bm, bone->firstBallIndex, bone->secondBallIndex);
    bmeshAddChildBallRelation(bm, bone->secondBallIndex, bone->firstBallIndex);
    return index;
}

static int bmeshAddInbetweenBallBetween(bmesh *bm, bmeshBall *firstBall, bmeshBall *secondBall, float frac, int parentBallIndex) 
{
    bmeshBall newBall;
    memset(&newBall, 0, sizeof(newBall));
    newBall.type = BMESH_BALL_TYPE_INBETWEEN;
    newBall.radius = firstBall->radius * (1 - frac) + secondBall->radius * frac;
    newBall.radiusX = firstBall->radiusX * (1 - frac) + secondBall->radiusX * frac;
    newBall.radiusY = firstBall->radiusY * (1 - frac) + secondBall->radiusY * frac;
    vec3Lerp(&firstBall->position, &secondBall->position, frac, &newBall.position);
    bmeshAddBall(bm, &newBall);
    bmeshAddChildBallRelation(bm, parentBallIndex, newBall.index);
    return newBall.index;
}

static void generateYZfromBoneDirection(vec3 *boneDirection, vec3 *localYaxis, vec3 *localZaxis) {
    vec3 worldYaxis = { 0, 1, 0 };
    vec3CrossProduct(&worldYaxis, boneDirection, localYaxis);
    vec3Normalize(localYaxis);
    vec3CrossProduct(localYaxis, boneDirection, localZaxis);
    vec3Normalize(localZaxis);
}

static void bmeshGenerateInbetweenBallsBetween(bmesh *bm, int firstBallIndex, int secondBallIndex) {
    float step;
    float distance;
    int parentBallIndex = firstBallIndex;
    vec3 localZaxis;
    vec3 localYaxis;
    vec3 boneDirection;
    vec3 normalizedBoneDirection;
    bmeshBall *firstBall = bmeshGetBall(bm, firstBallIndex);
    bmeshBall *secondBall = bmeshGetBall(bm, secondBallIndex);
    bmeshBall *newBall;
    float intvalDist;

    if (secondBall->roundColor == bm->roundColor) {
        return;
    }

    vec3Sub(&firstBall->position, &secondBall->position, &boneDirection);
    normalizedBoneDirection = boneDirection;
    vec3Normalize(&normalizedBoneDirection);
    generateYZfromBoneDirection(&boneDirection, &localYaxis, &localZaxis);

    intvalDist = min(firstBall->radius, secondBall->radius) / BMESH_INTVAL_DIST_DIV;
    //intvalDist = ((firstBall->radius + secondBall->radius) / 2) / BMESH_INTVAL_DIST_DIV;
    step = intvalDist;
    distance = vec3Length(&boneDirection);
    if (distance > intvalDist) {
        float offset;
        int calculatedStepCount = (int)(distance / intvalDist);
        float remaining = distance - intvalDist * calculatedStepCount;
        step += remaining / calculatedStepCount;
        offset = step;
        if (offset < distance) {
            while (offset < distance && offset + intvalDist <= distance) {
                float frac = offset / distance;
                parentBallIndex = bmeshAddInbetweenBallBetween(bm, firstBall, secondBall, frac, parentBallIndex);
                newBall = bmeshGetBall(bm, parentBallIndex);
                newBall->localYaxis = localYaxis;
                newBall->localZaxis = localZaxis;
                newBall->boneDirection = normalizedBoneDirection;
                offset += step;
            }
        }
        else if (distance > step) {
            parentBallIndex = bmeshAddInbetweenBallBetween(bm, firstBall, secondBall, 0.5, parentBallIndex);
            newBall = bmeshGetBall(bm, parentBallIndex);
            newBall->localYaxis = localYaxis;
            newBall->localZaxis = localZaxis;
            newBall->boneDirection = normalizedBoneDirection;
        }
    }
    bmeshAddChildBallRelation(bm, parentBallIndex, secondBallIndex);
}

bmeshBall *bmeshGetBallFirstChild(bmesh *bm, bmeshBall *ball, bmeshBallIterator *iterator) {
    if (-1 == ball->firstChildIndex) {
        return 0;
    }
    *iterator = ball->firstChildIndex;
    return bmeshGetBallNextChild(bm, ball, iterator);
}

bmeshBall *bmeshGetBallNextChild(bmesh *bm, bmeshBall *ball, bmeshBallIterator *iterator) {
    bmeshBallIndex *indexItem;
    if (-1 == *iterator) {
        return 0;
    }
    indexItem = arrayGetItem(bm->indexArray, *iterator);
    *iterator = indexItem->nextChildIndex;
    return bmeshGetBall(bm, indexItem->ballIndex);
}

bmeshBall *bmeshGetRootBall(bmesh *bm) {
    if (-1 == bm->rootBallIndex) {
        return 0;
    }
    return bmeshGetBall(bm, bm->rootBallIndex);
}

static void bmeshGenerateInbetweenBallsFrom(bmesh *bm, int parentBallIndex) {
    bmeshBallIterator iterator;
    int ballIndex;
    bmeshBall *parent;
    bmeshBall *ball;
    int oldChildrenIndices;

    parent = bmeshGetBall(bm, parentBallIndex);
    if (parent->roundColor == bm->roundColor) {
        return;
    }

    parent->roundColor = bm->roundColor;

    //
    // Old indices came from user's input will be removed
    // after the inbetween balls are genereated, though
    // the space occupied in indexArray will not been release.
    //

    ball = bmeshGetBallFirstChild(bm, parent, &iterator);
    parent->firstChildIndex = -1;
    oldChildrenIndices = parent->childrenIndices;
    parent->childrenIndices = 0;

    for (; ball != 0x0; ball = bmeshGetBallNextChild(bm, parent, &iterator)) {
        ballIndex = ball->index;
        bmeshGenerateInbetweenBallsBetween(bm, parentBallIndex, ballIndex);
        bmeshGenerateInbetweenBallsFrom(bm, ballIndex);
    }
}

int bmeshGenerateInbetweenBalls(bmesh *bm) {
    if (-1 == bm->rootBallIndex) {
        fprintf(stderr, "%s:No root ball.\n", __FUNCTION__);
        return -1;
    }
    bm->roundColor++;
    bmeshGenerateInbetweenBallsFrom(bm, bm->rootBallIndex);
    return 0;
}

int bmeshGetQuadNum(bmesh *bm) {
    return arrayGetLength(bm->quadArray);
}

quad *bmeshGetQuad(bmesh *bm, int index) {
    return arrayGetItem(bm->quadArray, index);
}

int bmeshAddQuad(bmesh *bm, quad *q) {
    int index = arrayGetLength(bm->quadArray);
    arraySetLength(bm->quadArray, index + 1);
    memcpy(arrayGetItem(bm->quadArray, index), q, sizeof(quad));
    return index;
}

static void bmeshSweepFrom(bmesh *bm, bmeshBall *parent, bmeshBall *ball) {
    bmeshBallIterator iterator;
    bmeshBall *child = 0;
    if (bm->roundColor == ball->roundColor) {
        return;
    }
    ball->roundColor = bm->roundColor;
    if (BMESH_BALL_TYPE_KEY == ball->type) {
        child = bmeshGetBallFirstChild(bm, ball, &iterator);
        if (child) {
            if (parent) {
                float rotateAngle;
                vec3 rotateAxis;
                vec3CrossProduct(&parent->boneDirection, &child->boneDirection, &rotateAxis);
                vec3Normalize(&rotateAxis);
                rotateAngle = vec3Angle(&parent->boneDirection, &child->boneDirection);
                ball->boneDirection = parent->boneDirection;
                rotateAxis.y = 0;
                rotateAngle *= 0.5;
                vec3RotateAlong(&ball->boneDirection, rotateAngle, &rotateAxis, &ball->boneDirection);
                generateYZfromBoneDirection(&ball->boneDirection, &ball->localYaxis, &ball->localZaxis);
            }
        }
        else {
            ball->boneDirection = parent->boneDirection;
            generateYZfromBoneDirection(&ball->boneDirection, &ball->localYaxis, &ball->localZaxis);
        }
    }
    for (child = bmeshGetBallFirstChild(bm, ball, &iterator); child; child = bmeshGetBallNextChild(bm, ball, &iterator)) 
    {
        bmeshSweepFrom(bm, ball, child);
    }
}

int bmeshSweep(bmesh *bm) {
    bm->roundColor++;
    bmeshSweepFrom(bm, 0, bmeshGetRootBall(bm));
    return 0;
}

static int isDistanceEnoughForConvexHull(bmeshBall *root,
    bmeshBall *ball) {
    float distance = vec3Distance(&root->position, &ball->position);
    if (distance >= root->radius) {
        return 1;
    }
    return 0;
}

static bmeshBall *bmeshFindChildBallForConvexHull(bmesh *bm, bmeshBall *root, bmeshBall *ball) {
    bmeshBallIterator iterator;
    bmeshBall *child;
    if (ball->convexHullCount) {
        return ball;
    }
    if (!ball->notFitHull && isDistanceEnoughForConvexHull(root, ball)) {
        return ball;
    }
    child = bmeshGetBallFirstChild(bm, ball, &iterator);
    if (!child) {
        return ball;
    }
    ball->radius = 0;
    return bmeshFindChildBallForConvexHull(bm, root, child);
}

static bmeshBall *bmeshFindParentBallForConvexHull(bmesh *bm, bmeshBall *root, int depth, bmeshBall *ball) {
    if (depth <= 0 || ball->convexHullCount) {
        return ball;
    }
    if (!ball->notFitHull && isDistanceEnoughForConvexHull(root, ball)) {
        return ball;
    }
    ball->radius = 0;
    return bmeshFindParentBallForConvexHull(bm, root, depth - 1, bm->parentBallStack[depth - 1]);
}

static void addBallToHull(convexHull *hull, bmeshBall *ballForConvexHull, bmeshBall **outmostBall, int *outmostBallFirstVertexIndex) {
    vec3 z, y;
    quad q;
    int vertexIndex[4];
    int needUpdateOutmost = 0;

    vec3Scale(&ballForConvexHull->localYaxis, ballForConvexHull->radius, &y);
    vec3Scale(&ballForConvexHull->localZaxis, ballForConvexHull->radius, &z);
    vec3Sub(&ballForConvexHull->position, &y, &q.pt[0]);
    vec3Add(&q.pt[0], &z, &q.pt[0]);
    vec3Sub(&ballForConvexHull->position, &y, &q.pt[1]);
    vec3Sub(&q.pt[1], &z, &q.pt[1]);
    vec3Add(&ballForConvexHull->position, &y, &q.pt[2]);
    vec3Sub(&q.pt[2], &z, &q.pt[2]);
    vec3Add(&ballForConvexHull->position, &y, &q.pt[3]);
    vec3Add(&q.pt[3], &z, &q.pt[3]);

    vertexIndex[0] = convexHullAddVertex(hull, &q.pt[0], ballForConvexHull->index, 0);
    vertexIndex[1] = convexHullAddVertex(hull, &q.pt[1], ballForConvexHull->index, 1);
    vertexIndex[2] = convexHullAddVertex(hull, &q.pt[2], ballForConvexHull->index, 2);
    vertexIndex[3] = convexHullAddVertex(hull, &q.pt[3], ballForConvexHull->index, 3);

    if (*outmostBall) {
        if (ballForConvexHull->radius > (*outmostBall)->radius) {
            needUpdateOutmost = 1;
        }
    }
    else {
        needUpdateOutmost = 1;
    }
    if (needUpdateOutmost) {
        *outmostBall = ballForConvexHull;
        *outmostBallFirstVertexIndex = vertexIndex[0];
    }
}

static convexHull *createConvexHullForBall(bmesh *bm, int depth, bmeshBall *ball, int *needRetry) {
    convexHull *hull;
    bmeshBallIterator iterator;
    bmeshBall *child;
    bmeshBall *ballForConvexHull;
    bmeshBall *outmostBall = 0;
    int outmostBallFirstVertexIndex = 0;
    array *ballPtrArray;
    bmeshBall **ballPtr;
    int i;
    int hasVertexNotFitOnHull = 0;

    *needRetry = 0;

    ballPtrArray = arrayCreate(sizeof(bmeshBall *));
    hull = convexHullCreate();
    if (BMESH_BALL_TYPE_KEY == ball->type) {
        bmeshBall reduceBall = *ball;
        reduceBall.radius *= 0.4;
        addBallToHull(hull, &reduceBall, &outmostBall, &outmostBallFirstVertexIndex);
    }
    for (child = bmeshGetBallFirstChild(bm, ball, &iterator); child; child = bmeshGetBallNextChild(bm, ball, &iterator)) 
    {
        ballForConvexHull = bmeshFindChildBallForConvexHull(bm, ball, child);
        ballPtr = arrayNewItem(ballPtrArray);
        *ballPtr = ballForConvexHull;
        addBallToHull(hull, ballForConvexHull, &outmostBall, &outmostBallFirstVertexIndex);
    }
    if (depth > 0 && depth - 1 < BMESH_MAX_PARENT_BALL_DEPTH) {
        ballForConvexHull = bmeshFindParentBallForConvexHull(bm, ball, depth - 1, bm->parentBallStack[depth - 1]);
        ballPtr = (bmeshBall **)arrayNewItem(ballPtrArray);
        *ballPtr = ballForConvexHull;
        addBallToHull(hull, ballForConvexHull, &outmostBall, &outmostBallFirstVertexIndex);
    }
    if (outmostBall) {
        convexHullAddTodo(hull, outmostBallFirstVertexIndex + 0, outmostBallFirstVertexIndex + 1, outmostBallFirstVertexIndex + 2);
    }

    for (i = 0; i < arrayGetLength(ballPtrArray); ++i) {
        bmeshBall *ballItem = *((bmeshBall **)arrayGetItem(ballPtrArray, i));
        ballItem->flagForHull = 0;
    }

    convexHullGenerate(hull);

    convexHullUnifyNormals(hull, &ball->position);
    convexHullMergeTriangles(hull);

    for (i = 0; i < convexHullGetFaceNum(hull); ++i) {
        convexHullFace *f = (convexHullFace *)convexHullGetFace(hull, i);
        if (-1 != f->plane) {
            bmeshBall *ballItem = arrayGetItem(bm->ballArray, f->plane);
            ballItem->flagForHull = 1;
            f->vertexNum = 0;
        }
    }

    for (i = 0; i < arrayGetLength(ballPtrArray); ++i) {
        bmeshBall *ballItem = *((bmeshBall **)arrayGetItem(ballPtrArray, i));
        if (!ballItem->flagForHull) {
            hasVertexNotFitOnHull = 1;
            if (!ballItem->notFitHull) {
                *needRetry = 1;
                ballItem->notFitHull = 1;
                arrayDestroy(ballPtrArray);
                convexHullDestroy(hull);
                return 0;
            }
        }
    }

    if (hasVertexNotFitOnHull) {
        fprintf(stderr, "%s:hasVertexNotFitOnHull.\n", __FUNCTION__);
        arrayDestroy(ballPtrArray);
        convexHullDestroy(hull);
        return 0;
    }

    for (i = 0; i < arrayGetLength(ballPtrArray); ++i) {
        bmeshBall *ballItem = *((bmeshBall **)arrayGetItem(ballPtrArray, i));
        ballItem->convexHullCount++;
    }

    arrayDestroy(ballPtrArray);
    return hull;
}

static void bmeshStichFrom(bmesh *bm, int depth, bmeshBall *ball) {
    bmeshBallIterator iterator;
    bmeshBall *child;

    if (bm->roundColor == ball->roundColor) {
        return;
    }

    if (depth < BMESH_MAX_PARENT_BALL_DEPTH) {
        bm->parentBallStack[depth] = ball;
    }
    ball->roundColor = bm->roundColor;
    if ((BMESH_BALL_TYPE_ROOT == ball->type || BMESH_BALL_TYPE_KEY == ball->type) &&
        bmeshGetBallFirstChild(bm, ball, &iterator)) 
    {
        convexHull *hull = 0;

        for (;;) {
            int needRetry = 0;
            hull = createConvexHullForBall(bm, depth, ball, &needRetry);
            if (hull) {
                break;
            }
            if (!needRetry) {
                break;
            }
        }

        if (hull) {
            int i;
            for (i = 0; i < convexHullGetFaceNum(hull); ++i) {
                convexHullFace *face = (convexHullFace *)convexHullGetFace(hull, i);
                if (4 == face->vertexNum) 
                {
                    quad q;
                    q.pt[0] = convexHullGetVertex(hull, face->u.q.indices[0])->pt;
                    q.pt[1] = convexHullGetVertex(hull, face->u.q.indices[1])->pt;
                    q.pt[2] = convexHullGetVertex(hull, face->u.q.indices[2])->pt;
                    q.pt[3] = convexHullGetVertex(hull, face->u.q.indices[3])->pt;
                    bmeshAddQuadToModel(bm, &q);
                }
                else if (3 == face->vertexNum) 
                {
                    triangle t;
                    t.pt[0] = convexHullGetVertex(hull, face->u.t.indices[0])->pt;
                    t.pt[1] = convexHullGetVertex(hull, face->u.t.indices[1])->pt;
                    t.pt[2] = convexHullGetVertex(hull, face->u.t.indices[2])->pt;
                    bmeshAddTriangleToModel(bm, &t);
                }
            }
            convexHullDestroy(hull);
        }
    }

    for (child = bmeshGetBallFirstChild(bm, ball, &iterator); child; child = bmeshGetBallNextChild(bm, ball, &iterator)) 
    {
        bmeshStichFrom(bm, depth + 1, child);
    }
}

int bmeshStitch(bmesh *bm) {
    bm->roundColor++;
    memset(bm->parentBallStack, 0, sizeof(bm->parentBallStack));
    bmeshStichFrom(bm, 0, bmeshGetRootBall(bm));
    return 0;
}

static void rollQuadVertexs(quad *q) {
    int i;
    quad oldQuad = *q;
    for (i = 0; i < 4; ++i) {
        q->pt[i] = oldQuad.pt[(i + 1) % 4];
    }
}

static void matchTwoFaces(quad *q1, quad *q2) {
    int i;
    float minDistance = 9999;
    int matchTo = 0;
    for (i = 0; i < 4; ++i) {
        float distance = vec3Distance(&q1->pt[0], &q2->pt[i]);
        if (distance < minDistance) {
            minDistance = distance;
            matchTo = i;
        }
    }
    for (i = 0; i < matchTo; ++i) {
        rollQuadVertexs(q2);
    }
}

void calculateBallQuad(bmeshBall *ball, quad *q) {
    vec3 z, y;
    vec3Scale(&ball->localYaxis, ball->radius, &y);
    vec3Scale(&ball->localZaxis, ball->radius, &z);
    vec3Sub(&ball->position, &y, &q->pt[0]);
    vec3Add(&q->pt[0], &z, &q->pt[0]);
    vec3Sub(&ball->position, &y, &q->pt[1]);
    vec3Sub(&q->pt[1], &z, &q->pt[1]);
    vec3Add(&ball->position, &y, &q->pt[2]);
    vec3Sub(&q->pt[2], &z, &q->pt[2]);
    vec3Add(&ball->position, &y, &q->pt[3]);
    vec3Add(&q->pt[3], &z, &q->pt[3]);
}

static int bmeshAddWallsBetweenQuadsToModel(bmesh *bm, vec3 *origin, quad *q1,
    quad *q2) {
    int i;
    int result = 0;
    vec3 normal;
    vec3 o2v;
    matchTwoFaces(q1, q2);
    for (i = 0; i < 4; ++i) {
        quad wall;
        wall.pt[0] = q1->pt[i];
        wall.pt[1] = q2->pt[i];
        wall.pt[2] = q2->pt[(i + 1) % 4];
        wall.pt[3] = q1->pt[(i + 1) % 4];
        vec3Normal(&wall.pt[0], &wall.pt[1], &wall.pt[2], &normal);
        vec3Sub(&wall.pt[0], origin, &o2v);
        if (vec3DotProduct(&o2v, &normal) < 0) {
            int j;
            quad oldWall = wall;
            for (j = 0; j < 4; ++j) {
                wall.pt[j] = oldWall.pt[3 - j];
            }
        }
        bmeshAddQuadToModel(bm, &wall);
    }
    return result;
}

static bmeshBall *bmeshFindChildBallForInbetweenMesh(bmesh *bm, bmeshBall *ball) {
    bmeshBallIterator iterator;
    bmeshBall *child;
    if (ball->convexHullCount > 0) {
        return ball;
    }
    child = bmeshGetBallFirstChild(bm, ball, &iterator);
    if (!child) {
        return ball;
    }
    return bmeshFindChildBallForInbetweenMesh(bm, child);
}

static void bmeshGenerateInbetweenMeshFrom(bmesh *bm, int depth, bmeshBall *ball) 
{
    bmeshBallIterator iterator;
    bmeshBall *child = 0;
    bmeshBall *parent;
    quad currentFace, childFace;

    if (bm->roundColor == ball->roundColor) {
        return;
    }
    if (depth < BMESH_MAX_PARENT_BALL_DEPTH) {
        bm->parentBallStack[depth] = ball;
    }
    ball->roundColor = bm->roundColor;
    calculateBallQuad(ball, &currentFace);
    if (BMESH_BALL_TYPE_KEY == ball->type && !bmeshGetBallFirstChild(bm, ball, &iterator)) 
    {
        if (depth - 1 >= 0) {
            bmeshBall fakeBall;
            parent = bm->parentBallStack[depth - 1];
            fakeBall = *ball;

            if (parent == 0x0)
                fakeBall.position = ball->position;
            else
                vec3Lerp(&parent->position, &ball->position, BMESH_INTVAL_DIST_DIV, &fakeBall.position);
            calculateBallQuad(&fakeBall, &childFace);
            bmeshAddWallsBetweenQuadsToModel(bm, &ball->position, &currentFace, &childFace);
            bmeshAddQuadToModel(bm, &childFace);
            //drawQuad(&childFace);
        }
    }
    else if (1 == ball->convexHullCount && !ball->meshGenerated) 
    {
        child = bmeshGetBallFirstChild(bm, ball, &iterator);
        if (child && !child->meshGenerated) {
            //if (vec3Distance(&ball->position, &child->position) <=
            //    (ball->radius + child->radius) / 10) {
              // TODO: merge two face
            //} else {
            child = bmeshFindChildBallForInbetweenMesh(bm, child);
            calculateBallQuad(child, &childFace);
            bmeshAddWallsBetweenQuadsToModel(bm, &ball->position, &currentFace, &childFace);
            //}
            ball->meshGenerated = 1;
            child->meshGenerated = 1;
        }
    }
    for (child = bmeshGetBallFirstChild(bm, ball, &iterator); child; child = bmeshGetBallNextChild(bm, ball, &iterator)) 
    {
        bmeshGenerateInbetweenMeshFrom(bm, depth + 1, child);
    }
}

int bmeshGenerateInbetweenMesh(bmesh *bm) {
    bm->roundColor++;
    memset(bm->parentBallStack, 0, sizeof(bm->parentBallStack));
    bmeshGenerateInbetweenMeshFrom(bm, 0, bmeshGetRootBall(bm));
    return 0;
}

int bmeshGenerate(bmesh *bm, int level) {
    bmeshGenerateInbetweenBalls(bm);
    bmeshSweep(bm);
    bmeshStitch(bm);
    bmeshGenerateInbetweenMesh(bm);
    subdivCalculteNorms(bm->model);
    bm->subdivModel = subdivCatmullClarkWithLoops(bm->model, level);
    return 0;
}

int bmeshDraw(bmesh *bm) {
    //WTF dude glPushMatrix();
    //WTF dude glTranslatef(-7, 0, 0);
    //WTF dude subdivDrawModel(bm->model);
    //WTF dude glPopMatrix();
    //WTF dude subdivDrawModel(bm->subdivModel);
    return 0;
}

subdivModel* bmeshSubdivModel(bmesh* bm)
{
    return bm->subdivModel;
}