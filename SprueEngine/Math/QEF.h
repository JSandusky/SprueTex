/*
* This is free and unencumbered software released into the public domain.
*
* Anyone is free to copy, modify, publish, use, compile, sell, or
* distribute this software, either in source code form or as a compiled
* binary, for any purpose, commercial or non-commercial, and by any
* means.
*
* In jurisdictions that recognize copyright laws, the author or authors
* of this software dedicate any and all copyright interest in the
* software to the public domain. We make this dedication for the benefit
* of the public at large and to the detriment of our heirs and
* successors. We intend this dedication to be an overt act of
* relinquishment in perpetuity of all present and future rights to this
* software under copyright law.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
* IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
* OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
* ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
* OTHER DEALINGS IN THE SOFTWARE.
*
* For more information, please refer to <http://unlicense.org/>
*/
#pragma once


#ifndef NO_OSTREAM
#include <iostream>
#endif

#include <SprueEngine/MathGeoLib/AllMath.h>
#include <SprueEngine/Math/SVD.h>

namespace SprueEngine
{
    class QefData
    {
    public:
        double ata_00, ata_01, ata_02, ata_11, ata_12, ata_22;
        double atb_x, atb_y, atb_z;
        double btb;
        double massPoint_x, massPoint_y, massPoint_z;
        int numPoints;

        QefData();

        QefData(const double ata_00, const double ata_01,
            const double ata_02, const double ata_11, const double ata_12,
            const double ata_22, const double atb_x, const  double atb_y,
            const double atb_z, const  double btb, const    double massPoint_x,
            const double massPoint_y, const double massPoint_z,
            const int numPoints);

        void add(const QefData &rhs);

        void clear();

        void set(const double ata_00, const double ata_01,
            const double ata_02, const double ata_11, const double ata_12,
            const double ata_22, const double atb_x, const double atb_y,
            const double atb_z, const double btb, const double massPoint_x,
            const double massPoint_y, const double massPoint_z,
            const int numPoints);

        void set(const QefData &rhs);

        QefData(const QefData &rhs);
        QefData &operator= (const QefData &rhs);
    };
#ifndef NO_OSTREAM
    //std::ostream &operator<<(std::ostream &os, const QefData &d);
#endif
    class QefSolver
    {
    private:
        QefData data;
        SMat3 ata;
        Vec3 atb, massPoint, x;
        bool hasSolution;
    public:
        QefSolver();
    public:

        const Vec3& getMassPoint() const { return massPoint; }

        void add(const double px, const double py, const double pz,
            double nx, double ny, double nz);
        void add(const Vec3 &p, const Vec3 &n);
        void add(const QefData &rhs);
        QefData getData();
        double getError();
        double getError(const Vec3 &pos);
        void reset();
        double solve(Vec3 &outx, const double svd_tol,
            const int svd_sweeps, const double pinv_tol);
    private:
        QefSolver(const QefSolver &rhs);
        QefSolver &operator=(const QefSolver &rhs);
        void setAta();
        void setAtb();
    };
};
