using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Data.Processing
{
    /// <summary>
    /// CPU-side NaiveSurfaceNets
    /// </summary>
    public class NaiveSurfaceNets
    {
        static Vector3[] SamplingOffsets =
        {
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(1, 1, 0),
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 1),
            new Vector3(0, 1, 1),
            new Vector3(1, 1, 1)
        };

        public List<int> IndexBuffer { get; set; } = new List<int>();
        public List<PluginLib.VertexData> VertexBuffer { get; set; } = new List<PluginLib.VertexData>();

        public NaiveSurfaceNets(IHaveDensity densitySource, Vector3 offset)
        {
            densitySource.PrepareDensity();

            int BLOCK_SIZE = 64;
            int CELL_SIZE = BLOCK_SIZE + 1;

            Stopwatch timer = new Stopwatch();
            timer.Start();
            float[] densities = new float[CELL_SIZE * CELL_SIZE * CELL_SIZE];
            int[] inds = new int[CELL_SIZE * CELL_SIZE * 2];

            Vector3 size = new Vector3(CELL_SIZE);
            Vector3 position = new Vector3();
            Vector3 samplePosition = new Vector3();

            // Calculate densities
            for (int z = 0; z < CELL_SIZE; ++z)
            {
                position.Z = z;
                samplePosition.Z = z + offset.Z;
                for (int y = 0; y < CELL_SIZE; ++y)
                {
                    position.Y = y;
                    samplePosition.Y = y + offset.Y;
                    for (int x = 0; x < CELL_SIZE; ++x)
                    {
                        position.X = x;
                        samplePosition.X = x + offset.X;
                        densities[offset_3d(position, size)] = densitySource.GetDensity(samplePosition);
                    }
                }
            }

            for (int z = 0; z < BLOCK_SIZE; ++z)
            {
                position.Z = z;
                for (int y = 0; y < BLOCK_SIZE; ++y)
                {
                    position.Y = y ;
                    for (int x = 0; x < BLOCK_SIZE; ++x)
                    {
                        position.X = x;
                        float[] density = {
                            densities[offset_3d(position, size)],
                            densities[offset_3d(position + SamplingOffsets[0], size)],
                            densities[offset_3d(position + SamplingOffsets[1], size)],
                            densities[offset_3d(position + SamplingOffsets[2], size)],
                            densities[offset_3d(position + SamplingOffsets[3], size)],
                            densities[offset_3d(position + SamplingOffsets[4], size)],
                            densities[offset_3d(position + SamplingOffsets[5], size)],
                            densities[offset_3d(position + SamplingOffsets[6], size)]
                        };

                        int layout = ((density[0] < 0.0f ? 1 : 0) << 0) |
                            ((density[1] < 0.0f ? 1 : 0) << 1) |
                            ((density[2] < 0.0f ? 1 : 0) << 2) |
                            ((density[3] < 0.0f ? 1 : 0) << 3) |
                            ((density[4] < 0.0f ? 1 : 0) << 4) |
                            ((density[5] < 0.0f ? 1 : 0) << 5) |
                            ((density[6] < 0.0f ? 1 : 0) << 6) |
                            ((density[7] < 0.0f ? 1 : 0) << 7);

                        if (layout == 0 || layout == 255)
                            continue;

                        Vector3 average = new Vector3();
                        int average_n = 0;
                        do_edge(ref average, ref average_n, density[0], density[1], 0, new Vector3(x, y, z));
                        do_edge(ref average, ref average_n, density[2], density[3], 0, new Vector3(x, y + 1, z));
                        do_edge(ref average, ref average_n, density[4], density[5], 0, new Vector3(x, y, z + 1));
                        do_edge(ref average, ref average_n, density[6], density[7], 0, new Vector3(x, y + 1, z + 1));
                        do_edge(ref average, ref average_n, density[0], density[2], 1, new Vector3(x, y, z));
                        do_edge(ref average, ref average_n, density[1], density[3], 1, new Vector3(x + 1, y, z));
                        do_edge(ref average, ref average_n, density[4], density[6], 1, new Vector3(x, y, z + 1));
                        do_edge(ref average, ref average_n, density[5], density[7], 1, new Vector3(x + 1, y, z + 1));
                        do_edge(ref average, ref average_n, density[0], density[4], 2, new Vector3(x, y, z));
                        do_edge(ref average, ref average_n, density[1], density[5], 2, new Vector3(x + 1, y, z));
                        do_edge(ref average, ref average_n, density[2], density[6], 2, new Vector3(x, y + 1, z));
                        do_edge(ref average, ref average_n, density[3], density[7], 2, new Vector3(x + 1, y + 1, z));

                        average /= (float)average_n;

                        Vector3 insertion = average + offset;
                        //insertion.X = Mathf.Clamp(insertion.X, x + Mathf.LARGE_EPSILON + offset.X, x + 1 - Mathf.LARGE_EPSILON + offset.X);
                        //insertion.Y = Mathf.Clamp(insertion.Y, y + Mathf.LARGE_EPSILON + offset.Y, y + 1 - Mathf.LARGE_EPSILON + offset.Y);
                        //insertion.Z = Mathf.Clamp(insertion.Z, z + Mathf.LARGE_EPSILON + offset.Z, z + 1 - Mathf.LARGE_EPSILON + offset.Z);
                        Vector3 normal = Vector3.Normalize(insertion);// CalculateSurfaceNormal(densitySource, insertion);
                        //insertion += normal * Mathf.Clamp(-densitySource.GetDensity(insertion), -0.5f, 0.5f);

                        inds[offset_3d_slab(position, size)] = VertexBuffer.Count;
                        VertexBuffer.Add(new PluginLib.VertexData(insertion, normal, Vector2.Zero));

                        bool flip = density[0] < 0.0f;
                        if (position.Y > 0 && position.Z > 0 && (density[0] < 0.0f) != (density[1] < 0.0f))
                        {
                            quad(flip,
                                inds[offset_3d_slab(new Vector3(position.X, position.Y, position.Z), size)],
                                inds[offset_3d_slab(new Vector3(position.X, position.Y, position.Z - 1), size)],
                                inds[offset_3d_slab(new Vector3(position.X, position.Y - 1, position.Z - 1), size)],
                                inds[offset_3d_slab(new Vector3(position.X, position.Y - 1, position.Z), size)]
                                );
                        }
                        if (position.X > 0 && position.Z > 0 && (density[0] < 0.0f) != (density[2] < 0.0f))
                        {
                            quad(flip,
                                inds[offset_3d_slab(new Vector3(position.X, position.Y, position.Z), size)],
                                inds[offset_3d_slab(new Vector3(position.X - 1, position.Y, position.Z), size)],
                                inds[offset_3d_slab(new Vector3(position.X - 1, position.Y, position.Z - 1), size)],
                                inds[offset_3d_slab(new Vector3(position.X, position.Y, position.Z - 1), size)]
                                );
                        }
                        if (position.X > 0 && position.Y > 0 && (density[0] < 0.0f) != (density[4] < 0.0f))
                        {
                            quad(flip,
                                inds[offset_3d_slab(new Vector3(position.X, position.Y, position.Z), size)],
                                inds[offset_3d_slab(new Vector3(position.X, position.Y - 1, position.Z), size)],
                                inds[offset_3d_slab(new Vector3(position.X - 1, position.Y - 1, position.Z), size)],
                                inds[offset_3d_slab(new Vector3(position.X - 1, position.Y, position.Z), size)]
                                );
                        }
                    }
                }
            }

            foreach (var vert in VertexBuffer)
                vert.Normal.Normalize();

            timer.Stop();
            ErrorHandler.inst().PublishError(string.Format("Generated mesh in {0}", timer.Elapsed.ToString()), 4);
        }

        public NaiveSurfaceNets(float[] densities, Vector4[] positions, byte[] corners, IHaveDensity densitySource)
        {
            int BLOCK_SIZE = 64;
            int CELL_SIZE = BLOCK_SIZE + 1;

            Stopwatch timer = new Stopwatch();
            timer.Start();
            int[] inds = new int[CELL_SIZE * CELL_SIZE * 2];

            Vector3 blockSize = new Vector3(BLOCK_SIZE);
            Vector3 size = new Vector3(CELL_SIZE);
            Vector3 position = new Vector3();
            Vector3 samplePosition = new Vector3();

            for (int z = 0; z < BLOCK_SIZE; ++z)
            {
                position.Z = z;
                for (int y = 0; y < BLOCK_SIZE; ++y)
                {
                    position.Y = y;
                    for (int x = 0; x < BLOCK_SIZE; ++x)
                    {
                        position.X = x;

                        int cellPos = offset_3d(position, blockSize);
                        int layout = corners[cellPos];

                        if (layout == 0 || layout == 255)
                            continue;

                        float[] density = {
                            densities[offset_3d(position, size)],
                            densities[offset_3d(position + SamplingOffsets[0], size)],
                            densities[offset_3d(position + SamplingOffsets[1], size)],
                            densities[offset_3d(position + SamplingOffsets[2], size)],
                            densities[offset_3d(position + SamplingOffsets[3], size)],
                            densities[offset_3d(position + SamplingOffsets[4], size)],
                            densities[offset_3d(position + SamplingOffsets[5], size)],
                            densities[offset_3d(position + SamplingOffsets[6], size)]
                        };

                        Vector4 posPt = positions[cellPos];
                        Vector3 insertion = new Vector3(posPt.X / posPt.W, posPt.Y / posPt.W, posPt.Z / posPt.W);
                        //insertion.X = Mathf.Clamp(insertion.X, x + Mathf.LARGE_EPSILON + offset.X, x + 1 - Mathf.LARGE_EPSILON + offset.X);
                        //insertion.Y = Mathf.Clamp(insertion.Y, y + Mathf.LARGE_EPSILON + offset.Y, y + 1 - Mathf.LARGE_EPSILON + offset.Y);
                        //insertion.Z = Mathf.Clamp(insertion.Z, z + Mathf.LARGE_EPSILON + offset.Z, z + 1 - Mathf.LARGE_EPSILON + offset.Z);
                        Vector3 normal = CalculateSurfaceNormal(densitySource, insertion);

                        inds[offset_3d_slab(position, size)] = VertexBuffer.Count;
                        VertexBuffer.Add(new PluginLib.VertexData(insertion, normal, Vector2.Zero));

                        bool flip = density[0] < 0.0f;
                        if (position.Y > 0 && position.Z > 0 && (density[0] < 0.0f) != (density[1] < 0.0f))
                        {
                            quad(flip,
                                inds[offset_3d_slab(new Vector3(position.X, position.Y, position.Z), size)],
                                inds[offset_3d_slab(new Vector3(position.X, position.Y, position.Z - 1), size)],
                                inds[offset_3d_slab(new Vector3(position.X, position.Y - 1, position.Z - 1), size)],
                                inds[offset_3d_slab(new Vector3(position.X, position.Y - 1, position.Z), size)]
                                );
                        }
                        if (position.X > 0 && position.Z > 0 && (density[0] < 0.0f) != (density[2] < 0.0f))
                        {
                            quad(flip,
                                inds[offset_3d_slab(new Vector3(position.X, position.Y, position.Z), size)],
                                inds[offset_3d_slab(new Vector3(position.X - 1, position.Y, position.Z), size)],
                                inds[offset_3d_slab(new Vector3(position.X - 1, position.Y, position.Z - 1), size)],
                                inds[offset_3d_slab(new Vector3(position.X, position.Y, position.Z - 1), size)]
                                );
                        }
                        if (position.X > 0 && position.Y > 0 && (density[0] < 0.0f) != (density[4] < 0.0f))
                        {
                            quad(flip,
                                inds[offset_3d_slab(new Vector3(position.X, position.Y, position.Z), size)],
                                inds[offset_3d_slab(new Vector3(position.X, position.Y - 1, position.Z), size)],
                                inds[offset_3d_slab(new Vector3(position.X - 1, position.Y - 1, position.Z), size)],
                                inds[offset_3d_slab(new Vector3(position.X - 1, position.Y, position.Z), size)]
                                );
                        }
                    }
                }
            }

            foreach (var vert in VertexBuffer)
                vert.Normal.Normalize();
        }

        public static int ToIndex(int x, int y, int z, int width, int height, int depth)
        {
            //if (flipIndexing_)
            //    y = height_ - y - 1;
            x = Mathf.Clamp(x, 0, width - 1);
            y = Mathf.Clamp(y, 0, height - 1);
            z = Mathf.Clamp(z, 0, depth - 1);
            return (z * width * height + y * width + x);
        }

        static int offset_3d(Vector3 p, Vector3 size)
        {
            return (int)((p.Z * size.Y + p.Y) * size.X + p.X);
        }

        static int offset_3d_slab(Vector3 p, Vector3 size)
        {
            return (int)(size.X * size.Y * (((int)p.Z) % 2) + p.Y * size.X + p.X);
        }

        static void do_edge(ref Vector3 average, ref int average_n, float va, float vb, int axis, Vector3 p) {
            if ((va < 0.0) == (vb < 0.0))
                return;

            Vector3 v = p;
            if (axis == 0)
                v.X += va / (va - vb);
            else if (axis == 1)
                v.Y += va / (va - vb);
            else if (axis == 2)
                v.Z += va / (va - vb);
            average = average + v;
            average_n++;
        }

        void quad(bool flip, int ia, int ib, int ic, int id)
        {
            if (flip)
            {
                int t = ib;
                ib = id;
                id = t;
            }
            
            if (ia >= VertexBuffer.Count || ib >= VertexBuffer.Count || ic >= VertexBuffer.Count || id >= VertexBuffer.Count)
                return;
            
            var va = VertexBuffer[ia];
            var vb = VertexBuffer[ib];
            var vc = VertexBuffer[ic];
            var vd = VertexBuffer[id];
            
            Vector3 ab = va.Position - vb.Position;
            Vector3 cb = vc.Position - vb.Position;
            Vector3 n1 = Vector3.Cross(cb, ab);
            va.Normal += n1;
            vb.Normal += n1;
            vc.Normal += n1;
            
            Vector3 ac = va.Position - vc.Position;
            Vector3 dc = vd.Position - vc.Position;
            Vector3 n2 = Vector3.Cross(dc, ac);
            va.Normal += n2;
            vc.Normal += n2;
            vd.Normal += n2;
            
            IndexBuffer.Add(ia);
            IndexBuffer.Add(ib);
            IndexBuffer.Add(ic);
            
            IndexBuffer.Add(ia);
            IndexBuffer.Add(ic);
            IndexBuffer.Add(id);
        }

        public MeshData GetMeshData(GraphicsDevice device)
        {
            return new MeshData(IndexBuffer, VertexBuffer);
        }

        static readonly float H = 0.0001f;
        static readonly Vector3[] NormalOffsets =
        {
            new Vector3(H, 0, 0),
            new Vector3(0, H, 0),
            new Vector3(0, 0, H)
        };
        public Vector3 CalculateSurfaceNormal(IHaveDensity densitySrc, Vector3 p)
        {
            float dx = densitySrc.GetDensity(p + NormalOffsets[0]) - densitySrc.GetDensity(p - NormalOffsets[0]);
            float dy = densitySrc.GetDensity(p + NormalOffsets[1]) - densitySrc.GetDensity(p - NormalOffsets[1]);
            float dz = densitySrc.GetDensity(p + NormalOffsets[2]) - densitySrc.GetDensity(p - NormalOffsets[2]);

            var ret = new Vector3(dx, dy, dz);
            ret.Normalize();
            return ret;
        }
    }
}
