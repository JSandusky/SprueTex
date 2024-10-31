#include "MakeLevelSet.h"

#include <SprueEngine/Geometry/MeshData.h>

#include "Array3.h"

using namespace SprueEngine;

namespace SprueLevelSetMethods
{
    template<typename T>
    static T tripleMin(T x, T y, T z) 
    {
        return std::min(x, std::min(y, z));
    }

    template<typename T>
    static T tripleMax(T x, T y, T z)
    {
        return std::max(x, std::max(y, z));
    }

    template<typename T>
    static T clamp(T val, T min, T max)
    {
        return std::min(std::max(val, min), max);
    }

    static float dist(const Vec3& lhs, const Vec3& rhs)
    {
        return (rhs - lhs).Length();
    }

    static float mag2(const Vec3& lhs)
    {
        return lhs.LengthSq();
    }

    static float dot(const Vec3& lhs, const Vec3& rhs)
    {
        return lhs.Dot(rhs);
    }

    // find distance x0 is from segment x1-x2
    static float point_segment_distance(const Vec3 &x0, const Vec3 &x1, const Vec3 &x2)
    {
        Vec3 dx(x2 - x1);
        double m2 = dx.LengthSq();
        // find parameter value of closest point on segment
        float s12 = (float)((x2 - x0).Dot(dx) / m2);
        if (s12<0) {
            s12 = 0;
        }
        else if (s12>1) {
            s12 = 1;
        }
        // and find the distance
        return dist(x0, s12*x1 + (1 - s12)*x2);
    }

    // find distance x0 is from triangle x1-x2-x3
    static float point_triangle_distance(const Vec3 &x0, const Vec3 &x1, const Vec3 &x2, const Vec3 &x3)
    {
        // first find barycentric coordinates of closest point on infinite plane
        Vec3 x13(x1 - x3), x23(x2 - x3), x03(x0 - x3);
        float m13 = mag2(x13), m23 = mag2(x23), d = dot(x13, x23);
        float invdet = 1.f / std::max(m13*m23 - d*d, 1e-30f);
        float a = dot(x13, x03), b = dot(x23, x03);
        // the barycentric coordinates themselves
        float w23 = invdet*(m23*a - d*b);
        float w31 = invdet*(m13*b - d*a);
        float w12 = 1 - w23 - w31;
        if (w23 >= 0 && w31 >= 0 && w12 >= 0) { // if we're inside the triangle
            return dist(x0, w23*x1 + w31*x2 + w12*x3);
        }
        else { // we have to clamp to one of the edges
            if (w23>0) // this rules out edge 2-3 for us
                return std::min(point_segment_distance(x0, x1, x2), point_segment_distance(x0, x1, x3));
            else if (w31>0) // this rules out edge 1-3
                return std::min(point_segment_distance(x0, x1, x2), point_segment_distance(x0, x2, x3));
            else // w12 must be >0, ruling out edge 1-2
                return std::min(point_segment_distance(x0, x1, x3), point_segment_distance(x0, x2, x3));
        }
    }

    static void check_neighbour(const std::vector<unsigned> &tri, const std::vector<Vec3> &x,
        FilterableBlockMap<float>* phi, Array3i &closest_tri,
        const Vec3 &gx, int i0, int j0, int k0, int i1, int j1, int k1)
    {
        if (closest_tri(i1, j1, k1) >= 0) {
            unsigned closestTri = closest_tri(i1, j1, k1);
            unsigned int p = tri[closestTri], q = tri[closestTri + 1], r = tri[closestTri + 2];
            //assign(tri[closest_tri(i1, j1, k1)], p, q, r);
            float d = point_triangle_distance(gx, x[p], x[q], x[r]);
            if (d < phi->get(i0, j0, k0)) {
                phi->set(d, i0, j0, k0);
                closest_tri(i0, j0, k0) = closest_tri(i1, j1, k1);
            }
        }
    }

    static void sweep(const std::vector<unsigned> &tri, const std::vector<Vec3> &x,
        FilterableBlockMap<float>* phi, Array3i &closest_tri, const Vec3 &origin, float dx,
        int di, int dj, int dk)
    {
        int i0, i1;
        if (di>0) { i0 = 1; i1 = phi->getWidth(); }
        else { i0 = phi->getWidth() - 2; i1 = -1; }
        int j0, j1;
        if (dj>0) { j0 = 1; j1 = phi->getHeight(); }
        else { j0 = phi->getHeight() - 2; j1 = -1; }
        int k0, k1;
        if (dk>0) { k0 = 1; k1 = phi->getDepth(); }
        else { k0 = phi->getDepth() - 2; k1 = -1; }
        for (int k = k0; k != k1; k += dk) for (int j = j0; j != j1; j += dj) for (int i = i0; i != i1; i += di) {
            Vec3 gx(i*dx + origin[0], j*dx + origin[1], k*dx + origin[2]);
            check_neighbour(tri, x, phi, closest_tri, gx, i, j, k, i - di, j, k);
            check_neighbour(tri, x, phi, closest_tri, gx, i, j, k, i, j - dj, k);
            check_neighbour(tri, x, phi, closest_tri, gx, i, j, k, i - di, j - dj, k);
            check_neighbour(tri, x, phi, closest_tri, gx, i, j, k, i, j, k - dk);
            check_neighbour(tri, x, phi, closest_tri, gx, i, j, k, i - di, j, k - dk);
            check_neighbour(tri, x, phi, closest_tri, gx, i, j, k, i, j - dj, k - dk);
            check_neighbour(tri, x, phi, closest_tri, gx, i, j, k, i - di, j - dj, k - dk);
        }
    }

    // calculate twice signed area of triangle (0,0)-(x1,y1)-(x2,y2)
    // return an SOS-determined sign (-1, +1, or 0 only if it's a truly degenerate triangle)
    static int orientation(float x1, float y1, float x2, float y2, float &twice_signed_area)
    {
        twice_signed_area = y1*x2 - x1*y2;
        if (twice_signed_area>0) return 1;
        else if (twice_signed_area<0) return -1;
        else if (y2>y1) return 1;
        else if (y2<y1) return -1;
        else if (x1>x2) return 1;
        else if (x1<x2) return -1;
        else return 0; // only true when x1==x2 and y1==y2
    }

    // robust test of (x0,y0) in the triangle (x1,y1)-(x2,y2)-(x3,y3)
    // if true is returned, the barycentric coordinates are set in a,b,c.
    static bool point_in_triangle_2d(float x0, float y0,
        float x1, float y1, float x2, float y2, float x3, float y3,
        float& a, float& b, float& c)
    {
        x1 -= x0; x2 -= x0; x3 -= x0;
        y1 -= y0; y2 -= y0; y3 -= y0;
        int signa = orientation(x2, y2, x3, y3, a);
        if (signa == 0) return false;
        int signb = orientation(x3, y3, x1, y1, b);
        if (signb != signa) return false;
        int signc = orientation(x1, y1, x2, y2, c);
        if (signc != signa) return false;
        double sum = a + b + c;
        assert(sum != 0); // if the SOS signs match and are nonkero, there's no way all of a, b, and c are zero.
        a /= sum;
        b /= sum;
        c /= sum;
        return true;
    }
}

using namespace SprueLevelSetMethods;

namespace SprueEngine
{

    FilterableBlockMap<float>* MakeLevelSet(MeshData* mesh,
        const Vec3& origin, float dx, int ni, int nj, int nk,
        const int exact_band)
    {
        FilterableBlockMap<float>* phi = new FilterableBlockMap<float>();
        phi->resize(ni, nj, nk);
        phi->fill((ni + nj + nk)*dx);

        Array3i closest_tri(ni, nj, nk, -1);
        Array3i intersection_count(ni, nj, nk, 0); // intersection_count(i,j,k) is # of tri intersections in (i-1,i]x{j}x{k}
                                                   // we begin by initializing distances near the mesh, and figuring out intersection counts
        Vec3 ijkmin, ijkmax;

        auto& tri = mesh->GetIndexBuffer();
        auto& x = mesh->GetPositionBuffer();

        for (unsigned int t = 0; t < tri.size() / 3; ++t) {
            unsigned int p = tri[t], q = tri[t+1], r = tri[t+2];
                
            // coordinates in grid to high precision
            double fip = ((double)x[p][0] - origin[0]) / dx, fjp = ((double)x[p][1] - origin[1]) / dx, fkp = ((double)x[p][2] - origin[2]) / dx;
            double fiq = ((double)x[q][0] - origin[0]) / dx, fjq = ((double)x[q][1] - origin[1]) / dx, fkq = ((double)x[q][2] - origin[2]) / dx;
            double fir = ((double)x[r][0] - origin[0]) / dx, fjr = ((double)x[r][1] - origin[1]) / dx, fkr = ((double)x[r][2] - origin[2]) / dx;
            // do distances nearby
            int i0 = clamp(int(tripleMin(fip, fiq, fir)) - exact_band, 0, ni - 1), i1 = clamp(int(tripleMax(fip, fiq, fir)) + exact_band + 1, 0, ni - 1);
            int j0 = clamp(int(tripleMin(fjp, fjq, fjr)) - exact_band, 0, nj - 1), j1 = clamp(int(tripleMax(fjp, fjq, fjr)) + exact_band + 1, 0, nj - 1);
            int k0 = clamp(int(tripleMin(fkp, fkq, fkr)) - exact_band, 0, nk - 1), k1 = clamp(int(tripleMax(fkp, fkq, fkr)) + exact_band + 1, 0, nk - 1);
            for (int k = k0; k <= k1; ++k) 
                for (int j = j0; j <= j1; ++j) 
                    for (int i = i0; i <= i1; ++i) {
                        Vec3 gx(i*dx + origin[0], j*dx + origin[1], k*dx + origin[2]);
                        float d = point_triangle_distance(gx, x[p], x[q], x[r]);
                        if (d < phi->get(i, j, k)) {
                            phi->set(d, i, j, k);
                            closest_tri(i, j, k) = t;
                    }
            }
            // and do intersection counts
            j0 = clamp((int)std::ceil(tripleMin(fjp, fjq, fjr)), 0, nj - 1);
            j1 = clamp((int)std::floor(tripleMax(fjp, fjq, fjr)), 0, nj - 1);
            k0 = clamp((int)std::ceil(tripleMin(fkp, fkq, fkr)), 0, nk - 1);
            k1 = clamp((int)std::floor(tripleMax(fkp, fkq, fkr)), 0, nk - 1);
            for (int k = k0; k <= k1; ++k) 
                for (int j = j0; j <= j1; ++j) {
                    float a, b, c;
                    if (point_in_triangle_2d(j, k, fjp, fkp, fjq, fkq, fjr, fkr, a, b, c)) {
                        double fi = a*fip + b*fiq + c*fir; // intersection i coordinate
                        int i_interval = int(std::ceil(fi)); // intersection is in (i_interval-1,i_interval]
                        if (i_interval < 0) 
                            ++intersection_count(0, j, k); // we enlarge the first interval to include everything to the -x direction
                        else if (i_interval < ni) 
                            ++intersection_count(i_interval, j, k);
                        // we ignore intersections that are beyond the +x side of the grid
                }
            }
        }

        // and now we fill in the rest of the distances with fast sweeping
        for (unsigned int pass = 0; pass<2; ++pass) {
            sweep(tri, x, phi, closest_tri, origin, dx, +1, +1, +1);
            sweep(tri, x, phi, closest_tri, origin, dx, -1, -1, -1);
            sweep(tri, x, phi, closest_tri, origin, dx, +1, +1, -1);
            sweep(tri, x, phi, closest_tri, origin, dx, -1, -1, +1);
            sweep(tri, x, phi, closest_tri, origin, dx, +1, -1, +1);
            sweep(tri, x, phi, closest_tri, origin, dx, -1, +1, -1);
            sweep(tri, x, phi, closest_tri, origin, dx, +1, -1, -1);
            sweep(tri, x, phi, closest_tri, origin, dx, -1, +1, +1);
        }
        // then figure out signs (inside/outside) from intersection counts
        for (int k = 0; k<nk; ++k) for (int j = 0; j<nj; ++j) {
            int total_count = 0;
            for (int i = 0; i<ni; ++i) {
                total_count += intersection_count(i, j, k);
                if (total_count % 2 == 1) { // if parity of intersections so far is odd,
                    phi->set(-phi->get(i, j, k), i, j, k); // we are inside the mesh
                }
            }
        }

        return phi;
    }

    void CombineLevelSet(FilterableBlockMap<float>* targetSet, FilterableBlockMap<float>* sourceSet)
    {
        for (int z = 0; z < targetSet->getDepth(); ++z)
            for (int y = 0; y < targetSet->getHeight(); ++y)
                for (int x = 0; x < targetSet->getWidth(); ++x)
                    targetSet->set(std::min(sourceSet->get(x, y, z), targetSet->get(x,y,z)), x, y, z);
    }

    void MakeSolidSet(BlockMap<bool>* targetSolid, FilterableBlockMap<float>* levelset, float max)
    {
        for (int z = 0; z < targetSolid->getDepth(); ++z)
            for (int y = 0; y < targetSolid->getHeight(); ++y)
                for (int x = 0; x < targetSolid->getWidth(); ++x)
                    targetSolid->set(levelset->get(x, y, z) < max, x, y, z);
    }

    BlockMap<bool>* MakeSolidSet(FilterableBlockMap<float>* levelset, float max)
    {
        BlockMap<bool>* ret = new BlockMap<bool>(levelset->getWidth(), levelset->getHeight(), levelset->getDepth());
        ret->fill(false);
        MakeSolidSet(ret, levelset, max);
        return ret;
    }
}