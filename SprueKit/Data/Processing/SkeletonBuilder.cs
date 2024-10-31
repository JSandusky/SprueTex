using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PluginLib;
using Microsoft.Xna.Framework;

namespace SprueKit.Data.Processing
{
    public class SkeletonBuilder
    {
        public static SkeletonData BuildSkeleton(Data.SprueModel model)
        {
            SkeletonData ret = new SkeletonData();
            ret.AddJoint(null, new JointData() { Name = "Root", Position = model.Position });
            BuildSkeleton(model, ret, ret.Root, false, true);
            return ret;
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

        static void BuildSkeleton(SpruePiece piece, SkeletonData skeleton, JointData currentJoint, bool symmetric, bool rootPass)
        {
            if (!piece.IsEnabled)
                return;

            if (piece is ChainPiece)
            {
                ChainPiece chain = piece as ChainPiece;

                List<JointData> newJoints = new List<JointData>();
                if (chain.GenerateBones)
                {
                    var bones = chain.Bones;
                    int startIdx = 0;
                    if (chain.IsSpine)
                    {
                        int startIndex = ClosestChainBone(currentJoint.Position, chain);
                        // buidl list of new joints
                        for (int i = 0; i < bones.Count; ++i)
                        {
                            JointData newChild = new JointData { Name = bones[i].Name, Position = bones[i].Position };
                            if (symmetric)
                            {
                                newChild.Position = piece.GetSymmetricVector(newChild.Position);
                                newChild.Name = newChild.Name.SymmetricVersion();
                            }
                            newJoints.Add(newChild);
                        }

                        for (int i = startIndex; i < bones.Count; ++i)
                        {
                            if (i == startIndex)
                                skeleton.AddJoint(currentJoint, newJoints[i]);
                            else
                                skeleton.AddJoint(newJoints[i - 1], newJoints[i]);
                        }

                        for (int i = startIndex - 1; i >= 0; --i)
                        {
                            skeleton.AddJoint(newJoints[i + 1], newJoints[i]);
                        }

                        for (int i = 0; i < newJoints.Count; ++i)
                        foreach (var child in bones[i].Children)
                            BuildSkeleton(child, skeleton, newJoints[i], false, false);
                    }
                    else
                    {
                        for (int i = 0; i < bones.Count; ++i)
                        {
                            JointData newChild = new JointData { Name = bones[i].Name, Position = bones[i].Position };
                            if (symmetric)
                            {
                                newChild.Position = piece.GetSymmetricVector(newChild.Position);
                                newChild.Name = newChild.Name.SymmetricVersion();
                            }

                            newJoints.Add(newChild);
                            if (i == startIdx)
                                skeleton.AddJoint(currentJoint, newChild);
                            else
                                skeleton.AddJoint(newJoints[i - 1], newChild);

                            foreach (var child in bones[i].Children)
                                BuildSkeleton(child, skeleton, newChild, false, false);
                        }
                    }

                    if (piece.Symmetric != SymmetricAxis.None && !symmetric)
                        BuildSkeleton(piece, skeleton, currentJoint, true, false);
                }

                foreach (var child in piece.Children)
                {
                    if (newJoints.Count > 0)
                        BuildSkeleton(child, skeleton, newJoints[0], false, false);
                    else
                        BuildSkeleton(child, skeleton, currentJoint, false, false);
                }
            }
            else if (piece is ModelPiece)
            {
                ModelPiece self = piece as ModelPiece;
                List<MeshData> meshes = self.GetMeshes();
                if (meshes != null && meshes.Count > 0)
                {
                    foreach (var mesh in meshes)
                    {
                        if (mesh.Skeleton != null)
                        {
                            AddSkeleton(skeleton, currentJoint, mesh.Skeleton, symmetric, self);
                            if (!symmetric && self.Symmetric != SymmetricAxis.None)
                                AddSkeleton(skeleton, currentJoint, mesh.Skeleton, true, self);
                        }
                    }
                }

                foreach (var child in piece.Children)
                {
                    BuildSkeleton(child, skeleton, currentJoint, false, false);
                }
            }
            else
            {
                foreach (var child in piece.Children)
                    BuildSkeleton(child, skeleton, currentJoint, false, false);
            }
        }

        static void AddSkeleton(SkeletonData currentSkeleton, JointData currentJoint, SkeletonData newSkeleton, bool symmetric, SpruePiece self)
        {
            //JointData newRoot = new JointData();
            //JointData srcRoot = newSkeleton.Root;
            //newRoot.Position = Vector3.Transform(srcRoot.Position, currentJoint.Transform);
            JointData subRoot = newSkeleton.Root.Duplicate();
            currentSkeleton.AddJoint(currentJoint, subRoot);
            BuildSkeleton(currentSkeleton, subRoot, newSkeleton.Root, symmetric, self);
        }

        static void BuildSkeleton(SkeletonData currentSkeleton, JointData targetJoint, JointData currentJoint, bool symmetric, SpruePiece self)
        {
            foreach (var child in currentJoint.Children)
            {
                var newJoint = child.Duplicate();
                if (symmetric)
                {
                    newJoint.Position = self.GetSymmetricVector(newJoint.Position);
                    newJoint.Name = newJoint.Name.SymmetricVersion();
                }
                currentSkeleton.AddJoint(targetJoint, newJoint);
                BuildSkeleton(currentSkeleton, newJoint, child, symmetric, self);
            }
        }
    }
}
