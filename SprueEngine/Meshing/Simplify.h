/////////////////////////////////////////////
//
// Mesh Simplification Tutorial
//
// (C) by Sven Forstmann in 2014
//
// License : MIT
// http://opensource.org/licenses/MIT
//
//https://github.com/sp4cerat/Fast-Quadric-Mesh-Simplification
//
// 5/2016: Chris Rorden created minimal version for OSX/Linux/Windows compile

// 5/12/2016: Jonathan Sandusky, modifications
//          switch from double to float
//          convert to SprueEngine types (Vec3)
//      TODO: add support for vertex attributes and their interpolation

#pragma once

#include <SprueEngine/MathGeoLib/AllMath.h>

#include <string.h>
#include <stdio.h>
#include <stdlib.h>
#include <vector>
#include <math.h>
#include <float.h> //FLT_EPSILON, DBL_EPSILON

#define loopi(start_l,end_l) for ( int i=start_l;i<end_l;++i )
#define loopi(start_l,end_l) for ( int i=start_l;i<end_l;++i )
#define loopj(start_l,end_l) for ( int j=start_l;j<end_l;++j )
#define loopk(start_l,end_l) for ( int k=start_l;k<end_l;++k )

using namespace SprueEngine;

class SymetricMatrix {

public:

    // Constructor

    SymetricMatrix(float c = 0) { loopi(0, 10) m[i] = c; }

    SymetricMatrix(float m11, float m12, float m13, float m14,
        float m22, float m23, float m24,
        float m33, float m34,
        float m44) {
        m[0] = m11;  m[1] = m12;  m[2] = m13;  m[3] = m14;
        m[4] = m22;  m[5] = m23;  m[6] = m24;
        m[7] = m33;  m[8] = m34;
        m[9] = m44;
    }

    // Make plane

    SymetricMatrix(float a, float b, float c, float d)
    {
        m[0] = a*a;  m[1] = a*b;  m[2] = a*c;  m[3] = a*d;
        m[4] = b*b;  m[5] = b*c;  m[6] = b*d;
        m[7] = c*c; m[8] = c*d;
        m[9] = d*d;
    }

    float operator[](int c) const { return m[c]; }

    // Determinant

    float det(int a11, int a12, int a13,
        int a21, int a22, int a23,
        int a31, int a32, int a33)
    {
        float det = m[a11] * m[a22] * m[a33] + m[a13] * m[a21] * m[a32] + m[a12] * m[a23] * m[a31]
            - m[a13] * m[a22] * m[a31] - m[a11] * m[a23] * m[a32] - m[a12] * m[a21] * m[a33];
        return det;
    }

    const SymetricMatrix operator+(const SymetricMatrix& n) const
    {
        return SymetricMatrix(m[0] + n[0], m[1] + n[1], m[2] + n[2], m[3] + n[3],
            m[4] + n[4], m[5] + n[5], m[6] + n[6],
            m[7] + n[7], m[8] + n[8],
            m[9] + n[9]);
    }

    SymetricMatrix& operator+=(const SymetricMatrix& n)
    {
        m[0] += n[0];   m[1] += n[1];   m[2] += n[2];   m[3] += n[3];
        m[4] += n[4];   m[5] += n[5];   m[6] += n[6];   m[7] += n[7];
        m[8] += n[8];   m[9] += n[9];
        return *this;
    }

    float m[10];
};
///////////////////////////////////////////

namespace Simplify
{
    // Global Variables & Strctures

    struct Triangle { int v[3]; float err[4]; int deleted, dirty; Vec3 n; };
    struct Vertex { Vec3 p; Vec2 uv; int tstart, tcount; SymetricMatrix q; int border; };
    struct Ref { int tid, tvertex; };
    std::vector<Triangle> triangles;
    std::vector<Vertex> vertices;
    std::vector<Ref> refs;

    // Helper functions

    float vertex_error(SymetricMatrix q, float x, float y, float z);
    float calculate_error(int id_v1, int id_v2, Vec3 &p_result);
    bool flipped(Vec3 p, int i0, int i1, Vertex &v0, Vertex &v1, std::vector<int> &deleted);
    void update_triangles(int i0, Vertex &v, std::vector<int> &deleted, int &deleted_triangles);
    void update_mesh(int iteration);
    void compact_mesh();
    //
    // Main simplification function
    //
    // target_count  : target nr. of triangles
    // agressiveness : sharpness to increase the threashold.
    //                 5..8 are good numbers
    //                 more iterations yield higher quality
    //

    void simplify_mesh(int target_count, float agressiveness = 7, bool verbose = false)
    {
        // init
        loopi(0, triangles.size()) triangles[i].deleted = 0;

        // main iteration loop
        int deleted_triangles = 0;
        std::vector<int> deleted0, deleted1;
        int triangle_count = triangles.size();
        //int iteration = 0;
        //loop(iteration,0,100)
        for (int iteration = 0; iteration < 100; iteration++)
        {
            if (triangle_count - deleted_triangles <= target_count)break;

            // update mesh once in a while
            if (iteration % 5 == 0)
            {
                update_mesh(iteration);
            }

            // clear dirty flag
            loopi(0, triangles.size()) triangles[i].dirty = 0;

            //
            // All triangles with edges below the threshold will be removed
            //
            // The following numbers works well for most models.
            // If it does not, try to adjust the 3 parameters
            //
            float threshold = 0.000000001*pow(float(iteration + 3), agressiveness);

            // target number of triangles reached ? Then break
            if ((verbose) && (iteration % 5 == 0)) {
                printf("iteration %d - triangles %d threshold %g\n", iteration, triangle_count - deleted_triangles, threshold);
            }

            // remove vertices & mark deleted triangles
            loopi(0, triangles.size())
            {
                Triangle &t = triangles[i];
                if (t.err[3]>threshold) continue;
                if (t.deleted) continue;
                if (t.dirty) continue;

                loopj(0, 3)if (t.err[j]<threshold)
                {

                    int i0 = t.v[j]; Vertex &v0 = vertices[i0];
                    int i1 = t.v[(j + 1) % 3]; Vertex &v1 = vertices[i1];
                    // Border check
                    if (v0.border != v1.border)  continue;

                    // Compute vertex to collapse to
                    Vec3 p;
                    calculate_error(i0, i1, p);
                    deleted0.resize(v0.tcount); // normals temporarily
                    deleted1.resize(v1.tcount); // normals temporarily
                    // dont remove if flipped
                    if (flipped(p, i0, i1, v0, v1, deleted0)) continue;

                    if (flipped(p, i1, i0, v1, v0, deleted1)) continue;


                    // not flipped, so remove edge
                    v0.p = p;
                    v0.q = v1.q + v0.q;
                    int tstart = refs.size();

                    update_triangles(i0, v0, deleted0, deleted_triangles);
                    update_triangles(i0, v1, deleted1, deleted_triangles);

                    int tcount = refs.size() - tstart;

                    if (tcount <= v0.tcount)
                    {
                        // save ram
                        if (tcount)memcpy(&refs[v0.tstart], &refs[tstart], tcount*sizeof(Ref));
                    }
                    else
                        // append
                        v0.tstart = tstart;

                    v0.tcount = tcount;
                    break;
                }
                // done?
                if (triangle_count - deleted_triangles <= target_count)break;
            }
        }
        // clean up mesh
        compact_mesh();
    } //simplify_mesh()

    void simplify_mesh_lossless(bool verbose = false)
    {
        // init
        loopi(0, triangles.size()) triangles[i].deleted = 0;

        // main iteration loop
        int deleted_triangles = 0;
        std::vector<int> deleted0, deleted1;
        int triangle_count = triangles.size();
        //int iteration = 0;
        //loop(iteration,0,100)
        for (int iteration = 0; iteration < 9999; iteration++)
        {
            // update mesh constantly
            update_mesh(iteration);
            // clear dirty flag
            loopi(0, triangles.size()) triangles[i].dirty = 0;
            //
            // All triangles with edges below the threshold will be removed
            //
            // The following numbers works well for most models.
            // If it does not, try to adjust the 3 parameters
            //
            float threshold = DBL_EPSILON; //1.0E-3 EPS;
            if (verbose) {
                printf("lossless iteration %d\n", iteration);
            }

            // remove vertices & mark deleted triangles
            loopi(0, triangles.size())
            {
                Triangle &t = triangles[i];
                if (t.err[3]>threshold) continue;
                if (t.deleted) continue;
                if (t.dirty) continue;

                loopj(0, 3)if (t.err[j]<threshold)
                {
                    int i0 = t.v[j]; Vertex &v0 = vertices[i0];
                    int i1 = t.v[(j + 1) % 3]; Vertex &v1 = vertices[i1];

                    // Border check
                    if (v0.border != v1.border)  continue;

                    // Compute vertex to collapse to
                    Vec3 p;
                    calculate_error(i0, i1, p);

                    deleted0.resize(v0.tcount); // normals temporarily
                    deleted1.resize(v1.tcount); // normals temporarily

                    // dont remove if flipped
                    if (flipped(p, i0, i1, v0, v1, deleted0)) continue;
                    if (flipped(p, i1, i0, v1, v0, deleted1)) continue;

                    // not flipped, so remove edge
                    v0.p = p;
                    v0.q = v1.q + v0.q;
                    int tstart = refs.size();

                    update_triangles(i0, v0, deleted0, deleted_triangles);
                    update_triangles(i0, v1, deleted1, deleted_triangles);

                    int tcount = refs.size() - tstart;

                    if (tcount <= v0.tcount)
                    {
                        // save ram
                        if (tcount)memcpy(&refs[v0.tstart], &refs[tstart], tcount*sizeof(Ref));
                    }
                    else
                        // append
                        v0.tstart = tstart;

                    v0.tcount = tcount;
                    break;
                }
            }
            if (deleted_triangles <= 0)break;
            deleted_triangles = 0;
        } //for each iteration
        // clean up mesh
        compact_mesh();
    } //simplify_mesh_lossless()


    // Check if a triangle flips when this edge is removed

    bool flipped(Vec3 p, int i0, int i1, Vertex &v0, Vertex &v1, std::vector<int> &deleted)
    {

        loopk(0, v0.tcount)
        {
            Triangle &t = triangles[refs[v0.tstart + k].tid];
            if (t.deleted)continue;

            int s = refs[v0.tstart + k].tvertex;
            int id1 = t.v[(s + 1) % 3];
            int id2 = t.v[(s + 2) % 3];

            if (id1 == i1 || id2 == i1) // delete ?
            {

                deleted[k] = 1;
                continue;
            }
            Vec3 d1 = vertices[id1].p - p; d1.Normalize();
            Vec3 d2 = vertices[id2].p - p; d2.Normalize();
            if (fabs(d1.Dot(d2))>0.999) 
                return true;
            Vec3 n = d1.Cross(d2);
            n.Normalize();
            deleted[k] = 0;
            if (n.Dot(t.n)<0.2) 
                return true;
        }
        return false;
    }

    // Update triangle connections and edge error after a edge is collapsed

    void update_triangles(int i0, Vertex &v, std::vector<int> &deleted, int &deleted_triangles)
    {
        Vec3 p;
        loopk(0, v.tcount)
        {
            Ref &r = refs[v.tstart + k];
            Triangle &t = triangles[r.tid];
            if (t.deleted)continue;
            if (deleted[k])
            {
                t.deleted = 1;
                deleted_triangles++;
                continue;
            }
            t.v[r.tvertex] = i0;
            t.dirty = 1;
            t.err[0] = calculate_error(t.v[0], t.v[1], p);
            t.err[1] = calculate_error(t.v[1], t.v[2], p);
            t.err[2] = calculate_error(t.v[2], t.v[0], p);
            t.err[3] = SprueMin(t.err[0], SprueMin(t.err[1], t.err[2]));
            refs.push_back(r);
        }
    }

    // compact triangles, compute edge error and build reference list

    void update_mesh(int iteration)
    {
        if (iteration>0) // compact triangles
        {
            int dst = 0;
            loopi(0, triangles.size())
                if (!triangles[i].deleted)
                {
                    triangles[dst++] = triangles[i];
                }
            triangles.resize(dst);
        }
        //
        // Init Quadrics by Plane & Edge Errors
        //
        // required at the beginning ( iteration == 0 )
        // recomputing during the simplification is not required,
        // but mostly improves the result for closed meshes
        //
        if (iteration == 0)
        {
            loopi(0, vertices.size())
                vertices[i].q = SymetricMatrix(0.0);

            loopi(0, triangles.size())
            {
                Triangle &t = triangles[i];
                Vec3 n, p[3];
                loopj(0, 3) p[j] = vertices[t.v[j]].p;
                n = (p[1] - p[0]).Cross(p[2] - p[0]);
                n.Normalize();
                t.n = n;
                loopj(0, 3) vertices[t.v[j]].q =
                    vertices[t.v[j]].q + SymetricMatrix(n.x, n.y, n.z, -n.Dot(p[0]));
            }
            loopi(0, triangles.size())
            {
                // Calc Edge Error
                Triangle &t = triangles[i]; Vec3 p;
                loopj(0, 3) t.err[j] = calculate_error(t.v[j], t.v[(j + 1) % 3], p);
                t.err[3] = SprueMin(t.err[0], SprueMin(t.err[1], t.err[2]));
            }
        }

        // Init Reference ID list
        loopi(0, vertices.size())
        {
            vertices[i].tstart = 0;
            vertices[i].tcount = 0;
        }
        loopi(0, triangles.size())
        {
            Triangle &t = triangles[i];
            loopj(0, 3) vertices[t.v[j]].tcount++;
        }
        int tstart = 0;
        loopi(0, vertices.size())
        {
            Vertex &v = vertices[i];
            v.tstart = tstart;
            tstart += v.tcount;
            v.tcount = 0;
        }

        // Write References
        refs.resize(triangles.size() * 3);
        loopi(0, triangles.size())
        {
            Triangle &t = triangles[i];
            loopj(0, 3)
            {
                Vertex &v = vertices[t.v[j]];
                refs[v.tstart + v.tcount].tid = i;
                refs[v.tstart + v.tcount].tvertex = j;
                v.tcount++;
            }
        }

        // Identify boundary : vertices[].border=0,1
        if (iteration == 0)
        {
            std::vector<int> vcount, vids;

            loopi(0, vertices.size())
                vertices[i].border = 0;

            loopi(0, vertices.size())
            {
                Vertex &v = vertices[i];
                vcount.clear();
                vids.clear();
                loopj(0, v.tcount)
                {
                    int k = refs[v.tstart + j].tid;
                    Triangle &t = triangles[k];
                    loopk(0, 3)
                    {
                        int ofs = 0, id = t.v[k];
                        while (ofs<vcount.size())
                        {
                            if (vids[ofs] == id)break;
                            ofs++;
                        }
                        if (ofs == vcount.size())
                        {
                            vcount.push_back(1);
                            vids.push_back(id);
                        }
                        else
                            vcount[ofs]++;
                    }
                }
                loopj(0, vcount.size()) if (vcount[j] == 1)
                    vertices[vids[j]].border = 1;
            }
        }
    }

    // Finally compact mesh before exiting

    void compact_mesh()
    {
        int dst = 0;
        loopi(0, vertices.size())
        {
            vertices[i].tcount = 0;
        }
        loopi(0, triangles.size())
            if (!triangles[i].deleted)
            {
                Triangle &t = triangles[i];
                triangles[dst++] = t;
                loopj(0, 3)vertices[t.v[j]].tcount = 1;
            }
        triangles.resize(dst);
        dst = 0;
        loopi(0, vertices.size())
            if (vertices[i].tcount)
            {
                vertices[i].tstart = dst;
                vertices[dst].p = vertices[i].p;
                dst++;
            }
        loopi(0, triangles.size())
        {
            Triangle &t = triangles[i];
            loopj(0, 3)t.v[j] = vertices[t.v[j]].tstart;
        }
        vertices.resize(dst);
    }

    // Error between vertex and Quadric

    float vertex_error(SymetricMatrix q, float x, float y, float z)
    {
        return   q[0] * x*x + 2 * q[1] * x*y + 2 * q[2] * x*z + 2 * q[3] * x + q[4] * y*y
            + 2 * q[5] * y*z + 2 * q[6] * y + q[7] * z*z + 2 * q[8] * z + q[9];
    }

    // Error for one edge

    float calculate_error(int id_v1, int id_v2, Vec3 &p_result)
    {
        // compute interpolated vertex

        SymetricMatrix q = vertices[id_v1].q + vertices[id_v2].q;
        bool   border = vertices[id_v1].border & vertices[id_v2].border;
        float error = 0;
        float det = q.det(0, 1, 2, 1, 4, 5, 2, 5, 7);
        if (det != 0 && !border)
        {

            // q_delta is invertible
            p_result.x = -1 / det*(q.det(1, 2, 3, 4, 5, 6, 5, 7, 8));	// vx = A41/det(q_delta)
            p_result.y = 1 / det*(q.det(0, 2, 3, 1, 5, 6, 2, 7, 8));	// vy = A42/det(q_delta)
            p_result.z = -1 / det*(q.det(0, 1, 3, 1, 4, 6, 2, 5, 8));	// vz = A43/det(q_delta)

            error = vertex_error(q, p_result.x, p_result.y, p_result.z);
        }
        else
        {
            // det = 0 -> try to find best result
            Vec3 p1 = vertices[id_v1].p;
            Vec3 p2 = vertices[id_v2].p;
            Vec3 p3 = (p1 + p2) / 2;
            float error1 = vertex_error(q, p1.x, p1.y, p1.z);
            float error2 = vertex_error(q, p2.x, p2.y, p2.z);
            float error3 = vertex_error(q, p3.x, p3.y, p3.z);
            error = SprueMin(error1, SprueMin(error2, error3));
            if (error1 == error) 
                p_result = p1;
            if (error2 == error) 
                p_result = p2;
            if (error3 == error) 
                p_result = p3;
        }
        return error;
    }

    //Option : Load OBJ
    void load_obj(const char* filename){
        vertices.clear();
        triangles.clear();
        //printf ( "Loading Objects %s ... \n",filename);
        FILE* fn;
        if (filename == NULL)		return;
        if ((char)filename[0] == 0)	return;
        if ((fn = fopen(filename, "rb")) == NULL)
        {
            printf("File %s not found!\n", filename);
            return;
        }
        char line[1000];
        memset(line, 0, 1000);
        int vertex_cnt = 0;
        while (fgets(line, 1000, fn) != NULL)
        {
            Vertex v;
            if (line[0] == 'v')
            {
                if (line[1] == ' ')
                    if (sscanf(line, "v %lf %lf %lf",
                        &v.p.x, &v.p.y, &v.p.z) == 3)
                    {
                        vertices.push_back(v);
                    }
            }
            int integers[9];
            if (line[0] == 'f')
            {
                Triangle t;
                bool tri_ok = false;

                if (sscanf(line, "f %d %d %d",
                    &integers[0], &integers[1], &integers[2]) == 3)
                {
                    tri_ok = true;
                }
                else
                    if (sscanf(line, "f %d// %d// %d//",
                        &integers[0], &integers[1], &integers[2]) == 3)
                    {
                        tri_ok = true;
                    }
                    else
                        if (sscanf(line, "f %d//%d %d//%d %d//%d",
                            &integers[0], &integers[3],
                            &integers[1], &integers[4],
                            &integers[2], &integers[5]) == 6)
                        {
                            tri_ok = true;
                        }
                        else
                            if (sscanf(line, "f %d/%d/%d %d/%d/%d %d/%d/%d",
                                &integers[0], &integers[6], &integers[3],
                                &integers[1], &integers[7], &integers[4],
                                &integers[2], &integers[8], &integers[5]) == 9)
                            {
                                tri_ok = true;
                            }
                            else
                            {
                                printf("unrecognized sequence\n");
                                printf("%s\n", line);
                                while (1);
                            }
                if (tri_ok)
                {
                    t.v[0] = integers[0] - 1 - vertex_cnt;
                    t.v[1] = integers[1] - 1 - vertex_cnt;
                    t.v[2] = integers[2] - 1 - vertex_cnt;

                    //tri.material = material;
                    //geo.triangles.push_back ( tri );
                    triangles.push_back(t);
                    //state_before = state;
                    //state ='f';
                }
            }
        }
        fclose(fn);
        //printf("load_obj: vertices = %lu, triangles = %lu\n", vertices.size(), triangles.size() );
    } // load_obj()

    // Optional : Store as OBJ

    void write_obj(const char* filename)
    {
        FILE *file = fopen(filename, "w");
        if (!file)
        {
            printf("write_obj: can't write data file \"%s\".\n", filename);
            exit(0);
        }
        loopi(0, vertices.size())
        {
            //fprintf(file, "v %lf %lf %lf\n", vertices[i].p.x,vertices[i].p.y,vertices[i].p.z);
            fprintf(file, "v %g %g %g\n", vertices[i].p.x, vertices[i].p.y, vertices[i].p.z); //more compact: remove trailing zeros
        }
        loopi(0, triangles.size()) if (!triangles[i].deleted)
        {
            fprintf(file, "f %d %d %d\n", triangles[i].v[0] + 1, triangles[i].v[1] + 1, triangles[i].v[2] + 1);
            //fprintf(file, "f %d// %d// %d//\n", triangles[i].v[0]+1, triangles[i].v[1]+1, triangles[i].v[2]+1); //more compact: remove trailing zeros
        }
        fclose(file);
    }
};
///////////////////////////////////////////