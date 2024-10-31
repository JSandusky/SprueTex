#ifndef DRAW_SPHERE_H
#define DRAW_SPHERE_H
#include <SprueEngine/Libs/BMesh/3dstruct.h>
#ifdef __APPLE__
#include <OpenGL/glu.h>
#elif defined(_WIN32)
#include <windows.h>
//#include <GL/glu.h>
#endif

#ifdef __cplusplus
extern "C" {
#endif

#ifndef M_PI
#define M_PI 3.14159265
#endif

int drawInit(void);
int drawSphere(vec3 *origin, float radius, int slices, int stacks);
void drawTriangle(triangle *poly);
void drawQuad(quad *poly);
int drawCylinder(vec3 *topOrigin, vec3 *bottomOrigin, float radius, int slices,
    int stacks);
int drawGrid(float size, float step);
int drawText(int x, int y, char *text);
int drawPrintf(int x, int y, const char *fmt, ...);
int drawDebugPrintf(const char *fmt, ...);
void drawDebugPoint(vec3 *origin, int colorIndex);
void drawTestUnproject(void);

#ifdef __cplusplus
}
#endif

#endif
