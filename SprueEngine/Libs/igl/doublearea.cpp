// This file is part of libigl, a simple c++ geometry processing library.
//
// Copyright (C) 2013 Alec Jacobson <alecjacobson@gmail.com>
//
// This Source Code Form is subject to the terms of the Mozilla Public License
// v. 2.0. If a copy of the MPL was not distributed with this file, You can
// obtain one at http://mozilla.org/MPL/2.0/.
#include "doublearea.h"
#include "edge_lengths.h"
#include "parallel_for.h"
#include "sort.h"
#include <cassert>
#include <iostream>

template <typename DerivedV, typename DerivedF, typename DeriveddblA>
IGL_INLINE void igl::doublearea(
  const Eigen::PlainObjectBase<DerivedV> & V,
  const Eigen::PlainObjectBase<DerivedF> & F,
  Eigen::PlainObjectBase<DeriveddblA> & dblA)
{
  if (F.cols() == 4) // quads are handled by a specialized function
    return doublearea_quad(V,F,dblA);

  const int dim = V.cols();
  // Only support triangles
  assert(F.cols() == 3);
  const size_t m = F.rows();
  // Compute edge lengths
  Eigen::Matrix<typename DerivedV::Scalar, Eigen::Dynamic, 3> l;

  // Projected area helper
  const auto & proj_doublearea =
    [&V,&F](const int x, const int y, const int f)->double
  {
    auto rx = V(F(f,0),x)-V(F(f,2),x);
    auto sx = V(F(f,1),x)-V(F(f,2),x);
    auto ry = V(F(f,0),y)-V(F(f,2),y);
    auto sy = V(F(f,1),y)-V(F(f,2),y);
    return rx*sy - ry*sx;
  };

  switch(dim)
  {
    case 3:
    {
      dblA = DeriveddblA::Zero(m,1);
      for(size_t f = 0;f<m;f++)
      {
        for(int d = 0;d<3;d++)
        {
          double dblAd = proj_doublearea(d,(d+1)%3,f);
          dblA(f) += dblAd*dblAd;
        }
      }
      dblA = dblA.array().sqrt().eval();
      break;
    }
    case 2:
    {
      dblA.resize(m,1);
      for(size_t f = 0;f<m;f++)
      {
        dblA(f) = proj_doublearea(0,1,f);
      }
      break;
    }
    default:
    {
      edge_lengths(V,F,l);
      return doublearea(l,dblA);
    }
  }
}


template <
  typename DerivedA,
  typename DerivedB,
  typename DerivedC,
  typename DerivedD>
IGL_INLINE void igl::doublearea(
  const Eigen::PlainObjectBase<DerivedA> & A,
  const Eigen::PlainObjectBase<DerivedB> & B,
  const Eigen::PlainObjectBase<DerivedC> & C,
  Eigen::PlainObjectBase<DerivedD> & D)
{
  assert((B.cols() == A.cols()) && "dimensions of A and B should match");
  assert((C.cols() == A.cols()) && "dimensions of A and C should match");
  assert(A.rows() == B.rows() && "corners should have same length");
  assert(A.rows() == C.rows() && "corners should have same length");
  switch(A.cols())
  {
    case 2:
    {
      // For 2d compute signed area
      const auto & R = A-C;
      const auto & S = B-C;
      D = R.col(0).array()*S.col(1).array() - R.col(1).array()*S.col(0).array();
      break;
    }
    default:
    {
      Eigen::Matrix<typename DerivedD::Scalar,DerivedD::RowsAtCompileTime,3>
        uL(A.rows(),3);
      uL.col(0) = (B-C).rowwise().norm();
      uL.col(1) = (C-A).rowwise().norm();
      uL.col(2) = (A-B).rowwise().norm();
      doublearea(uL,D);
    }
  }
}

template <
  typename DerivedA,
  typename DerivedB,
  typename DerivedC>
IGL_INLINE typename DerivedA::Scalar igl::doublearea_single(
  const Eigen::PlainObjectBase<DerivedA> & A,
  const Eigen::PlainObjectBase<DerivedB> & B,
  const Eigen::PlainObjectBase<DerivedC> & C)
{
  assert(A.size() == 2 && "Vertices should be 2D");
  assert(B.size() == 2 && "Vertices should be 2D");
  assert(C.size() == 2 && "Vertices should be 2D");
  auto r = A-C;
  auto s = B-C;
  return r(0)*s(1) - r(1)*s(0);
}

template <typename Derivedl, typename DeriveddblA>
IGL_INLINE void igl::doublearea(
  const Eigen::PlainObjectBase<Derivedl> & ul,
  Eigen::PlainObjectBase<DeriveddblA> & dblA)
{
  using namespace Eigen;
  using namespace std;
  typedef typename Derivedl::Index Index;
  // Only support triangles
  assert(ul.cols() == 3);
  // Number of triangles
  const Index m = ul.rows();
  Eigen::Matrix<typename Derivedl::Scalar, Eigen::Dynamic, 3> l;
  MatrixXi _;
  // "Lecture Notes on Geometric Robustness" Shewchuck 09, Section 3.1
  // http://www.cs.berkeley.edu/~jrs/meshpapers/robnotes.pdf
  //
  // "Miscalculating Area and Angles of a Needle-like Triangle"
  // https://people.eecs.berkeley.edu/~wkahan/Triangle.pdf
  igl::sort(ul,2,false,l,_);
  // semiperimeters
  //Matrix<typename Derivedl::Scalar,Dynamic,1> s = l.rowwise().sum()*0.5;
  //assert((Index)s.rows() == m);
  // resize output
  dblA.resize(l.rows(),1);
  parallel_for(
    m,
    [&l,&dblA](const int i)
    {
      // Kahan's Heron's formula
      const typename Derivedl::Scalar arg =
        (l(i,0)+(l(i,1)+l(i,2)))*
        (l(i,2)-(l(i,0)-l(i,1)))*
        (l(i,2)+(l(i,0)-l(i,1)))*
        (l(i,0)+(l(i,1)-l(i,2)));
      dblA(i) = 2.0*0.25*sqrt(arg);
      //JS assert( l(i,2) - (l(i,0)-l(i,1)) && "FAILED KAHAN'S ASSERTION");
      //JS assert(dblA(i) == dblA(i) && "DOUBLEAREA() PRODUCED NaN");
    },
    1000l);
}

template <typename DerivedV, typename DerivedF, typename DeriveddblA>
IGL_INLINE void igl::doublearea_quad(
const Eigen::PlainObjectBase<DerivedV> & V,
const Eigen::PlainObjectBase<DerivedF> & F,
Eigen::PlainObjectBase<DeriveddblA> & dblA)
{
  assert(V.cols() == 3); // Only supports points in 3D
  assert(F.cols() == 4); // Only support quads
  const size_t m = F.rows();

  // Split the quads into triangles
  Eigen::MatrixXi Ft(F.rows()*2,3);

  for(size_t i=0; i<m;++i)
  {
    Ft.row(i*2    ) << F(i,0), F(i,1), F(i,2);
    Ft.row(i*2 + 1) << F(i,2), F(i,3), F(i,0);
  }

  // Compute areas
  Eigen::VectorXd doublearea_tri;
  igl::doublearea(V,Ft,doublearea_tri);

  dblA.resize(F.rows(),1);
  for(unsigned i=0; i<F.rows();++i)
    dblA(i) = doublearea_tri(i*2) + doublearea_tri(i*2 + 1);

}

#ifdef IGL_STATIC_LIBRARY
// Explicit template specialization
// generated by autoexplicit.sh
template void igl::doublearea<Eigen::Matrix<double, -1, -1, 0, -1, -1>, Eigen::Matrix<double, -1, -1, 0, -1, -1>, Eigen::Matrix<double, -1, -1, 0, -1, -1>, Eigen::Matrix<double, -1, 1, 0, -1, 1> >(Eigen::PlainObjectBase<Eigen::Matrix<double, -1, -1, 0, -1, -1> > const&, Eigen::PlainObjectBase<Eigen::Matrix<double, -1, -1, 0, -1, -1> > const&, Eigen::PlainObjectBase<Eigen::Matrix<double, -1, -1, 0, -1, -1> > const&, Eigen::PlainObjectBase<Eigen::Matrix<double, -1, 1, 0, -1, 1> >&);
// generated by autoexplicit.sh
template void igl::doublearea<Eigen::Matrix<float, -1, 3, 1, -1, 3>, Eigen::Matrix<unsigned int, -1, 3, 1, -1, 3>, Eigen::Matrix<float, -1, 1, 0, -1, 1> >(Eigen::PlainObjectBase<Eigen::Matrix<float, -1, 3, 1, -1, 3> > const&, Eigen::PlainObjectBase<Eigen::Matrix<unsigned int, -1, 3, 1, -1, 3> > const&, Eigen::PlainObjectBase<Eigen::Matrix<float, -1, 1, 0, -1, 1> >&);
// generated by autoexplicit.sh
template void igl::doublearea<Eigen::Matrix<double, -1, 3, 0, -1, 3>, Eigen::Matrix<double, -1, 1, 0, -1, 1> >(Eigen::PlainObjectBase<Eigen::Matrix<double, -1, 3, 0, -1, 3> > const&, Eigen::PlainObjectBase<Eigen::Matrix<double, -1, 1, 0, -1, 1> >&);
// generated by autoexplicit.sh
template void igl::doublearea<Eigen::Matrix<float, -1, 3, 1, -1, 3>, Eigen::Matrix<unsigned int, -1, -1, 1, -1, -1>, Eigen::Matrix<double, -1, 1, 0, -1, 1> >(Eigen::PlainObjectBase<Eigen::Matrix<float, -1, 3, 1, -1, 3> > const&, Eigen::PlainObjectBase<Eigen::Matrix<unsigned int, -1, -1, 1, -1, -1> > const&, Eigen::PlainObjectBase<Eigen::Matrix<double, -1, 1, 0, -1, 1> >&);
template void igl::doublearea<Eigen::Matrix<double, -1, 3, 1, -1, 3>, Eigen::Matrix<unsigned int, -1, -1, 1, -1, -1>, Eigen::Matrix<double, -1, 1, 0, -1, 1> >(Eigen::PlainObjectBase<Eigen::Matrix<double, -1, 3, 1, -1, 3> > const&, Eigen::PlainObjectBase<Eigen::Matrix<unsigned int, -1, -1, 1, -1, -1> > const&, Eigen::PlainObjectBase<Eigen::Matrix<double, -1, 1, 0, -1, 1> >&);
template void igl::doublearea<Eigen::Matrix<double, -1, -1, 0, -1, -1>, Eigen::Matrix<int, -1, -1, 0, -1, -1>, Eigen::Matrix<double, -1, 1, 0, -1, 1> >(Eigen::PlainObjectBase<Eigen::Matrix<double, -1, -1, 0, -1, -1> > const&, Eigen::PlainObjectBase<Eigen::Matrix<int, -1, -1, 0, -1, -1> > const&, Eigen::PlainObjectBase<Eigen::Matrix<double, -1, 1, 0, -1, 1> >&);
template void igl::doublearea<Eigen::Matrix<double, -1, -1, 0, -1, -1>, Eigen::Matrix<double, -1, 1, 0, -1, 1> >(Eigen::PlainObjectBase<Eigen::Matrix<double, -1, -1, 0, -1, -1> > const&, Eigen::PlainObjectBase<Eigen::Matrix<double, -1, 1, 0, -1, 1> >&);
template void igl::doublearea<Eigen::Matrix<double, -1, -1, 0, -1, -1>, Eigen::Matrix<int, -1, -1, 0, -1, -1>, Eigen::Matrix<int, -1, 1, 0, -1, 1> >(Eigen::PlainObjectBase<Eigen::Matrix<double, -1, -1, 0, -1, -1> > const&, Eigen::PlainObjectBase<Eigen::Matrix<int, -1, -1, 0, -1, -1> > const&, Eigen::PlainObjectBase<Eigen::Matrix<int, -1, 1, 0, -1, 1> >&);
template void igl::doublearea<Eigen::Matrix<double, -1, 3, 0, -1, 3>, Eigen::Matrix<int, -1, 3, 0, -1, 3>, Eigen::Matrix<double, -1, 1, 0, -1, 1> >(Eigen::PlainObjectBase<Eigen::Matrix<double, -1, 3, 0, -1, 3> > const&, Eigen::PlainObjectBase<Eigen::Matrix<int, -1, 3, 0, -1, 3> > const&, Eigen::PlainObjectBase<Eigen::Matrix<double, -1, 1, 0, -1, 1> >&);
template Eigen::Matrix<double, 2, 1, 0, 2, 1>::Scalar igl::doublearea_single<Eigen::Matrix<double, 2, 1, 0, 2, 1>, Eigen::Matrix<double, 2, 1, 0, 2, 1>, Eigen::Matrix<double, 2, 1, 0, 2, 1> >(Eigen::PlainObjectBase<Eigen::Matrix<double, 2, 1, 0, 2, 1> > const&, Eigen::PlainObjectBase<Eigen::Matrix<double, 2, 1, 0, 2, 1> > const&, Eigen::PlainObjectBase<Eigen::Matrix<double, 2, 1, 0, 2, 1> > const&);
template void igl::doublearea<Eigen::Matrix<double, -1, 3, 0, -1, 3>, Eigen::Matrix<int, -1, 3, 0, -1, 3>, Eigen::Matrix<double, -1, -1, 0, -1, -1> >(Eigen::PlainObjectBase<Eigen::Matrix<double, -1, 3, 0, -1, 3> > const&, Eigen::PlainObjectBase<Eigen::Matrix<int, -1, 3, 0, -1, 3> > const&, Eigen::PlainObjectBase<Eigen::Matrix<double, -1, -1, 0, -1, -1> >&);
template void igl::doublearea<Eigen::Matrix<double, -1, -1, 0, -1, -1>, Eigen::Matrix<int, -1, -1, 0, -1, -1>, Eigen::Matrix<double, -1, -1, 0, -1, -1> >(Eigen::PlainObjectBase<Eigen::Matrix<double, -1, -1, 0, -1, -1> > const&, Eigen::PlainObjectBase<Eigen::Matrix<int, -1, -1, 0, -1, -1> > const&, Eigen::PlainObjectBase<Eigen::Matrix<double, -1, -1, 0, -1, -1> >&);
template void igl::doublearea<Eigen::Matrix<double, 1, 3, 1, 1, 3>, Eigen::Matrix<double, 1, 3, 1, 1, 3>, Eigen::Matrix<double, 1, 3, 1, 1, 3>, Eigen::Matrix<double, 1, 1, 0, 1, 1> >(Eigen::PlainObjectBase<Eigen::Matrix<double, 1, 3, 1, 1, 3> > const&, Eigen::PlainObjectBase<Eigen::Matrix<double, 1, 3, 1, 1, 3> > const&, Eigen::PlainObjectBase<Eigen::Matrix<double, 1, 3, 1, 1, 3> > const&, Eigen::PlainObjectBase<Eigen::Matrix<double, 1, 1, 0, 1, 1> >&);
template void igl::doublearea<Eigen::Matrix<double, -1, -1, 0, -1, -1>, Eigen::Matrix<double, -1, -1, 0, -1, -1> >(Eigen::PlainObjectBase<Eigen::Matrix<double, -1, -1, 0, -1, -1> > const&, Eigen::PlainObjectBase<Eigen::Matrix<double, -1, -1, 0, -1, -1> >&);
template void igl::doublearea<Eigen::Matrix<double, -1, -1, 0, -1, -1>, Eigen::Matrix<double, -1, -1, 0, -1, -1>, Eigen::Matrix<double, -1, -1, 0, -1, -1>, Eigen::Matrix<double, -1, -1, 0, -1, -1> >(Eigen::PlainObjectBase<Eigen::Matrix<double, -1, -1, 0, -1, -1> > const&, Eigen::PlainObjectBase<Eigen::Matrix<double, -1, -1, 0, -1, -1> > const&, Eigen::PlainObjectBase<Eigen::Matrix<double, -1, -1, 0, -1, -1> > const&, Eigen::PlainObjectBase<Eigen::Matrix<double, -1, -1, 0, -1, -1> >&);
template Eigen::Matrix<double, -1, -1, 0, -1, -1>::Scalar igl::doublearea_single<Eigen::Matrix<double, -1, -1, 0, -1, -1>, Eigen::Matrix<double, -1, -1, 0, -1, -1>, Eigen::Matrix<double, -1, -1, 0, -1, -1> >(Eigen::PlainObjectBase<Eigen::Matrix<double, -1, -1, 0, -1, -1> > const&, Eigen::PlainObjectBase<Eigen::Matrix<double, -1, -1, 0, -1, -1> > const&, Eigen::PlainObjectBase<Eigen::Matrix<double, -1, -1, 0, -1, -1> > const&);
template void igl::doublearea<Eigen::Matrix<double, -1, 3, 1, -1, 3>, Eigen::Matrix<int, -1, 3, 1, -1, 3>, Eigen::Matrix<double, -1, 1, 0, -1, 1> >(Eigen::PlainObjectBase<Eigen::Matrix<double, -1, 3, 1, -1, 3> > const&, Eigen::PlainObjectBase<Eigen::Matrix<int, -1, 3, 1, -1, 3> > const&, Eigen::PlainObjectBase<Eigen::Matrix<double, -1, 1, 0, -1, 1> >&);
#endif
