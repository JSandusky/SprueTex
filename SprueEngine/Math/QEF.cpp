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
#include "SprueEngine/Math/QEF.h"
#include <stdexcept>

namespace SprueEngine
{

    QefData::QefData()
    {
        clear();
    }

    QefData::QefData(const double ata_00, const double ata_01,
        const double ata_02, const double ata_11, const double ata_12,
        const double ata_22, const double atb_x, const double atb_y,
        const double atb_z, const double btb, const double massPoint_x,
        const double massPoint_y, const double massPoint_z,
        const int numPoints)
    {
        set(ata_00, ata_01, ata_02, ata_11, ata_12, ata_22, atb_x, atb_y,
            atb_z, btb, massPoint_x, massPoint_y, massPoint_z, numPoints);
    }

    QefData::QefData(const QefData &rhs)
    {
        set(rhs);
    }

    QefData& QefData::operator=(const QefData& rhs)
    {
        set(rhs);
        return *this;
    }

    void QefData::add(const QefData &rhs)
    {
        ata_00 += rhs.ata_00;
        ata_01 += rhs.ata_01;
        ata_02 += rhs.ata_02;
        ata_11 += rhs.ata_11;
        ata_12 += rhs.ata_12;
        ata_22 += rhs.ata_22;
        atb_x += rhs.atb_x;
        atb_y += rhs.atb_y;
        atb_z += rhs.atb_z;
        btb += rhs.btb;
        massPoint_x += rhs.massPoint_x;
        massPoint_y += rhs.massPoint_y;
        massPoint_z += rhs.massPoint_z;
        numPoints += rhs.numPoints;
    }

    void QefData::clear()
    {
        set(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    }

    void QefData::set(const double ata_00, const double ata_01,
        const double ata_02, const double ata_11, const double ata_12,
        const double ata_22, const double atb_x, const double atb_y,
        const double atb_z, const double btb, const double massPoint_x,
        const double massPoint_y, const double massPoint_z,
        const int numPoints)
    {
        this->ata_00 = ata_00;
        this->ata_01 = ata_01;
        this->ata_02 = ata_02;
        this->ata_11 = ata_11;
        this->ata_12 = ata_12;
        this->ata_22 = ata_22;
        this->atb_x = atb_x;
        this->atb_y = atb_y;
        this->atb_z = atb_z;
        this->btb = btb;
        this->massPoint_x = massPoint_x;
        this->massPoint_y = massPoint_y;
        this->massPoint_z = massPoint_z;
        this->numPoints = numPoints;
    }

    void QefData::set(const QefData &rhs)
    {
        set(rhs.ata_00, rhs.ata_01, rhs.ata_02, rhs.ata_11, rhs.ata_12,
            rhs.ata_22, rhs.atb_x, rhs.atb_y, rhs.atb_z, rhs.btb,
            rhs.massPoint_x, rhs.massPoint_y, rhs.massPoint_z,
            rhs.numPoints);
    }

#ifndef NO_OSTREAM
    //std::ostream &operator<<(std::ostream &os, const QefData &qef)
    //{
    //    SMat3 ata;
    //    Vec3 atb, mp;
    //    ata.setSymmetric(qef.ata_00, qef.ata_01, qef.ata_02, qef.ata_11,
    //        qef.ata_12, qef.ata_22);
    //    atb.Set(qef.atb_x, qef.atb_y, qef.atb_z);
    //    mp.Set(qef.massPoint_x, qef.massPoint_y, qef.massPoint_z);
    //
    //    if (qef.numPoints > 0) {
    //        VecUtils::scale(mp, 1.0f / qef.numPoints);
    //    }
    //
    //    os << "QefData [ " << std::endl
    //        << " ata =" << std::endl << ata << "," << std::endl
    //        << " atb = " << atb << "," << std::endl
    //        << " btb = " << qef.btb << "," << std::endl
    //        << " massPoint = " << mp << "," << std::endl
    //        << " numPoints = " << qef.numPoints << "]";
    //    return os;
    //}
#endif

    QefSolver::QefSolver() : data(), ata(), atb(), massPoint(), x(),
        hasSolution(false) {}

    static void normalize(double &nx, double &ny, double &nz)
    {
        Vec3 tmpv(nx, ny, nz);
        VecUtils::normalize(tmpv);
        nx = tmpv.x;
        ny = tmpv.y;
        nz = tmpv.z;
    }

    void QefSolver::add(const double px, const double py, const double pz,
        double nx, double ny, double nz)
    {
        hasSolution = false;
        normalize(nx, ny, nz);
        data.ata_00 += nx * nx;
        data.ata_01 += nx * ny;
        data.ata_02 += nx * nz;
        data.ata_11 += ny * ny;
        data.ata_12 += ny * nz;
        data.ata_22 += nz * nz;
        const double dot = nx * px + ny * py + nz * pz;
        data.atb_x += dot * nx;
        data.atb_y += dot * ny;
        data.atb_z += dot * nz;
        data.btb += dot * dot;
        data.massPoint_x += px;
        data.massPoint_y += py;
        data.massPoint_z += pz;
        ++data.numPoints;
    }

    void QefSolver::add(const Vec3 &p, const Vec3 &n)
    {
        add(p.x, p.y, p.z, n.x, n.y, n.z);
    }

    void QefSolver::add(const QefData &rhs)
    {
        hasSolution = false;
        data.add(rhs);
    }

    QefData QefSolver::getData()
    {
        return data;
    }

    double QefSolver::getError()
    {
        if (!hasSolution) {
            throw std::runtime_error("illegal state");
        }

        return getError(x);
    }

    double QefSolver::getError(const Vec3 &pos)
    {
        if (!hasSolution) {
            setAta();
            setAtb();
        }

        Vec3 atax;
        MatUtils::vmul_symmetric(atax, ata, pos);
        return VecUtils::dot(pos, atax) - 2 * VecUtils::dot(pos, atb)
            + data.btb;
    }

    void QefSolver::reset()
    {
        hasSolution = false;
        data.clear();
    }

    void QefSolver::setAta()
    {
        ata.setSymmetric(data.ata_00, data.ata_01,
            data.ata_02, data.ata_11, data.ata_12,
            data.ata_22);
    }

    void QefSolver::setAtb()
    {
        atb.Set(data.atb_x, data.atb_y, data.atb_z);
    }

    double QefSolver::solve(Vec3 &outx, const double svd_tol,
        const int svd_sweeps, const double pinv_tol)
    {
        if (data.numPoints == 0) {
            throw std::invalid_argument("...");
        }

        massPoint.Set(data.massPoint_x, data.massPoint_y,
            data.massPoint_z);
        VecUtils::scale(massPoint, 1.0f / data.numPoints);
        setAta();
        setAtb();
        Vec3 tmpv;
        MatUtils::vmul_symmetric(tmpv, ata, massPoint);
        VecUtils::sub(atb, atb, tmpv);
        x.Set(0.0f,0.0f,0.0f);
        const double result = Svd::solveSymmetric(ata, atb,
            x, svd_tol, svd_sweeps, pinv_tol);
        VecUtils::addScaled(x, 1, massPoint);
        setAtb();
        outx = x;
        hasSolution = true;
        return result;
    }
}