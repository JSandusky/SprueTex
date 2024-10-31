using Microsoft.Xna.Framework;
using SprueKit.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SprueKit.Data.Sprue
{
    public class BMeshTask : TaskItem
    {
        public override string TaskName { get { return "generating B-Mesh"; } }
        static CancellationTokenSource source;

        SprueModel targetModel_;
        MeshData resultMesh_;

        public BMeshTask(SprueModelDocument doc, SprueModel target) : base(null)
        {
            targetModel_ = target;

            if (source != null)
                source.Cancel();
            source = new CancellationTokenSource();
            cancelationToken = source.Token;

            Consequences.Add(new CSGTask(target) { cancelationToken = this.cancelationToken });
            Consequences.Add(new UVTaskThekla(target, new Sprue.UVSettings()) { cancelationToken = this.cancelationToken });
            //Consequences.Add(new UVTask(target, new Sprue.UVSettings()) { cancelationToken = this.cancelationToken });
            Consequences.Add(new SkeletonBuilderTask(target) { cancelationToken = this.cancelationToken });
            Consequences.Add(new BoneWeightTask(target) { cancelationToken = this.cancelationToken });
            Consequences.Add(new TextureMapTask(target) { cancelationToken = this.cancelationToken });
            Consequences.Add(new FinalizationTask(doc) { cancelationToken = this.cancelationToken });
        }

        static int ClosestChainBone(Vector3 pos, ChainPiece piece)
        {
            Vector3 testPos = pos * new Vector3(1, 0, 1);
            float[] distance = new float[piece.Bones.Count];
            float minVal = float.MaxValue;
            int minIndex = 0;
            for (int i = 0; i < piece.Bones.Count; ++i)
            {
                distance[i] = (piece.Bones[i].Position * new Vector3(1, 0, 1)).Distance(testPos);
                if (distance[i] < minVal)
                {
                    minVal = Math.Min(minVal, distance[i]);
                    minIndex = i;
                }
            }

            return minIndex;
        }
        void BuildBMeshData(SpruePiece currentPiece, int parentIdx, List<SprueBindings.BMeshBall> balls, List<SprueBindings.BMeshConnection> conns, bool symmetric)
        {
            if (currentPiece is ChainPiece)
            {
                ChainPiece chain = currentPiece as ChainPiece;
                var bones = chain.Bones;
                int startIdx = 0;
                List<SprueBindings.BMeshBall> newBalls = new List<SprueBindings.BMeshBall>();
                if (chain.IsSpine)
                {
                    int startIndex = ClosestChainBone(Vector3.Zero, chain);
                    for (int i = 0; i < bones.Count; ++i)
                    {
                        SprueBindings.BMeshBall newChild = new SprueBindings.BMeshBall {
                            position_ = bones[i].Position,
                            radius_ = bones[i].CrossSection.MaxElement(), radiusX_ = bones[i].CrossSection.X, radiusY_ = bones[i].CrossSection.Y,
                            axisX_ = Vector3.TransformNormal(Vector3.UnitX, bones[i].Transform) * bones[i].CrossSection.X,
                            axisY_ = Vector3.TransformNormal(Vector3.UnitY, bones[i].Transform) * bones[i].CrossSection.Y,
                            axisZ_ = Vector3.TransformNormal(Vector3.UnitZ, bones[i].Transform) * bones[i].CrossSection.Z,
                            ballType_ = 0 };
                        if (newBalls.Count == 1)//i == startIndex)
                            newChild.ballType_ = 1;
                        if (symmetric)
                            newChild.position_ = currentPiece.GetSymmetricVector(newChild.position_);
                        balls.Add(newChild);
                        newBalls.Add(newChild);
                    }

                    // start -> end
                    for (int i = startIndex; i < bones.Count; ++i)
                    {
                        if (i == startIndex && parentIdx != -1)
                            conns.Add(new SprueBindings.BMeshConnection
                            {
                                from_ = parentIdx,
                                to_ = balls.IndexOf(newBalls[i])
                            });
                        //skeleton.AddJoint(currentJoint, newJoints[i]);
                        else if (i != startIndex)
                            conns.Add(new SprueBindings.BMeshConnection
                            {
                                from_ = balls.IndexOf(newBalls[i - 1]),
                                to_ = balls.IndexOf(newBalls[i])
                            });
                    }

                    // start -> front
                    for (int i = startIndex - 1; i >= 0; --i)
                    {
                        conns.Add(new SprueBindings.BMeshConnection
                        {
                            from_ = balls.IndexOf(newBalls[i + 1]),
                            to_ = balls.IndexOf(newBalls[i])
                        });
                    }

                    for (int i = 0; i < newBalls.Count; ++i)
                        foreach (var child in bones[i].Children)
                            BuildBMeshData(child, balls.IndexOf(newBalls[i]), balls, conns, false);
                }
                else
                {
                    for (int i = 0; i < bones.Count; ++i)
                    {
                        SprueBindings.BMeshBall ball = new SprueBindings.BMeshBall {
                            position_ = bones[i].Position,
                            ballType_ = balls.Count == 1 ? 1 : 0,
                            radius_ = bones[i].CrossSection.MaxElement(), radiusX_ = bones[i].CrossSection.X, radiusY_ = bones[i].CrossSection.Y,
                            axisX_ = Vector3.TransformNormal(Vector3.UnitX, bones[i].Transform) * bones[i].CrossSection.X,
                            axisY_ = Vector3.TransformNormal(Vector3.UnitY, bones[i].Transform) * bones[i].CrossSection.Y,
                            axisZ_ = Vector3.TransformNormal(Vector3.UnitZ, bones[i].Transform) * bones[i].CrossSection.Z,
                        };
                        if (symmetric)
                            ball.position_ = currentPiece.GetSymmetricVector(ball.position_);

                        balls.Add(ball);
                        newBalls.Add(ball);
                        if (i == startIdx && startIdx != -1)
                        {
                            SprueBindings.BMeshConnection conn = new SprueBindings.BMeshConnection
                            {
                                from_ = parentIdx,
                                to_ = balls.IndexOf(ball)
                            };
                            conns.Add(conn);
                        }
                        else if (startIdx != -1)
                        {
                            conns.Add(new SprueBindings.BMeshConnection {
                                from_ = balls.IndexOf(newBalls[i - 1]),
                                to_ = balls.IndexOf(ball)
                            });
                        }

                        foreach (var child in bones[i].Children)
                            BuildBMeshData(child, balls.IndexOf(ball), balls, conns, false);
                    }
                }

                if (currentPiece.Symmetric != SymmetricAxis.None && !symmetric)
                    BuildBMeshData(currentPiece, parentIdx, balls, conns, true);
            }
            else
            {
                foreach (var child in currentPiece.FlatChildren)
                    BuildBMeshData(child as SpruePiece, parentIdx, balls, conns, false);
            }
        }

        public override void TaskLaunch()
        {
            Dictionary<ChainPiece.ChainBone, int> boneTable = new Dictionary<ChainPiece.ChainBone, int>();
            List<SprueBindings.BMeshBall> balls = new List<SprueBindings.BMeshBall>();
            List<SprueBindings.BMeshConnection> conns = new List<SprueBindings.BMeshConnection>();
            BuildBMeshData(targetModel_, -1, balls, conns, false);

            if (conns.Count > 0 && balls.Count > 0)
            {
                SprueBindings.BMeshBuilder builder = new SprueBindings.BMeshBuilder();
                SprueBindings.MeshData meshData = builder.Build(balls.ToArray(), conns.ToArray(), targetModel_.BMeshSubdivisions);
                if (meshData != null)
                    resultMesh_ = BindingUtil.ToMesh(meshData);
            }
        }

        public override void TaskEnd()
        {
            targetModel_.MeshData = resultMesh_;
            //((UVTask)Consequences[0]).targetMeshData = targetModel_.MeshData;
        }

        public override void CanceledEnd()
        {
            targetModel_.MeshData = resultMesh_;
        }
    }
}
