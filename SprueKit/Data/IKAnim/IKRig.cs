using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace SprueKit.Data.IKAnim
{
    public class IKTriangle
    {
        int[] vertices_ = new int[3] { -1, -1, -1 };
        public int[] Vertices { get { return vertices_; } }

        public bool SharesVerts(IKTriangle t)
        {
            for (int i = 0; i < 3; ++i)
                for (int j = 0; j < 3; ++j)
                    if (vertices_[i] == t.vertices_[j])
                        return true;
            return false;
        }
    }

    public class DistLimit
    {
        public int VertexA { get; set; } = -1;
        public int VertexB { get; set; } = -1;
        public float MinDist { get; set; }
        public float MaxDist { get; set; }
    }

    public class RotLimit
    {
        public int TriA { get; set; } = -1;
        public int TriB { get; set; } = -1;
        public float MaxAngle { get; set; } = 180.0f;
        public Matrix3 Middle { get; set; } = new Matrix3();
    }

    public class RotFriction
    {
        public int TriA { get; set; } = -1;
        public int TriB { get; set; } = -1;
        public Matrix3 Middle { get; set; } = new Matrix3();
    }

    public class IKVert
    {
        public Vector3 Position { get; set; }
        public float Radius { get; set; }
        public float Weight { get; set; }
    }

    public class IKJoint
    {
        public int Bone { get; set; }
        public int Tri { get; set; }
        public int[] Verts { get; private set; } = new int[3] { -1, -1, -1 };
        public float Weight { get; set; } = 0;
        public Matrix Orient { get; set; } = Matrix.Identity;
    }

    public class RelJoint
    {
        public int Bone { get; set; }
        public int Parent { get; set; }
    }

    public class IKRig
    {
        public List<IKVert> verts = new List<IKVert>();
        public List<IKTriangle> tris = new List<IKTriangle>();
        public List<DistLimit> distlimits = new List<DistLimit>();
        public List<RotLimit> rotlimits = new List<RotLimit>();
        public List<RotFriction> rotfrictions = new List<RotFriction>();
        public List<IKJoint> joints = new List<IKJoint>();
        public List<RelJoint> reljoints = new List<RelJoint>();
        public bool IsLoaded { get; private set; } = false;

        public int Eye { get; set; } = -1;

        public void SetupJoints()
        {
            for (int i = 0; i < verts.Count; ++i)
                verts[i].Weight = 0;
            for (int i = 0; i < joints.Count; ++i)
            {
                IKJoint j = joints[i];
                j.Weight = 0;
                Vector3 pos = Vector3.Zero;
                for (int k = 0; k < 3; ++k)
                {
                    if (j.Verts[k] >= 0)
                    {
                        pos += verts[j.Verts[k]].Position;
                        j.Weight += 1.0f;
                        verts[j.Verts[k]].Weight += 1.0f;
                    }
                }
                if (j.Weight > 0.0f)
                    j.Weight = 1.0f / j.Weight;
                pos *= j.Weight;

                if (j.Tri >= 0)
                {
                    IKTriangle t = tris[j.Tri];
                    Matrix m = j.Orient;
                    Vector3 v1 = verts[t.Vertices[0]].Position;
                    Vector3 v2 = verts[t.Vertices[1]].Position;
                    Vector3 v3 = verts[t.Vertices[2]].Position;

                    Vector3 a = Vector3.Normalize((v2 - v1));
                    Vector3 c = Vector3.Normalize(Vector3.Cross(a, v3 - v1));
                    Vector3 b = Vector3.Cross(c, a);
                    Vector3 d = pos;

                    m = Matrix.CreateLookAt(d, v1, c);
                    j.Orient = m;
                }
            }
            for (int i = 0; i < verts.Count; ++i)
            {
                if (verts[i].Weight >= 0.0f)
                    verts[i].Weight = 1.0f / verts[i].Weight;
            }
            reljoints.Clear();
        }

        void SetupRotFrictions()
        {
            rotfrictions.Clear();
            for (int i = 0; i < tris.Count; ++i)
            {
                for (int j = i + 1; j < tris.Count; j++)
                {
                    if (tris[i].SharesVerts(tris[j]))
                    {
                        RotFriction r = new RotFriction();
                        rotfrictions.Add(r);
                        r.TriA = i;
                        r.TriB = j;
                    }
                }
            }
        }

        public void Setup()
        {
            SetupJoints();
            SetupRotFrictions();

            IsLoaded = true;
        }

        public void AddRelJoint(int bone, int parent)
        {
            RelJoint r = new RelJoint();
            reljoints.Add(r);
            r.Bone = bone;
            r.Parent = parent;
        }
    }

    public class RagdollData
    {
        public class vert
        {
            public Vector3 oldpos = Vector3.Zero;
            public Vector3 newPos = Vector3.Zero;
            public Vector3 pos= Vector3.Zero;
            public float weight = 0.0f;
            public bool collided = false;
            public bool stuck = true;
        };

        IKRig skel;
        int millis, collidemillis, collisions, floating, lastmove, unsticks;
        Vector3 offset = Vector3.Zero, center = Vector3.Zero;
        float radius = 0.0f, timestep = 0.0f, scale = 1.0f;
        List<vert> verts = new List<vert>();
        List<Matrix3> tris = new List<Matrix3>();
        //matrix4x3* animjoints;
        //dualquat* reljoints;

        void CalcTris()
        {
            for (int i = 0; i < skel.tris.Count; ++i)
            {             
                var t = skel.tris[i];

                Vector3 v1 = verts[t.Vertices[0]].pos;
                Vector3 v2 = verts[t.Vertices[1]].pos;
                Vector3 v3 = verts[t.Vertices[2]].pos;

                Vector3 a = Vector3.Normalize(v2 - v1);
                Vector3 c = Vector3.Normalize(Vector3.Cross(a, v3 - v1));
                Vector3 b = Vector3.Cross(c, a);

                tris[i] = XNAExt.CreateBasis(a, b, c);
                //??
            }
        }

        void CalcBoundsSphere()
        {
            center = Vector3.Zero;
            for (int i = 0; i < skel.verts.Count; ++i)
            {
                center += verts[i].pos;
            }
            center /= skel.verts.Count;
            radius = 0;
            for (int i = 0; i <skel.verts.Count; ++i)
                radius = Math.Max(radius, verts[i].pos.Distance(center));
        }

        public RagdollData(IKRig rig)
        {
            skel = rig;
            Init();
        }

        public void Init()
        {
            for (int i = 0; i < skel.verts.Count; ++i)
                verts[i].oldpos = verts[i].pos;

            CalcTris();
            CalcBoundsSphere();
        }

        void ConstrainDist()
        {
            float invscale = 1.0f / scale;
            for (int i = 0; i < skel.distlimits.Count; ++i)
            {
                var d = skel.distlimits[i];
                vert v1 = verts[d.VertexA];
                vert v2 = verts[d.VertexB];
                Vector3 dir = v2.pos - v1.pos;

                float dist = dir.Length() * invscale, cdist;
                if (dist < d.MinDist)
                    cdist = d.MinDist;
                else if (dist > d.MaxDist)
                    cdist = d.MaxDist;
                else
                    continue;
                if (dist > 1e-4f)
                    dir *= (cdist * 0.5f / dist);
                else
                    dir = new Vector3(0, 0, cdist * 0.5f / invscale);

                Vector3 center = (v1.pos + v2.pos) * 0.5f;
                v1.newPos += center - dir;;
                v1.weight++;
                v2.newPos += center + dir;
                v2.weight++;
            }
        }

        void ApplyRotLimit(IKTriangle t1, IKTriangle t2, float angle, Vector3 axis)
        {
            vert v1a = verts[t1.Vertices[0]], v1b = verts[t1.Vertices[1]], v1c = verts[t1.Vertices[2]],
                 v2a = verts[t2.Vertices[0]], v2b = verts[t2.Vertices[1]], v2c = verts[t2.Vertices[2]];
            Vector3 m1 = (v1a.pos + v1b.pos + v1c.pos) / 3,
                m2 = (v2a.pos + v2b.pos + v2c.pos) / 3,
                q1a = (Vector3.Cross(axis, v1a.pos) - m1), 
                q1b = (Vector3.Cross(axis, v1b.pos) - m1), 
                q1c = (Vector3.Cross(axis, v1c.pos) - m1), 
                q2a = (Vector3.Cross(axis, v2a.pos) - m2), 
                q2b = (Vector3.Cross(axis, v2b.pos) - m2), 
                q2c = (Vector3.Cross(axis, v2c.pos) - m2);
                float w1 = q1a.Length() +
                           q1b.Length() +
                           q1c.Length(),
                      w2 = q2a.Length() +
                           q2b.Length() +
                           q2c.Length();
                angle /= w1 + w2 + 1e-9f;
            float a1 = angle * w2, a2 = -angle * w1,
                  s1 = Mathf.Sin(a1), s2 = Mathf.Sin(a2);
                Vector3 c1 = axis * (1 - Mathf.Cos(a1)), 
                    c2 = axis * (1 - Mathf.Cos(a2));

            v1a.newPos += Vector3.Cross(c1, q1a) + (q1a * s1) + v1a.pos;
            v1a.weight++;
            v1b.newPos += Vector3.Cross(c1, q1b) + (q1b * s1) + v1b.pos;
            v1b.weight++;
            v1c.newPos += Vector3.Cross(c1, q1c) + (q1c * s1) + v1c.pos;
            v1c.weight++;
            v2a.newPos += Vector3.Cross(c2, q2a) + (q2a * s2) + v2a.pos;
            v2a.weight++;
            v2b.newPos += Vector3.Cross(c2, q2b) + (q2b * s2) + v2b.pos;
            v2b.weight++;
            v2c.newPos += Vector3.Cross(c2, q2c) + (q2c * s2) + v2c.pos;
            v2c.weight++;
        }

        void ConstrainRot()
        {
            for (int i = 0; i < skel.rotlimits.Count; ++i)
            {
                var r = skel.rotlimits[i];
                Matrix3 rot = Matrix3.Identity;
                rot = tris[r.TriA];
                rot.Mul(r.Middle);
                rot.MulTranspose(tris[r.TriB]);

                Vector3 axis;
                float angle;
                if (!rot.CalcAngleAxis(out angle, out axis))
                    continue;
                angle = r.MaxAngle - Mathf.Abs(angle);
                if (angle >= 0)
                    continue;
                angle += 1e-3f;

                ApplyRotLimit(skel.tris[r.TriA], skel.tris[r.TriB], angle, axis);
            }
        }

        void CalcRotFriction()
        {
            for (int i = 0; i < skel.rotfrictions.Count; ++i)
            {
                var r = skel.rotfrictions[i];
                r.Middle = tris[r.TriA];
                r.Middle.TransposeMul(tris[r.TriB]);
            }
        }

        public static int RagdollTimeStepMin = 5;
        public static int RagdollTimeStepMax = 10;
        public static float RagdollRotFric = 0.85f;
        public static float RagdollRotFricStop = 0.1f;

        void ApplyRotFriction(float ts)
        {
            CalcTris();
            float stopangle = 2 * Mathf.PI * ts * RagdollRotFricStop, rotfric = 1.0f - Mathf.Pow(RagdollRotFric, ts * 1000.0f / RagdollTimeStepMin);
            for (int i = 0; i < skel.rotfrictions.Count; ++i)
            {
                var r = skel.rotfrictions[i];
                Matrix3 rot = tris[r.TriA];
                rot.Mul(r.Middle);
                rot.MulTranspose(tris[r.TriB]);

                Vector3 axis;
                float angle;
                if (rot.CalcAngleAxis(out angle, out axis))
                {
                    angle *= -(Mathf.Abs(angle) >= stopangle ? rotfric : 1.0f);
                    ApplyRotLimit(skel.tris[r.TriA], skel.tris[r.TriB], angle, axis);
                }
            }
            for (int i = 0; i < skel.verts.Count; ++i)
            {
                vert v = verts[i];
                if (v.weight > 0.0f)
                    v.pos = v.newPos / v.weight;
                v.newPos = Vector3.Zero;
                v.weight = 0;
            }
        }

        void TryUnstick(float speed)
        {
            Vector3 unstuck = Vector3.Zero;
            int stuck = 0;
            for (int i = 0; i < skel.verts.Count; ++i)
            {
                vert v = verts[i];
                if (v.stuck)
                {
                    //??if (collidevert(v.pos, vec(0, 0, 0), skel->verts[i].radius)) {
                    //??    stuck++;
                    //??    continue;
                    //??}
                    v.stuck = false;
                }
                unstuck += v.pos;
            }
            unsticks = 0;
            if (stuck == 0 || stuck >= skel.verts.Count)
                return;
            unstuck /= (skel.verts.Count - stuck);
            for (int i = 0; i < skel.verts.Count; ++i)
            {
                vert v = verts[i];
                if (v.stuck)
                {
                    v.pos += Vector3.Normalize(unstuck - v.pos) * speed;
                    unsticks++;
                }
            }
        }

        public void UpdatePos()
        {
            for (int i = 0; i < skel.verts.Count; ++i)
            {
                vert v = verts[i];
                if (v.weight > 0)
                {
                    Vector3 collidewall = Vector3.UnitX;
                    v.newPos /= v.weight;
                    //if (!CollideVert(v.newPos, v.newPos - v.pos, skel.verts[i].Radius))
                    //    v.pos = v.newPos;
                    //else
                    {
                        Vector3 dir = v.newPos - v.oldpos;
                        if (Vector3.Dot(dir, collidewall) < 0)
                            v.oldpos = v.pos - Vector3.Reflect(dir, collidewall);
                        v.collided = true;
                    }
                }
                v.newPos = Vector3.Zero;
                v.weight = 0;
            }
        }

        static int RagdollConstrainIterations = 5;
        public void Constrain()
        {
            for (int i = 0; i < RagdollConstrainIterations; ++i)
            {
                CalcBoundsSphere();

                ConstrainDist();
                UpdatePos();

                CalcTris();
                ConstrainRot();
                UpdatePos();
            }
        }


    }
}
