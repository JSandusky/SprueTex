
#include "SprueEngine/Core/Context.h"
#include "SprueEngine/Core/DensityFunctions.h"
#include "SprueEngine/Deserializer.h"
#include "SprueEngine/IDebugRender.h"
#include "SprueEngine/Math/Mat2x2.h"
#include "SprueEngine/Math/MathDef.h"
#include "SprueEngine/Core/SceneObject.h" // For SceneObjectDebugLine
#include "SprueEngine/Serializer.h"
#include "SprueEngine/Math/Trig.h"
#include "SprueEngine/MathGeoLib/AllMath.h"

#define length(ARG) ARG .Length()
#define dot(ARG1, ARG2) ARG1.Dot(ARG2)

#define POINT_ON_SPHERE(sphere, theta, phi) Vec3( \
        sphere.pos.x + sphere.r * sinf((float)(theta)  * DEG_TO_RAD) * sinf((float)(phi) * DEG_TO_RAD), \
        sphere.pos.y + sphere.r * cosf((float)(phi) * DEG_TO_RAD), \
        sphere.pos.z + sphere.r * cosf((float)(theta) * DEG_TO_RAD) * sinf((float)(phi) * DEG_TO_RAD))

// From http://iquilezles.org/www/articles/distfunctions/distfunctions.htm
namespace SprueEngine
{
    float SphereDensity(const Vec3& pos, const ShapeParams& params)
    {
        return pos.Length() - params.r;
    }

    float CubeDensity(const Vec3& pos, const ShapeParams& params)
    {
        Vec3 d = pos.Abs() - (Vec3(&params.x) / 2.0f);
        return SprueMin(d.MaxElement(), 0.0f) + Vec3::MaxVector(d, Vec3::zero).Length();
    }

    float RoundedBoxDensity(const Vec3& p, const ShapeParams& params)
    {
        Vec3 d = p.Abs() - Vec3(&params.x);
        return SprueMin(d.MaxElement(), 0.0f) + Vec3::MaxVector(d, Vec3::zero).Length() - params.r;
    }

    float CapsuleDensity(const Vec3& pos, const ShapeParams& params)
    {
        Vec3 a(0.0f, 0.0f, params.x / 2.0f);
        Vec3 b(0.0f, 0.0f, -a.z);
        Vec3 nearest = ClosestPoint(a, b, pos);
        return (pos - nearest).Length() - params.r;
    }

    float CylinderDensity(const Vec3& p, const ShapeParams& params)
    {
        float d = length(Vec2(p.x, p.z)) - params.r;
        return SprueMax(d, abs(p.y) - params.x);
    }

    float ConeDensity(const Vec3& p, const ShapeParams& params)
    {     
        const float c = params.x;// / 2.0f;
        const float& r = params.r;
        float d = length(Vec2(p.x, p.z)) - r * (1.0f - (c + p.y) / (c + c));
        d = SprueMax(d, -p.y - c);
        d = SprueMax(d, p.y - c);
        return d;

        //const float radius = params.r;
        //const float height = params.x;
        //const vec2 q = vec2(length(vec2(p.x, p.z)), p.y);
        //const vec2 tip = q - vec2(0, height);
        //const vec2 mantleDir = vec2(height, radius).Normalized();
        //const float mantle = dot(tip, mantleDir);
        //float d = SprueMax(mantle, -q.y);
        //const float projected = dot(tip, vec2(mantleDir.y, -mantleDir.x));
        //
        //if ((q.y > height) && (projected < 0))
        //    d = SprueMax(d, length(tip));
        //
        //if ((q.x > radius) && (projected > length(vec2(height, radius))))
        //    d = SprueMax(d, length((q - vec2(radius, 0))));
        //return d;
    }

    float CappedConeDensity(const Vec3& p, const ShapeParams& params)
    {
        Vec3 c(&params.r);
        Vec2 q = Vec2(Vec2(p.x, p.z).Length(), p.y);
        Vec2 v = Vec2(c.z * c.y / c.x, -c.z);
        Vec2 w = v - q;
        Vec2 vv = Vec2(v.Dot(v), v.x*v.x);
        Vec2 qv = Vec2(v.Dot(w), v.x*w.x);
        Vec2 d = SprueMax(qv, Vec2(0.0f, 0.0f)) * qv / vv;
        return sqrtf(w.Dot(w) - SprueMax(d.x, d.y)) * sgn(SprueMax(q.y *v.x - q.x*v.y, w.y));
    }

    float PlaneDistance(const Vec3& p, const ShapeParams& params)
    {
        // n must be normalized
        return p.Dot(Vec3(0, 1, 0));// + params.r;
    }

    float EllipsoidDistance(const Vec3& p, const ShapeParams& params)
    {
        // At 1.0f we're on the surface of the ellipsoid, if < than inside it, if > then outside it
        Vec3 r(&params.x);
        return ((p / r).Length() - 1.0f) * SprueMin(params.x, SprueMin(params.y, params.z));
    }

    float TorusDensity(const Vec3& pos, const ShapeParams& params)
    {
        Vec2 q = Vec2(Vec2(pos.x, pos.z).Length() - params.x, pos.y);
        return q.Length() - params.y;
    }

    Vec3 Twist(const Vec3& p, float amount)
    {
        float c = cos(amount*p.y);
        float s = sin(amount*p.y);
        Mat2x2  m = Mat2x2(c, -s, s, c);
        Vec2 v = m * Vec2(p.x, p.y);
        return Vec3(v.x, v.y, p.y);
    }

    Vec3 CheapBend(const Vec3& p, float amount)
    {
        float c = cos(amount * p.y);
        float s = sin(amount * p.y);
        Mat2x2  m = Mat2x2(c, -s, s, c);
        Vec2 v = m * Vec2(p.x, p.y);
        return Vec3(v.x, v.y, p.z);
    }

    float SuperEllipseDensity(const Vec3& pos, const ShapeParams& params)
    {
        float yaw = atanf(pos.x / -pos.y);
        float pitch = atanf(sqrtf(pos.x * pos.x + pos.y * pos.y) / pos.z);

        float tmp;
        float ct1, ct2, st1, st2;

        ct1 = cosf(yaw);
        ct2 = cosf(pitch);
        st1 = sinf(yaw);
        st2 = sinf(pitch);

        tmp = sgn(ct1) * powf(fabs(ct1), params.x);
        Vec3 newPos;
        newPos.x = tmp * sgn(ct2) * powf(fabs(ct2), params.y);
        newPos.y = sgn(st1) * powf(fabs(st1), params.x);
        newPos.z = tmp * sgn(st2) * powf(fabs(st2), params.y);

        return pos.Length() - newPos.Length();

        //float result;
        //result = powf(powf(pos.x / params.x, 2 / params.r) +
        //    powf(pos.y / params.y, 2 / params.r), params.r / params.alpha) +
        //    powf(pos.z / params.z, 2 / params.alpha);
        //return pos.Length() - result;
    }

    float Displace(float lhsDistance, float rhsDistance, float displacementPower)
    {
        return lhsDistance + rhsDistance * displacementPower;
    }
    
    float Blend(float lhsDistance, float rhsDistance, float blendPower)
    {
        return SprueLerp(lhsDistance, polySmoothMin(lhsDistance, rhsDistance), blendPower);
    }

    void RegisterDensityFunctions(Context* context)
    {
        SphereFunction::Register(context);
        CubeFunction::Register(context);
        RoundedBoxFunction::Register(context);
        CapsuleFunction::Register(context);
        CylinderFunction::Register(context);
        ConeFunction::Register(context);
        CappedConeFunction::Register(context);
        PlaneFunction::Register(context);
        EllipsoidFunction::Register(context);
        TorusFunction::Register(context);
        SuperEllipseFunction::Register(context);
    }

    void DensityHandler::DrawDebug(IDebugRender* renderer, const Mat3x4& transform, const RGBA& color)
    {
#define SLICES 32
#define STACKS 32
        std::vector<Vec3> points;
        const Mat3x4 inverse = transform.Inverted();
        points.reserve((SLICES + 1) * (STACKS + 1));
        Vec2 uv;
        Vec3 xyz;
        float* pointData = (float*)points.data();
        for (int stack = 0; stack < STACKS + 1; stack++) {
            uv.x = (float)stack / STACKS;
            for (int slice = 0; slice < SLICES+ 1; slice++) {
                uv.y = (float)slice / SLICES;
                CalculateDensity(xyz);
                *pointData++ = xyz.x;
                *pointData++ = xyz.y;
                *pointData++ = xyz.z;
            }
        }


    }

// While it would be possible to just bind the entire params structure and call it a day for serialization, these properties are necessary for editor GUI
#define SHAPE_ADDRESS(TYPE, PARAM) (offsetof(TYPE, params) + offsetof(ShapeParams, PARAM))

//==========================================
//  Sphere
//==========================================
    void SphereFunction::Register(Context* context)
    {
        context->RegisterFactory<SphereFunction>("SphereFunction", "Perfect sphere shape");
        REGISTER_PROPERTY_MEMORY(SphereFunction, float, SHAPE_ADDRESS(SphereFunction, r), 1.0f, "Radius", "", PS_VisualConsequence | PS_Permutable);
    }

    std::shared_ptr<MeshData> SphereFunction::GeneratePreviewMesh()
    {
        return 0x0;
    }

    std::string SphereFunction::ToString() const
    {
        return "SphereDensity(pos, shapeData, transformData, &paramIndex, &transformIndex)";
    }

    void SphereFunction::WriteParameters(Serializer* dst)
    {
        dst->WriteFloat(params.r);
    }

    void SphereFunction::DrawDebug(IDebugRender* renderer, const Mat3x4& transform, const RGBA& color)
    {
        Sphere sphere;
        sphere.pos = Vec3(0, 0, 0);
        sphere.r = getRadius();
        for (float j = 0; j < 180; j += 22.5f)
        {
            for (float i = 0; i < 360; i += 45.0f)
            {
                Vec3 p1 = POINT_ON_SPHERE(sphere, i, j);
                Vec3 p2 = POINT_ON_SPHERE(sphere, i + 45.0f, j);
                Vec3 p3 = POINT_ON_SPHERE(sphere, i, j + 22.5f);
                Vec3 p4 = POINT_ON_SPHERE(sphere, i + 45.0f, j + 22.5f);

                renderer->DrawLine(transform * p1, transform * p2, color);
                renderer->DrawLine(transform * p3, transform * p4, color);
                renderer->DrawLine(transform * p1, transform * p3, color);
                renderer->DrawLine(transform * p2, transform * p4, color);
            }
        }
        //renderer->DrawSphere(transform.GetPosition(), getRadius(), color);
    }

//==========================================
//  Cube
//==========================================
    void CubeFunction::Register(Context* context)
    {
        context->RegisterFactory<CubeFunction>("CubeFunction", "3d box shape");
        REGISTER_PROPERTY_MEMORY(CubeFunction, Vec3, SHAPE_ADDRESS(CubeFunction, x), Vec3(1,1,1), "Size", "", PS_VisualConsequence | PS_Permutable);
    }

    std::shared_ptr<MeshData> CubeFunction::GeneratePreviewMesh()
    {
        return 0x0;
    }

    std::string CubeFunction::ToString() const
    {
        return "BoxDensity(pos, shapeData, transformData, &paramIndex, &transformIndex)";
    }

    void CubeFunction::WriteParameters(Serializer* dst)
    {
        dst->WriteFloat(params.x);
        dst->WriteFloat(params.y);
        dst->WriteFloat(params.z);
    }

    void CubeFunction::DrawDebug(IDebugRender* renderer, const Mat3x4& transform, const RGBA& color)
    {
        BoundingBox bounds;
        bounds.minPoint = Vec3(-params.x / 2.0f, -params.y / 2.0f, -params.z / 2.0f);
        bounds.maxPoint = Vec3(params.x / 2.0f, params.y / 2.0f, params.z / 2.0f);
        renderer->DrawBoundingBox(transform, bounds, color);
    }

//==========================================
//  Rounded Box
//==========================================
    void RoundedBoxFunction::Register(Context* context)
    {
        context->RegisterFactory<RoundedBoxFunction>("RoundedBoxFunction", "Box with rounded edges");
        REGISTER_PROPERTY_MEMORY(RoundedBoxFunction, Vec3, SHAPE_ADDRESS(RoundedBoxFunction, x), Vec3(1, 1, 1), "Size", "", PS_VisualConsequence | PS_Permutable);
        REGISTER_PROPERTY_MEMORY(RoundedBoxFunction, float, SHAPE_ADDRESS(RoundedBoxFunction, r), 0.0f, "Rounding", "", PS_VisualConsequence | PS_Permutable);
    }

    std::shared_ptr<MeshData> RoundedBoxFunction::GeneratePreviewMesh()
    {
        return 0x0;
    }

    std::string RoundedBoxFunction::ToString() const
    {
        return "RoundedBoxDensity(pos, shapeData, transformData, &paramIndex, &transformIndex)";
    }

    void RoundedBoxFunction::WriteParameters(Serializer* dst)
    {
        dst->WriteFloat(params.x);
        dst->WriteFloat(params.y);
        dst->WriteFloat(params.z);
        dst->WriteFloat(params.r);
    }

    void RoundedBoxFunction::DrawDebug(IDebugRender* renderer, const Mat3x4& transform, const RGBA& color)
    {
        BoundingBox bounds;
        bounds.minPoint = Vec3(-params.x / 2.0f - params.r, -params.y / 2.0f - params.r, -params.z / 2.0f - params.r);
        bounds.maxPoint = Vec3(params.x / 2.0f + params.r, params.y / 2.0f + params.r, params.z / 2.0f + params.r);
        renderer->DrawBoundingBox(transform, bounds, color);
    }

//==========================================
//  Capsule
//==========================================
    void CapsuleFunction::Register(Context* context)
    {
        context->RegisterFactory<CapsuleFunction>("CapsuleFunction", "Pill-shaped capsule");
        REGISTER_PROPERTY_MEMORY(CapsuleFunction, float, SHAPE_ADDRESS(CapsuleFunction, r), 0.5f, "Radius", "", PS_VisualConsequence | PS_Permutable);
        REGISTER_PROPERTY_MEMORY(CapsuleFunction, float, SHAPE_ADDRESS(CapsuleFunction, x), 1.0f, "Height", "", PS_VisualConsequence | PS_Permutable);
    }

    std::shared_ptr<MeshData> CapsuleFunction::GeneratePreviewMesh()
    {
        return 0x0;
    }

    std::string CapsuleFunction::ToString() const
    {
        return "CapsuleDensity(pos, shapeData, transformData, &paramIndex, &transformIndex)";
    }

    void CapsuleFunction::WriteParameters(Serializer* dst)
    {
        dst->WriteFloat(params.x);
        dst->WriteFloat(params.r);
    }

    void CapsuleFunction::DrawDebug(IDebugRender* renderer, const Mat3x4& transform, const RGBA& color)
    {
        int stepDegrees = 30;

        Vec3 capStart(0.f, 0.f, 0.f);
        const float halfHeight = params.x / 2.0f;
        capStart.z = -halfHeight;

        Vec3 capEnd(0.f, 0.f, 0.f);
        capEnd.z = halfHeight;

        // Draw the ends
        {
            Vec3 center = capStart;
            Vec3 up = Vec3(1, 0, 0);
            Vec3 axis = Vec3(0, 0, -1);
            float minTh = -HALFPI;
            float maxTh =  HALFPI;
            float minPs = -HALFPI;
            float maxPs =  HALFPI;
        
            renderer->DrawSpherePatch(transform, capStart, up, axis, params.r, minTh, maxTh, minPs, maxPs, color);
        }
        
        {
            Vec3 up = Vec3(-1, 0, 0);
            Vec3 axis = Vec3(0, 0, 1);
            float minTh = -HALFPI;
            float maxTh =  HALFPI;
            float minPs = -HALFPI;
            float maxPs =  HALFPI;
            renderer->DrawSpherePatch(transform, capEnd, up, axis, params.r, minTh, maxTh, minPs, maxPs, color);
        }

        // Draw lines for the tube
        Vec3 start = transform.TranslatePart();
        for (int i = 0; i<360; i += stepDegrees)
        {
            capEnd.y = capStart.y = sinf(float(i) * DEG_TO_RAD) * params.r;
            capEnd.x = capStart.x = cosf(float(i) * DEG_TO_RAD) * params.r;
            renderer->DrawLine(transform * capStart, transform * capEnd, color);
        }
    }

//==========================================
//  Cylinder
//==========================================
    void CylinderFunction::Register(Context* context)
    {
        context->RegisterFactory<CylinderFunction>("CylinderFunction", "Simple tube");
        REGISTER_PROPERTY_MEMORY(CylinderFunction, float, SHAPE_ADDRESS(CylinderFunction, r), 1.0f, "Radius", "", PS_VisualConsequence | PS_Permutable);
        REGISTER_PROPERTY_MEMORY(CylinderFunction, float, SHAPE_ADDRESS(CylinderFunction, x), 1.0f, "Height", "", PS_VisualConsequence | PS_Permutable);
        //REGISTER_PROPERTY_MEMORY(CylinderFunction, Vec3, SHAPE_ADDRESS(CylinderFunction, x), Vec3(1,1,1), "Size", "", PS_Default);
    }

    std::shared_ptr<MeshData> CylinderFunction::GeneratePreviewMesh()
    {
        return 0x0;
    }

    std::string CylinderFunction::ToString() const
    {
        return "CylinderDensity(pos, shapeData, transformData, &paramIndex, &transformIndex)";
    }

    void CylinderFunction::WriteParameters(Serializer* dst)
    {
        dst->WriteFloat(params.r);
        dst->WriteFloat(params.x);
    }

    void CylinderFunction::DrawDebug(IDebugRender* renderer, const Mat3x4& transform, const RGBA& color)
    {
        Sphere sphere;
        sphere.r = params.r;
        Vec3 heightVec(0, params.x, 0);
        Vec3 offsetXVec(params.r, 0, 0);
        Vec3 offsetZVec(0, 0, params.r);
        for (float i = 0; i < 360; i += 22.5f)
        {
            Vec3 p1 = POINT_ON_SPHERE(sphere, i, 90);
            Vec3 p2 = POINT_ON_SPHERE(sphere, i + 22.5f, 90);
            renderer->DrawLine(transform * (p1 - heightVec), transform * (p2 - heightVec), color);
            renderer->DrawLine(transform * (p1 + heightVec), transform * (p2 + heightVec), color);
        }
        renderer->DrawLine(transform * (-heightVec + offsetXVec), transform * (heightVec + offsetXVec), color);
        renderer->DrawLine(transform * (-heightVec + -offsetXVec), transform * (heightVec - offsetXVec), color);
        renderer->DrawLine(transform * (-heightVec + offsetZVec), transform * (heightVec + offsetZVec), color);
        renderer->DrawLine(transform * (-heightVec + -offsetZVec), transform * (heightVec - offsetZVec), color);
    }

//==========================================
//  Cone
//==========================================
    void ConeFunction::Register(Context* context)
    {
        context->RegisterFactory<ConeFunction>("ConeFunction", "Rounded cone, requires high resolution to capture the shape correctly");
        //REGISTER_PROPERTY_MEMORY(ConeFunction, Vec3, SHAPE_ADDRESS(ConeFunction, x), Vec3(1, 1, 1), "Size", "", PS_Default);
        REGISTER_PROPERTY_MEMORY(ConeFunction, float, SHAPE_ADDRESS(ConeFunction, r), 1.0f, "Radius", "", PS_VisualConsequence | PS_Permutable);
        REGISTER_PROPERTY_MEMORY(ConeFunction, float, SHAPE_ADDRESS(ConeFunction, x), 1.0f, "Height", "", PS_VisualConsequence | PS_Permutable);
    }

    std::shared_ptr<MeshData> ConeFunction::GeneratePreviewMesh()
    {
        return 0x0;
    }

    std::string ConeFunction::ToString() const
    {
        return "ConeDensity(pos, shapeData, transformData, &paramIndex, &transformIndex)";
    }

    void ConeFunction::WriteParameters(Serializer* dst)
    {
        dst->WriteFloat(params.x);
        dst->WriteFloat(params.r);
    }

    void ConeFunction::DrawDebug(IDebugRender* renderer, const Mat3x4& transform, const RGBA& color)
    {
        Sphere ring;
        ring.r = params.r;

        Vec3 heightVec(0, params.x, 0);
        Vec3 offsetXVec(params.r, 0, 0);
        Vec3 offsetZVec(0, 0, params.r);

        for (float i = 0; i < 360; i += 22.5f)
        {
            Vec3 pt1 = POINT_ON_SPHERE(ring, i, 90);
            Vec3 pt2 = POINT_ON_SPHERE(ring, i + 22.5f, 90);

            renderer->DrawLine(transform * (pt1 - heightVec), transform * (pt2 - heightVec), color);
        }

        renderer->DrawLine(transform * (-heightVec + offsetXVec), transform *  (heightVec), color);
        renderer->DrawLine(transform * (-heightVec + -offsetXVec), transform * (heightVec), color);
        renderer->DrawLine(transform * (-heightVec + offsetZVec), transform *  (heightVec), color);
        renderer->DrawLine(transform * (-heightVec + -offsetZVec), transform * (heightVec), color);
    }

//==========================================
//  Capped Cone
//==========================================
    void CappedConeFunction::Register(Context* context)
    {
        context->RegisterFactory<CappedConeFunction>("CappedConeFunction", "Cone with the tip cut off into a flat plane");
        REGISTER_PROPERTY_MEMORY(CappedConeFunction, float, SHAPE_ADDRESS(CappedConeFunction, r), 1.0f, "Radius", "", PS_VisualConsequence | PS_Permutable);
    }

    std::shared_ptr<MeshData> CappedConeFunction::GeneratePreviewMesh()
    {
        return 0x0;
    }

    std::string CappedConeFunction::ToString() const
    {
        return "CappedConeDensity(pos, shapeData, transformData, &paramIndex, &transformIndex)";
    }

    void CappedConeFunction::WriteParameters(Serializer* dst)
    {
        dst->WriteFloat(params.r);
    }

//==========================================
//  Plane
//==========================================
    void PlaneFunction::Register(Context* context)
    {
        // Using Plane here because we can present a more natural gui for it
        context->RegisterFactory<PlaneFunction>("PlaneFunction", "Constant plane that is infinite in dimension");
        // Has no properties
        //REGISTER_PROPERTY_MEMORY(PlaneFunction, Plane, SHAPE_ADDRESS(PlaneFunction, r), Plane(Vec3::zero, Vec3::PositiveY), "Plane", "", PS_VisualConsequence);
    }

    std::shared_ptr<MeshData> PlaneFunction::GeneratePreviewMesh()
    {
        return 0x0;
    }

    std::string PlaneFunction::ToString() const
    {
        return "PlaneDensity(pos, shapeData, transformData, &paramIndex, &transformIndex)";
    }

    void PlaneFunction::WriteParameters(Serializer* dst)
    {
        dst->WriteVector4(Vec4(0, 1, 0, 0));
        //dst->WriteFloat(params.r);
    }

    void PlaneFunction::DrawDebug(IDebugRender* renderer, const Mat3x4& transform, const RGBA& color)
    {
        Vec3 topLeft(-10, 0, -10);
        Vec3 topRight(10, 0, -10);
        
        Vec3 bottomLeft(-10, 0, 10);
        Vec3 bottomRight(10, 0, 10);

        // Quad edges
        renderer->DrawLine(transform * topLeft, transform * topRight, color);
        renderer->DrawLine(transform * topLeft, transform * bottomLeft, color);
        renderer->DrawLine(transform * bottomLeft,  transform * bottomRight, color);
        renderer->DrawLine(transform * bottomRight, transform * topRight, color);

        // Cross
        renderer->DrawLine(transform * topLeft, transform * bottomRight, color);
        renderer->DrawLine(transform * bottomLeft, transform * topRight, color);
    }

//==========================================
//  Ellipsoid
//==========================================
    void EllipsoidFunction::Register(Context* context)
    {
        context->RegisterFactory<EllipsoidFunction>("EllipsoidFunction", "3d elliptical shape");
        REGISTER_PROPERTY_MEMORY(EllipsoidFunction, Vec3, SHAPE_ADDRESS(EllipsoidFunction, x), Vec3(1, 1, 1), "Size", "", PS_VisualConsequence | PS_Permutable);
    }

    std::shared_ptr<MeshData> EllipsoidFunction::GeneratePreviewMesh()
    {
        return 0x0;
    }

    std::string EllipsoidFunction::ToString() const
    {
        return "EllipsoidDensity(pos, shapeData, transformData, &paramIndex, &transformIndex)";
    }

    void EllipsoidFunction::WriteParameters(Serializer* dst)
    {
        dst->WriteFloat(params.x);
        dst->WriteFloat(params.y);
        dst->WriteFloat(params.z);
    }

#define POINT_ON_ELLIPSOID(EX, EY, EZ, THETA, PHI) Vec3( \
        EX * sinf((float)(THETA)  * DEG_TO_RAD) * sinf((float)(PHI) * DEG_TO_RAD), \
        EY * cosf((float)(PHI) * DEG_TO_RAD), \
        EZ * cosf((float)(THETA) * DEG_TO_RAD) * sinf((float)(PHI) * DEG_TO_RAD))

        //EX * (sinf(THETA * DEG_TO_RAD) * sinf(PHI * DEG_TO_RAD)), \
        //EY * (cosf(THETA * DEG_TO_RAD) * cosf(PHI * DEG_TO_RAD)), \
        //EZ * (sinf(THETA * DEG_TO_RAD)))

    void EllipsoidFunction::DrawDebug(IDebugRender* renderer, const Mat3x4& transform, const RGBA& color)
    {
        for (float j = 0; j < 180; j += 22.5f)
        {
            for (float i = 0; i < 360; i += 22.5f)
            {
                Vec3 p1 = POINT_ON_ELLIPSOID(params.x, params.y, params.z, i, j);
                Vec3 p2 = POINT_ON_ELLIPSOID(params.x, params.y, params.z, i + 22.5f, j);
                Vec3 p3 = POINT_ON_ELLIPSOID(params.x, params.y, params.z, i, j + 22.5f);
                Vec3 p4 = POINT_ON_ELLIPSOID(params.x, params.y, params.z, i + 22.5f, j + 22.5f);

                renderer->DrawLine(transform * p1, transform * p2, color);
                renderer->DrawLine(transform * p3, transform * p4, color);
                renderer->DrawLine(transform * p1, transform * p3, color);
                renderer->DrawLine(transform * p2, transform * p4, color);
            }
        }
    }

//==========================================
//  Torus Function
//==========================================
    void TorusFunction::Register(Context* context)
    {
        context->RegisterFactory<TorusFunction>("TorusFunction", "Donut shape");
        REGISTER_PROPERTY_MEMORY(TorusFunction, float, SHAPE_ADDRESS(TorusFunction, x), 1.0f, "Inner Radius", "", PS_VisualConsequence | PS_Permutable);
        REGISTER_PROPERTY_MEMORY(TorusFunction, float, SHAPE_ADDRESS(TorusFunction, y), 0.5f, "Outer Radius", "", PS_VisualConsequence | PS_Permutable);
    }

    std::shared_ptr<MeshData> TorusFunction::GeneratePreviewMesh()
    {
        return 0x0;
    }

    std::string TorusFunction::ToString() const
    {
        return "TorusDensity(pos, shapeData, transformData, &paramIndex, &transformIndex)";
    }

    void TorusFunction::WriteParameters(Serializer* dst)
    {
        dst->WriteFloat(params.x);
        dst->WriteFloat(params.y);
    }

    void TorusFunction::DrawDebug(IDebugRender* renderer, const Mat3x4& transform, const RGBA& color)
    {
        // http://paulbourke.net/geometry/torus/
        int u, v, du = 30, dv = 30;
        double r0 = params.x, r1 = params.y;
        double theta, phi;
        Vec3 p[4];

        for (u = 0; u<360; u += du) {
            for (v = 0; v<360; v += dv) {
                theta = (u)* DEG_TO_RAD;
                phi = (v)* DEG_TO_RAD;
                p[0].x = cos(theta) * (r0 + r1 * cos(phi));
                p[0].z = sin(theta) * (r0 + r1 * cos(phi));
                p[0].y = r1 * sin(phi);
                theta = (u + du) * DEG_TO_RAD;
                phi = (v)* DEG_TO_RAD;
                p[1].x = cos(theta) * (r0 + r1 * cos(phi));
                p[1].z = sin(theta) * (r0 + r1 * cos(phi));
                p[1].y = r1 * sin(phi);
                theta = (u + du) * DEG_TO_RAD;
                phi = (v + dv) * DEG_TO_RAD;
                p[2].x = cos(theta) * (r0 + r1 * cos(phi));
                p[2].z = sin(theta) * (r0 + r1 * cos(phi));
                p[2].y = r1 * sin(phi);
                theta = (u)* DEG_TO_RAD;
                phi = (v + dv) * DEG_TO_RAD;
                p[3].x = cos(theta) * (r0 + r1 * cos(phi));
                p[3].z = sin(theta) * (r0 + r1 * cos(phi));
                p[3].y = r1 * sin(phi);
                
                renderer->DrawLine(transform * p[0], transform * p[1], color);
                renderer->DrawLine(transform * p[2], transform * p[3], color);
                renderer->DrawLine(transform * p[0], transform * p[3], color);
                renderer->DrawLine(transform * p[1], transform * p[2], color);
            }
        }
    }


//==========================================
//  Super Ellipse
//==========================================
    void SuperEllipseFunction::Register(Context* context)
    {
        context->RegisterFactory<SuperEllipseFunction>("SuperEllipseFunction", "Versatile super-function that can produce many natural appearing shapes");
        REGISTER_PROPERTY_MEMORY(SuperEllipseFunction, float, SHAPE_ADDRESS(SuperEllipseFunction, alpha), 0.25f, "N", "", PS_VisualConsequence | PS_Permutable);
        REGISTER_PROPERTY_MEMORY(SuperEllipseFunction, float, SHAPE_ADDRESS(SuperEllipseFunction, r), 1.0f, "Exponent", "", PS_VisualConsequence | PS_Permutable);
        REGISTER_PROPERTY_MEMORY(SuperEllipseFunction, Vec3, SHAPE_ADDRESS(SuperEllipseFunction, x), Vec3(1, 1, 1), "Scale", "", PS_VisualConsequence | PS_Permutable);
    }

    std::shared_ptr<MeshData> SuperEllipseFunction::GeneratePreviewMesh()
    {
        return 0x0;
    }

    std::string SuperEllipseFunction::ToString() const
    {
        return "SuperEllipsoidDensity(pos, shapeData, transformData, &paramIndex, &transformIndex)";
    }

    void SuperEllipseFunction::WriteParameters(Serializer* dst)
    {
        dst->WriteFloat(params.alpha);
        dst->WriteFloat(params.r);
        dst->WriteFloat(params.x);
        dst->WriteFloat(params.y);
        dst->WriteFloat(params.z);
    }

#undef SHAPE_ADDRESS
}