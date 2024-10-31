#include "SurfaceNets.h"

#include <SprueEngine/IMeshable.h>

namespace SprueEngine
{
    static inline int offset_3d(const Vec3 &p, const Vec3 &size)
    {
        return (p.z * size.y + p.y) * size.x + p.x;
    }
    
    static inline int offset_3d_slab(const Vec3 &p, const Vec3 &size)
    {
        return size.x * size.y * (((int)p.z) % 2) + p.y * size.x + p.x;
    }

    void SurfaceNets::GenerateMesh(IMeshable* meshable, VertexBuffer& vertexBuffer, IndexBuffer& indexBuffer)
    {
        static std::vector<int> inds(65 * 65 * 2);
        static std::vector<float> densities(65 * 65 * 65);

        // Precompute all density
        for (int z = 0; z < 65; ++z)
            for (int y = 0; y < 65; ++y)
                for (int x = 0; x < 65; ++x)
                {
                    Vec3 position(x, y, z);
                    densities[offset_3d(position, Vec3(65))] = meshable->CalculateDensity(position);
                }

        // Iterate and incrementall build
        const Vec3 size(65);
        for (int z = 0; z < 64; ++z)
            for (int y = 0; y < 64; ++y)
                for (int x = 0; x < 64; ++x)
                {
                    Vec3 position(x,y,z);
                    const float density[8] = {
                        densities[offset_3d(position, size)],
                        densities[offset_3d(position + Vec3(1, 0, 0), size)],
                        densities[offset_3d(position + Vec3(0, 1, 0), size)],
                        densities[offset_3d(position + Vec3(1, 1, 0), size)],
                        densities[offset_3d(position + Vec3(0, 0, 1), size)],
                        densities[offset_3d(position + Vec3(1, 0, 1), size)],
                        densities[offset_3d(position + Vec3(0, 1, 1), size)],
                        densities[offset_3d(position + Vec3(1, 1, 1), size)]
                    };

                    const int layout = ((density[0] < 0.0f) << 0) |
                        ((density[1] < 0.0f) << 1) |
                        ((density[2] < 0.0f) << 2) |
                        ((density[3] < 0.0f) << 3) |
                        ((density[4] < 0.0f) << 4) |
                        ((density[5] < 0.0f) << 5) |
                        ((density[6] < 0.0f) << 6) |
                        ((density[7] < 0.0f) << 7);

                    if (layout == 0 || layout == 255)
                        continue;

                    Vec3 average;
                    int average_n = 0;
                    auto do_edge = [&](float va, float vb, int axis, const Vec3 &p) {
                        if ((va < 0.0) == (vb < 0.0))
                            return;

                        Vec3 v = p;
                        v[axis] += va / (va - vb);
                        average += v;
                        average_n++;
                    };

                    do_edge(density[0], density[1], 0, Vec3(x, y, z));
                    do_edge(density[2], density[3], 0, Vec3(x, y + 1, z));
                    do_edge(density[4], density[5], 0, Vec3(x, y, z + 1));
                    do_edge(density[6], density[7], 0, Vec3(x, y + 1, z + 1));
                    do_edge(density[0], density[2], 1, Vec3(x, y, z));
                    do_edge(density[1], density[3], 1, Vec3(x + 1, y, z));
                    do_edge(density[4], density[6], 1, Vec3(x, y, z + 1));
                    do_edge(density[5], density[7], 1, Vec3(x + 1, y, z + 1));
                    do_edge(density[0], density[4], 2, Vec3(x, y, z));
                    do_edge(density[1], density[5], 2, Vec3(x + 1, y, z));
                    do_edge(density[2], density[6], 2, Vec3(x, y + 1, z));
                    do_edge(density[3], density[7], 2, Vec3(x + 1, y + 1, z));

                    Vec3 v = average / average_n;
                    if (blocky_)
                        v.Set(x + 0.5f, y + 0.5f, z + 0.5f);

                    inds[offset_3d_slab(position, Vec3(65))] = vertexBuffer.size();
                    vertexBuffer.push_back({ v, Vec3(0, 0, 0) });

                    auto quad = [&](bool flip, int ia, int ib, int ic, int id)
                    {
                        if (flip)
                            std::swap(ib, id);

                        if (ia >= vertexBuffer.size() || ib >= vertexBuffer.size() || ic >= vertexBuffer.size() || id >= vertexBuffer.size())
                            return;

                        MeshVertex& va = vertexBuffer[ia];
                        MeshVertex& vb = vertexBuffer[ib];
                        MeshVertex& vc = vertexBuffer[ic];
                        MeshVertex& vd = vertexBuffer[id];

                        const Vec3 ab = va.Position - vb.Position;
                        const Vec3 cb = vc.Position - vb.Position;
                        const Vec3 n1 = cb.Cross(ab);
                        va.Normal += n1;
                        vb.Normal += n1;
                        vc.Normal += n1;

                        const Vec3 ac = va.Position - vc.Position;
                        const Vec3 dc = vd.Position - vc.Position;
                        const Vec3 n2 = dc.Cross(ac);
                        va.Normal += n2;
                        vc.Normal += n2;
                        vd.Normal += n2;

                        indexBuffer.push_back(ia);
                        indexBuffer.push_back(ib);
                        indexBuffer.push_back(ic);

                        indexBuffer.push_back(ia);
                        indexBuffer.push_back(ic);
                        indexBuffer.push_back(id);
                    };

                    const bool flip = density[0] < 0.0f;
                    if (position.y > 0 && position.z > 0 && (density[0] < 0.0f) != (density[1] < 0.0f)) {
                        quad(flip,
                            inds[offset_3d_slab(Vec3(position.x, position.y, position.z), Vec3(65))],
                            inds[offset_3d_slab(Vec3(position.x, position.y, position.z - 1), Vec3(65))],
                            inds[offset_3d_slab(Vec3(position.x, position.y - 1, position.z - 1), Vec3(65))],
                            inds[offset_3d_slab(Vec3(position.x, position.y - 1, position.z), Vec3(65))]
                            );
                    }
                    if (position.x > 0 && position.z > 0 && (density[0] < 0.0f) != (density[2] < 0.0f)) {
                        quad(flip,
                            inds[offset_3d_slab(Vec3(position.x, position.y, position.z), Vec3(65))],
                            inds[offset_3d_slab(Vec3(position.x - 1, position.y, position.z), Vec3(65))],
                            inds[offset_3d_slab(Vec3(position.x - 1, position.y, position.z - 1), Vec3(65))],
                            inds[offset_3d_slab(Vec3(position.x, position.y, position.z - 1), Vec3(65))]
                            );
                    }
                    if (position.x > 0 && position.y > 0 && (density[0] < 0.0f) != (density[4] < 0.0f)) {
                        quad(flip,
                            inds[offset_3d_slab(Vec3(position.x, position.y, position.z), Vec3(65))],
                            inds[offset_3d_slab(Vec3(position.x, position.y - 1, position.z), Vec3(65))],
                            inds[offset_3d_slab(Vec3(position.x - 1, position.y - 1, position.z), Vec3(65))],
                            inds[offset_3d_slab(Vec3(position.x - 1, position.y, position.z), Vec3(65))]
                            );
                    }
                }

                for (MeshVertex& v : vertexBuffer)
                    v.Normal.Normalize();
    }

    void SurfaceNets::ByteGenerateMesh(IMeshable* meshable, VertexBuffer& vertexBuffer, IndexBuffer& indexBuffer)
    {
        static std::vector<int> inds(65 * 65 * 2);
        static std::vector<unsigned char> densities(65 * 65 * 65);

        // Precompute all density
        for (int z = 0; z < 65; ++z)
            for (int y = 0; y < 65; ++y)
                for (int x = 0; x < 65; ++x)
                {
                    Vec3 position(x, y, z);
                    float sine = NORMALIZE(sinf(x), -1, 1) * 32;
                    float cosine = NORMALIZE(cosf(z), -1, 1) * 32;
                    densities[offset_3d(position, Vec3(65))] = y < sine && y < cosine ? 1 : 0;
                }

        // Iterate and incrementall build
        const Vec3 size(65);
        for (int z = 0; z < 64; ++z)
            for (int y = 0; y < 64; ++y)
                for (int x = 0; x < 64; ++x)
                {
                    Vec3 position(x, y, z);
                    const unsigned char density[8] = {
                        densities[offset_3d(position, size)],
                        densities[offset_3d(position + Vec3(1, 0, 0), size)],
                        densities[offset_3d(position + Vec3(0, 1, 0), size)],
                        densities[offset_3d(position + Vec3(1, 1, 0), size)],
                        densities[offset_3d(position + Vec3(0, 0, 1), size)],
                        densities[offset_3d(position + Vec3(1, 0, 1), size)],
                        densities[offset_3d(position + Vec3(0, 1, 1), size)],
                        densities[offset_3d(position + Vec3(1, 1, 1), size)]
                    };

                    const int layout = ((density[0] != 0) << 0) |
                        ((density[1] != 0) << 1) |
                        ((density[2] != 0) << 2) |
                        ((density[3] != 0) << 3) |
                        ((density[4] != 0) << 4) |
                        ((density[5] != 0) << 5) |
                        ((density[6] != 0) << 6) |
                        ((density[7] != 0) << 7);

                    if (layout == 0 || layout == 255)
                        continue;

                    Vec3 average;
                    int average_n = 0;
                    auto do_edge = [&](float va, float vb, int axis, const Vec3 &p) {
                        if ((va != 0) == (vb != 0))
                            return;

                        Vec3 v = p;
                        v[axis] += 0.5f;
                        average += v;
                        average_n++;
                    };

                    do_edge(density[0], density[1], 0, Vec3(x, y, z));
                    do_edge(density[2], density[3], 0, Vec3(x, y + 1, z));
                    do_edge(density[4], density[5], 0, Vec3(x, y, z + 1));
                    do_edge(density[6], density[7], 0, Vec3(x, y + 1, z + 1));
                    do_edge(density[0], density[2], 1, Vec3(x, y, z));
                    do_edge(density[1], density[3], 1, Vec3(x + 1, y, z));
                    do_edge(density[4], density[6], 1, Vec3(x, y, z + 1));
                    do_edge(density[5], density[7], 1, Vec3(x + 1, y, z + 1));
                    do_edge(density[0], density[4], 2, Vec3(x, y, z));
                    do_edge(density[1], density[5], 2, Vec3(x + 1, y, z));
                    do_edge(density[2], density[6], 2, Vec3(x, y + 1, z));
                    do_edge(density[3], density[7], 2, Vec3(x + 1, y + 1, z));

                    Vec3 v = average / average_n;
                    inds[offset_3d_slab(position, Vec3(65))] = vertexBuffer.size();
                    vertexBuffer.push_back({ v, Vec3(0, 0, 0) });

                    auto quad = [&](bool flip, int ia, int ib, int ic, int id)
                    {
                        if (flip)
                            std::swap(ib, id);

                        if (ia >= vertexBuffer.size() || ib >= vertexBuffer.size() || ic >= vertexBuffer.size() || id >= vertexBuffer.size())
                            return;

                        MeshVertex& va = vertexBuffer[ia];
                        MeshVertex& vb = vertexBuffer[ib];
                        MeshVertex& vc = vertexBuffer[ic];
                        MeshVertex& vd = vertexBuffer[id];

                        const Vec3 ab = va.Position - vb.Position;
                        const Vec3 cb = vc.Position - vb.Position;
                        const Vec3 n1 = cb.Cross(ab);
                        va.Normal += n1;
                        vb.Normal += n1;
                        vc.Normal += n1;

                        const Vec3 ac = va.Position - vc.Position;
                        const Vec3 dc = vd.Position - vc.Position;
                        const Vec3 n2 = dc.Cross(ac);
                        va.Normal += n2;
                        vc.Normal += n2;
                        vd.Normal += n2;

                        indexBuffer.push_back(ia);
                        indexBuffer.push_back(ib);
                        indexBuffer.push_back(ic);

                        indexBuffer.push_back(ia);
                        indexBuffer.push_back(ic);
                        indexBuffer.push_back(id);
                    };

                    const bool flip = density[0] != 0;
                    if (position.y > 0 && position.z > 0 && (density[0] != 0) != (density[1] != 0)) {
                        quad(flip,
                            inds[offset_3d_slab(Vec3(position.x, position.y, position.z), Vec3(65))],
                            inds[offset_3d_slab(Vec3(position.x, position.y, position.z - 1), Vec3(65))],
                            inds[offset_3d_slab(Vec3(position.x, position.y - 1, position.z - 1), Vec3(65))],
                            inds[offset_3d_slab(Vec3(position.x, position.y - 1, position.z), Vec3(65))]
                            );
                    }
                    if (position.x > 0 && position.z > 0 && (density[0] != 0) != (density[2] != 0)) {
                        quad(flip,
                            inds[offset_3d_slab(Vec3(position.x, position.y, position.z), Vec3(65))],
                            inds[offset_3d_slab(Vec3(position.x - 1, position.y, position.z), Vec3(65))],
                            inds[offset_3d_slab(Vec3(position.x - 1, position.y, position.z - 1), Vec3(65))],
                            inds[offset_3d_slab(Vec3(position.x, position.y, position.z - 1), Vec3(65))]
                            );
                    }
                    if (position.x > 0 && position.y > 0 && (density[0] != 0) != (density[4] != 0)) {
                        quad(flip,
                            inds[offset_3d_slab(Vec3(position.x, position.y, position.z), Vec3(65))],
                            inds[offset_3d_slab(Vec3(position.x, position.y - 1, position.z), Vec3(65))],
                            inds[offset_3d_slab(Vec3(position.x - 1, position.y - 1, position.z), Vec3(65))],
                            inds[offset_3d_slab(Vec3(position.x - 1, position.y, position.z), Vec3(65))]
                            );
                    }
                }

        for (MeshVertex& v : vertexBuffer)
            v.Normal.Normalize();
    }

    void SurfaceNets::GenerateMesh(const float* densities, const Vec4* points, const unsigned char* writeMasks, const int size, VertexBuffer& vertexBuffer, IndexBuffer& indexBuffer)
    {
        static std::vector<int> inds(65 * 65 * 2); // TODO: what's the actually maximum possible number of indices vs the actual plausible number?

        const Vec3 densitySizeVec(size + 1);
        const Vec3 sizeVec(size);
        for (int z = 0; z < size; ++z)
            for (int y = 0; y < size; ++y)
                for (int x = 0; x < size; ++x)
                {
                    Vec3 position(x, y, z);
                    unsigned cellPos = offset_3d(position, sizeVec);
                    unsigned char layout = writeMasks[cellPos];
                    if (layout == 0 || layout == 255)
                        continue;

                    const float density[8] = {
                        densities[offset_3d(position, densitySizeVec)],
                        densities[offset_3d(position + Vec3(1, 0, 0), densitySizeVec)],
                        densities[offset_3d(position + Vec3(0, 1, 0), densitySizeVec)],
                        densities[offset_3d(position + Vec3(1, 1, 0), densitySizeVec)],
                        densities[offset_3d(position + Vec3(0, 0, 1), densitySizeVec)],
                        densities[offset_3d(position + Vec3(1, 0, 1), densitySizeVec)],
                        densities[offset_3d(position + Vec3(0, 1, 1), densitySizeVec)],
                        densities[offset_3d(position + Vec3(1, 1, 1), densitySizeVec)]
                    };

                    Vec3 v = points[cellPos].xyz() / points[cellPos].w;
                    inds[offset_3d_slab(position, Vec3(65))] = vertexBuffer.size();
                    vertexBuffer.push_back({ v, Vec3(0, 0, 0) });

                    auto quad = [&](bool flip, int ia, int ib, int ic, int id)
                    {
                        if (flip)
                            std::swap(ib, id);

                        if (ia >= vertexBuffer.size() || ib >= vertexBuffer.size() || ic >= vertexBuffer.size() || id >= vertexBuffer.size())
                            return;

                        MeshVertex& va = vertexBuffer[ia];
                        MeshVertex& vb = vertexBuffer[ib];
                        MeshVertex& vc = vertexBuffer[ic];
                        MeshVertex& vd = vertexBuffer[id];

                        const Vec3 ab = va.Position - vb.Position;
                        const Vec3 cb = vc.Position - vb.Position;
                        const Vec3 n1 = cb.Cross(ab);
                        va.Normal += n1;
                        vb.Normal += n1;
                        vc.Normal += n1;

                        const Vec3 ac = va.Position - vc.Position;
                        const Vec3 dc = vd.Position - vc.Position;
                        const Vec3 n2 = dc.Cross(ac);
                        va.Normal += n2;
                        vc.Normal += n2;
                        vd.Normal += n2;

                        indexBuffer.push_back(ia);
                        indexBuffer.push_back(ib);
                        indexBuffer.push_back(ic);

                        indexBuffer.push_back(ia);
                        indexBuffer.push_back(ic);
                        indexBuffer.push_back(id);
                    };

                    const bool flip = density[0] < 0.0f;
                    if (position.y > 0 && position.z > 0 && (density[0] < 0.0f) != (density[1] < 0.0f)) {
                        quad(flip,
                            inds[offset_3d_slab(Vec3(position.x, position.y, position.z), Vec3(65))],
                            inds[offset_3d_slab(Vec3(position.x, position.y, position.z - 1), Vec3(65))],
                            inds[offset_3d_slab(Vec3(position.x, position.y - 1, position.z - 1), Vec3(65))],
                            inds[offset_3d_slab(Vec3(position.x, position.y - 1, position.z), Vec3(65))]
                            );
                    }
                    if (position.x > 0 && position.z > 0 && (density[0] < 0.0f) != (density[2] < 0.0f)) {
                        quad(flip,
                            inds[offset_3d_slab(Vec3(position.x, position.y, position.z), Vec3(65))],
                            inds[offset_3d_slab(Vec3(position.x - 1, position.y, position.z), Vec3(65))],
                            inds[offset_3d_slab(Vec3(position.x - 1, position.y, position.z - 1), Vec3(65))],
                            inds[offset_3d_slab(Vec3(position.x, position.y, position.z - 1), Vec3(65))]
                            );
                    }
                    if (position.x > 0 && position.y > 0 && (density[0] < 0.0f) != (density[4] < 0.0f)) {
                        quad(flip,
                            inds[offset_3d_slab(Vec3(position.x, position.y, position.z), Vec3(65))],
                            inds[offset_3d_slab(Vec3(position.x, position.y - 1, position.z), Vec3(65))],
                            inds[offset_3d_slab(Vec3(position.x - 1, position.y - 1, position.z), Vec3(65))],
                            inds[offset_3d_slab(Vec3(position.x - 1, position.y, position.z), Vec3(65))]
                            );
                    }
                }
    }
}