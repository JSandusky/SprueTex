// Copyright 2006-2015 Tobias Sargeant (tobias.sargeant@gmail.com).
//
// This file is part of the Carve CSG Library (http://carve-csg.com/)
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
// ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#if defined(HAVE_CONFIG_H)
#include <carve_config.h>
#endif

#include <carve/csg.hpp>
#include <iostream>
#include "intersect_debug.hpp"

#if defined(CARVE_DEBUG_WRITE_PLY_DATA)
void writePLY(const std::string& out_file, const carve::mesh::MeshSet<3>* poly,
              bool ascii);
#endif

namespace carve {
namespace csg {
namespace {

class BaseCollector : public CSG::Collector {
  BaseCollector();
  BaseCollector(const BaseCollector&);
  BaseCollector& operator=(const BaseCollector&);

 protected:
  struct face_data_t {
    carve::mesh::MeshSet<3>::face_t* face;
    const carve::mesh::MeshSet<3>::face_t* orig_face;
    bool flipped;
    face_data_t(carve::mesh::MeshSet<3>::face_t* _face,
                const carve::mesh::MeshSet<3>::face_t* _orig_face,
                bool _flipped)
        : face(_face), orig_face(_orig_face), flipped(_flipped){};
  };

  std::list<face_data_t> faces;

  const carve::mesh::MeshSet<3>* src_a;
  const carve::mesh::MeshSet<3>* src_b;

  BaseCollector(const carve::mesh::MeshSet<3>* _src_a,
                const carve::mesh::MeshSet<3>* _src_b)
      : CSG::Collector(), src_a(_src_a), src_b(_src_b) {}

  ~BaseCollector() override {}

  void FWD(const carve::mesh::MeshSet<3>::face_t* orig_face,
           const std::vector<carve::mesh::MeshSet<3>::vertex_t*>& vertices,
           carve::geom3d::Vector /* normal */, bool /* poly_a */,
           FaceClass face_class, CSG::Hooks& hooks) {
    std::vector<carve::mesh::MeshSet<3>::face_t*> new_faces;
    new_faces.reserve(1);
    new_faces.push_back(
        orig_face->create(vertices.begin(), vertices.end(), false));
    hooks.processOutputFace(new_faces, orig_face, false);
    for (size_t i = 0; i < new_faces.size(); ++i) {
      faces.push_back(face_data_t(new_faces[i], orig_face, false));
    }

#if defined(CARVE_DEBUG) && defined(DEBUG_PRINT_RESULT_FACES)
    std::cerr << "+" << ENUM(face_class) << " ";
    for (unsigned i = 0; i < vertices.size(); ++i)
      std::cerr << " " << vertices[i] << ":" << *vertices[i];
    std::cerr << std::endl;
#endif
  }

  void REV(const carve::mesh::MeshSet<3>::face_t* orig_face,
           const std::vector<carve::mesh::MeshSet<3>::vertex_t*>& vertices,
           carve::geom3d::Vector /* normal */, bool /* poly_a */,
           FaceClass face_class, CSG::Hooks& hooks) {
    // normal = -normal;
    std::vector<carve::mesh::MeshSet<3>::face_t*> new_faces;
    new_faces.reserve(1);
    new_faces.push_back(
        orig_face->create(vertices.begin(), vertices.end(), true));
    hooks.processOutputFace(new_faces, orig_face, true);
    for (size_t i = 0; i < new_faces.size(); ++i) {
      faces.push_back(face_data_t(new_faces[i], orig_face, true));
    }

#if defined(CARVE_DEBUG) && defined(DEBUG_PRINT_RESULT_FACES)
    std::cerr << "-" << ENUM(face_class) << " ";
    for (unsigned i = 0; i < vertices.size(); ++i)
      std::cerr << " " << vertices[i] << ":" << *vertices[i];
    std::cerr << std::endl;
#endif
  }

  virtual void collect(
      const carve::mesh::MeshSet<3>::face_t* orig_face,
      const std::vector<carve::mesh::MeshSet<3>::vertex_t*>& vertices,
      carve::geom3d::Vector normal, bool poly_a, FaceClass face_class,
      CSG::Hooks& hooks) = 0;

  void collect(FaceLoopGroup* grp, CSG::Hooks& hooks) override {
    std::list<ClassificationInfo>& cinfo = (grp->classification);

    if (cinfo.size() == 0) {
      std::cerr << "WARNING! group " << grp << " has no classification info!"
                << std::endl;
      return;
    }

    FaceClass fc = FACE_UNCLASSIFIED;

    unsigned fc_closed_bits = 0;
    unsigned fc_open_bits = 0;
    unsigned fc_bits = 0;

    for (std::list<ClassificationInfo>::const_iterator
             i = grp->classification.begin(),
             e = grp->classification.end();
         i != e; ++i) {
      if ((*i).intersected_mesh == nullptr) {
        // classifier only returns global info
        fc_closed_bits = class_to_class_bit((*i).classification);
        break;
      }

      if ((*i).classification == FACE_UNCLASSIFIED) {
        continue;
      }
      if ((*i).intersectedMeshIsClosed()) {
        fc_closed_bits |= class_to_class_bit((*i).classification);
      } else {
        fc_open_bits |= class_to_class_bit((*i).classification);
      }
    }

    if (fc_closed_bits) {
      fc_bits = fc_closed_bits;
    } else {
      fc_bits = fc_open_bits;
    }

    fc = class_bit_to_class(fc_bits);

    // handle the complex cases where a group is classified differently with
    // respect to two or more closed manifolds.
    if (fc == FACE_UNCLASSIFIED) {
      unsigned inout_bits = fc_bits & FACE_NOT_ON_BIT;
      unsigned on_bits = fc_bits & FACE_ON_BIT;

      // both in and out. indicates an invalid manifold embedding.
      if (inout_bits == (FACE_IN_BIT | FACE_OUT_BIT)) {
        goto out;
      }

      // on, both orientations. could be caused by two manifolds touching at a
      // face.
      if (on_bits == (FACE_ON_ORIENT_IN_BIT | FACE_ON_ORIENT_OUT_BIT)) {
        goto out;
      }

      // in or out, but also on (with orientation). the on classification takes
      // precedence.
      fc = class_bit_to_class(on_bits);
    }

  out:

    if (fc == FACE_UNCLASSIFIED) {
      std::cerr << "group " << grp << " is unclassified!" << std::endl;

#if defined(CARVE_DEBUG_WRITE_PLY_DATA)
      static int uc_count = 0;

      std::vector<carve::mesh::MeshSet<3>::face_t*> faces;

      for (FaceLoop* f = grp->face_loops.head; f; f = f->next) {
        carve::mesh::MeshSet<3>::face_t* temp =
            f->orig_face->create(f->vertices.begin(), f->vertices.end(), false);
        faces.push_back(temp);
      }

      carve::mesh::MeshSet<3>* p = new carve::mesh::MeshSet<3>(faces);

      std::ostringstream filename;
      filename << "classifier_fail_" << ++uc_count << ".ply";
      std::string out(filename.str().c_str());
      ::writePLY(out, p, false);

      delete p;
#endif

      return;
    }

    bool is_poly_a = grp->src == src_a;

    for (FaceLoop* f = grp->face_loops.head; f; f = f->next) {
      collect(f->orig_face, f->vertices, f->orig_face->plane.N, is_poly_a, fc,
              hooks);
    }
  }

  carve::mesh::MeshSet<3>* done(CSG::Hooks& hooks) override {
    std::vector<carve::mesh::MeshSet<3>::face_t*> f;
    f.reserve(faces.size());
    for (std::list<face_data_t>::iterator i = faces.begin(); i != faces.end();
         ++i) {
      f.push_back((*i).face);
    }

    carve::mesh::MeshSet<3>* p = new carve::mesh::MeshSet<3>(f);

    if (hooks.hasHook(carve::csg::CSG::Hooks::RESULT_FACE_HOOK)) {
      for (std::list<face_data_t>::iterator i = faces.begin(); i != faces.end();
           ++i) {
        hooks.resultFace((*i).face, (*i).orig_face, (*i).flipped);
      }
    }

    return p;
  }
};

class AllCollector : public BaseCollector {
 public:
  AllCollector(const carve::mesh::MeshSet<3>* _src_a,
               const carve::mesh::MeshSet<3>* _src_b)
      : BaseCollector(_src_a, _src_b) {}
  ~AllCollector() override {}
  void collect(FaceLoopGroup* grp, CSG::Hooks& hooks) override {
    for (FaceLoop* f = grp->face_loops.head; f; f = f->next) {
      FWD(f->orig_face, f->vertices, f->orig_face->plane.N,
          f->orig_face->mesh->meshset == src_a, FACE_OUT, hooks);
    }
  }
  void collect(const carve::mesh::MeshSet<3>::face_t* orig_face,
               const std::vector<carve::mesh::MeshSet<3>::vertex_t*>& vertices,
               carve::geom3d::Vector normal, bool poly_a, FaceClass face_class,
               CSG::Hooks& hooks) override {
    FWD(orig_face, vertices, normal, poly_a, face_class, hooks);
  }
};

class UnionCollector : public BaseCollector {
 public:
  UnionCollector(const carve::mesh::MeshSet<3>* _src_a,
                 const carve::mesh::MeshSet<3>* _src_b)
      : BaseCollector(_src_a, _src_b) {}
  ~UnionCollector() override {}
  void collect(const carve::mesh::MeshSet<3>::face_t* orig_face,
               const std::vector<carve::mesh::MeshSet<3>::vertex_t*>& vertices,
               carve::geom3d::Vector normal, bool poly_a, FaceClass face_class,
               CSG::Hooks& hooks) override {
    if (face_class == FACE_OUT ||
        (poly_a && face_class == FACE_ON_ORIENT_OUT)) {
      FWD(orig_face, vertices, normal, poly_a, face_class, hooks);
    }
  }
};

class IntersectionCollector : public BaseCollector {
 public:
  IntersectionCollector(const carve::mesh::MeshSet<3>* _src_a,
                        const carve::mesh::MeshSet<3>* _src_b)
      : BaseCollector(_src_a, _src_b) {}
  ~IntersectionCollector() override {}
  void collect(const carve::mesh::MeshSet<3>::face_t* orig_face,
               const std::vector<carve::mesh::MeshSet<3>::vertex_t*>& vertices,
               carve::geom3d::Vector normal, bool poly_a, FaceClass face_class,
               CSG::Hooks& hooks) override {
    if (face_class == FACE_IN || (poly_a && face_class == FACE_ON_ORIENT_OUT)) {
      FWD(orig_face, vertices, normal, poly_a, face_class, hooks);
    }
  }
};

class SymmetricDifferenceCollector : public BaseCollector {
 public:
  SymmetricDifferenceCollector(const carve::mesh::MeshSet<3>* _src_a,
                               const carve::mesh::MeshSet<3>* _src_b)
      : BaseCollector(_src_a, _src_b) {}
  ~SymmetricDifferenceCollector() override {}
  void collect(const carve::mesh::MeshSet<3>::face_t* orig_face,
               const std::vector<carve::mesh::MeshSet<3>::vertex_t*>& vertices,
               carve::geom3d::Vector normal, bool poly_a, FaceClass face_class,
               CSG::Hooks& hooks) override {
    if (face_class == FACE_OUT) {
      FWD(orig_face, vertices, normal, poly_a, face_class, hooks);
    } else if (face_class == FACE_IN) {
      REV(orig_face, vertices, normal, poly_a, face_class, hooks);
    }
  }
};

class AMinusBCollector : public BaseCollector {
 public:
  AMinusBCollector(const carve::mesh::MeshSet<3>* _src_a,
                   const carve::mesh::MeshSet<3>* _src_b)
      : BaseCollector(_src_a, _src_b) {}
  ~AMinusBCollector() override {}
  void collect(const carve::mesh::MeshSet<3>::face_t* orig_face,
               const std::vector<carve::mesh::MeshSet<3>::vertex_t*>& vertices,
               carve::geom3d::Vector normal, bool poly_a, FaceClass face_class,
               CSG::Hooks& hooks) override {
    if ((face_class == FACE_OUT || face_class == FACE_ON_ORIENT_IN) && poly_a) {
      FWD(orig_face, vertices, normal, poly_a, face_class, hooks);
    } else if (face_class == FACE_IN && !poly_a) {
      REV(orig_face, vertices, normal, poly_a, face_class, hooks);
    }
  }
};

class AMinusB_AOnly_Collector : public BaseCollector {
public:
    AMinusB_AOnly_Collector(const carve::mesh::MeshSet<3>* _src_a,
        const carve::mesh::MeshSet<3>* _src_b)
        : BaseCollector(_src_a, _src_b) {}
    ~AMinusB_AOnly_Collector() override {}
    void collect(const carve::mesh::MeshSet<3>::face_t* orig_face,
        const std::vector<carve::mesh::MeshSet<3>::vertex_t*>& vertices,
        carve::geom3d::Vector normal, bool poly_a, FaceClass face_class,
        CSG::Hooks& hooks) override {
        if ((face_class == FACE_OUT || face_class == FACE_ON_ORIENT_IN) && poly_a)
            FWD(orig_face, vertices, normal, poly_a, face_class, hooks);
    }
};

class BMinusACollector : public BaseCollector {
 public:
  BMinusACollector(const carve::mesh::MeshSet<3>* _src_a,
                   const carve::mesh::MeshSet<3>* _src_b)
      : BaseCollector(_src_a, _src_b) {}
  ~BMinusACollector() override {}
  void collect(const carve::mesh::MeshSet<3>::face_t* orig_face,
               const std::vector<carve::mesh::MeshSet<3>::vertex_t*>& vertices,
               carve::geom3d::Vector normal, bool poly_a, FaceClass face_class,
               CSG::Hooks& hooks) override {
    if ((face_class == FACE_OUT || face_class == FACE_ON_ORIENT_IN) &&
        !poly_a) {
      FWD(orig_face, vertices, normal, poly_a, face_class, hooks);
    } else if (face_class == FACE_IN && poly_a) {
      REV(orig_face, vertices, normal, poly_a, face_class, hooks);
    }
  }
};

class BMinusA_BOnly_Collector : public BaseCollector {
public:
    BMinusA_BOnly_Collector(const carve::mesh::MeshSet<3>* _src_a,
        const carve::mesh::MeshSet<3>* _src_b)
        : BaseCollector(_src_a, _src_b) {}
    ~BMinusA_BOnly_Collector() override {}
    void collect(const carve::mesh::MeshSet<3>::face_t* orig_face,
        const std::vector<carve::mesh::MeshSet<3>::vertex_t*>& vertices,
        carve::geom3d::Vector normal, bool poly_a, FaceClass face_class,
        CSG::Hooks& hooks) override {
        if ((face_class == FACE_OUT || face_class == FACE_ON_ORIENT_IN) && !poly_a)
            FWD(orig_face, vertices, normal, poly_a, face_class, hooks);
    }
};

}  // namespace

CSG::Collector* makeCollector(CSG::OP op, const carve::mesh::MeshSet<3>* poly_a,
                              const carve::mesh::MeshSet<3>* poly_b) {
  switch (op) {
    case CSG::UNION:
      return new UnionCollector(poly_a, poly_b);
    case CSG::INTERSECTION:
      return new IntersectionCollector(poly_a, poly_b);
    case CSG::A_MINUS_B:
      return new AMinusBCollector(poly_a, poly_b);
    case CSG::A_MINUS_B_ONLY_A:
        return new AMinusB_AOnly_Collector(poly_a, poly_b);
    case CSG::B_MINUS_A:
      return new BMinusACollector(poly_a, poly_b);
    case CSG::B_MINUS_A_ONLY_B:
        return new BMinusA_BOnly_Collector(poly_a, poly_b);
    case CSG::SYMMETRIC_DIFFERENCE:
      return new SymmetricDifferenceCollector(poly_a, poly_b);
    case CSG::ALL:
      return new AllCollector(poly_a, poly_b);
  }
  return nullptr;
}
}  // namespace csg
}  // namespace carve
