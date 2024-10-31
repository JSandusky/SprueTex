using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SprueKit.Tasks;
using Microsoft.Xna.Framework;

namespace SprueKit.Data.Sprue
{
    /// <summary>
    /// Responsible for generating a single stage of a texture map
    /// </summary>
    public class TextureMapTask : TaskItem
    {
        public override string TaskName { get { return "texture mapping"; } }

        Data.SprueModel targetModel_;

        public TextureMapTask(Data.SprueModel model) : base(null)
        {
            targetModel_ = model;
        }

        public override void TaskLaunch()
        {
            if (IsCanceled || targetModel_ == null)
                return;

            // Collect list of texture components
            List<TextureComponent> textureComponents = new List<TextureComponent>();
            targetModel_.VisitAll<TextureComponent>((tc) => { textureComponents.Add(tc); });

            if (textureComponents.Count == 0)
            {
                ErrorHandler.inst().Info("No texture components to process");
                return;
            }

            TextureChannel[] Channels =
            {
                TextureChannel.Diffuse,
                TextureChannel.Roughness,
                TextureChannel.Metallic,
                TextureChannel.Displacement
            };

            TexturePass[] Passes =
            {
                TexturePass.Base,
                TexturePass.Standard,
                TexturePass.Final
            };

            List<KeyValuePair<TextureChannel, TexturePass>> usedPasses = GetPasses(textureComponents);

            var meshData = targetModel_.MeshData;
            if (meshData == null)
                return;
            int TexWidth = targetModel_.TextureSize.X;
            int TextHeight = targetModel_.TextureSize.Y;

            for (int chanIdx = 0; chanIdx < Channels.Length; ++chanIdx)
            {
                Baking.RasterizerData rasterData = new Baking.RasterizerData();
                rasterData.Init(TexWidth, TextHeight, true);

                for (int passIdx = 0; passIdx < Passes.Length; ++passIdx)
                {
                    if (IsCanceled)
                        return;

                    KeyValuePair<TextureChannel, TexturePass> kvp = new KeyValuePair<TextureChannel, TexturePass>(Channels[chanIdx], Passes[passIdx]);
                    if (usedPasses.Contains(kvp))
                    {
                        ErrorHandler.inst().Info(string.Format("Building {0} texture, pass {1}", Channels[chanIdx].ToString(), Passes[passIdx].ToString()));

                        var subMapList = GetByChannelPass(textureComponents, kvp.Key, kvp.Value);
                        for (int i = 0; i < subMapList.Count; ++i)
                        {
                            if (IsCanceled)
                                return;

                            var subMap = subMapList[i];
                            Baking.RasterizerData subMapData = new Baking.RasterizerData();
                            subMapData.Init(TexWidth, TextHeight, true);

                            var meshVertices = meshData.GetVertices();
                            var meshindices = meshData.GetIndices();
                            for (int vertIndex = 0; vertIndex < meshData.IndexCount; vertIndex += 3)
                            {
                                if (IsCanceled)
                                    return;

                                int[] indices = {
                                    meshindices[vertIndex],
                                    meshindices[vertIndex + 1],
                                    meshindices[vertIndex + 2],
                                };
                                Vector2[] uvs = {
                                    meshVertices[indices[2]].TextureCoordinate,
                                    meshVertices[indices[1]].TextureCoordinate,
                                    meshVertices[indices[0]].TextureCoordinate,
                                };
                                Vector3[] pos = {
                                    meshVertices[indices[0]].Position,
                                    meshVertices[indices[1]].Position,
                                    meshVertices[indices[2]].Position,
                                };
                                Vector3[] nor = {
                                    meshVertices[indices[0]].Normal,
                                    meshVertices[indices[1]].Normal,
                                    meshVertices[indices[2]].Normal,
                                };

                                Rasterize(subMap, ref subMapData, pos, nor, uvs);
                            }


                            if (subMapData.AnythingWritten)
                            {
                                Baking.RasterizerData.PadEdges(ref subMapData, 3);
                                if (subMap.Value != null)
                                    Baking.MeshRasterizer.Blit(ref rasterData, ref subMapData, (Baking.MeshRasterizer.RasterizerBlend)((int)subMap.Value.Blending));
                                else
                                {
                                    SimpleTextureComponent tc = subMap.Key as SimpleTextureComponent;
                                    Baking.MeshRasterizer.Blit(ref rasterData, ref subMapData, (Baking.MeshRasterizer.RasterizerBlend)((int)tc.Blending));
                                }
                            }
                        }
                    }
                }

                if (rasterData.AnythingWritten)
                {
                    //Baking.RasterizerData.PadEdges(ref rasterData, 3);
                    var imgData = Data.BindingUtil.ToImageData(rasterData);

                    if (targetModel_.MeshData.Texture == null)
                        targetModel_.MeshData.Texture = new Material();
                    targetModel_.MeshData.Texture.SetTextureData(Channels[chanIdx], imgData);
                    ErrorHandler.inst().Info(string.Format("Generated texture channel {0}", Channels[chanIdx]));
                    SprueBindings.ImageData.Save(string.Format("{0}.png", Channels[chanIdx]), imgData, ErrorHandler.inst());
                }
            }
        }

        public override void TaskEnd()
        {
            
        }

        List<KeyValuePair<TextureComponent, TextureMap> > GetByChannelPass(List<TextureComponent> components, TextureChannel channel, TexturePass pass)
        {
            List<KeyValuePair<TextureComponent, TextureMap>> ret = new List<KeyValuePair<TextureComponent, TextureMap>>();
            for (int i = 0; i < components.Count; ++i)
            {
                if (components[i] is BasicTextureComponent)
                {
                    BasicTextureComponent comp = components[i] as BasicTextureComponent;
                    for (int tex = 0; tex < comp.TextureMaps.Count; ++tex)
                    {
                        var texMap = comp.TextureMaps[tex];
                        if (texMap.Channel == channel && texMap.Pass == pass)
                            ret.Add(new KeyValuePair<TextureComponent, TextureMap>(comp, texMap));
                    }
                }
                else if (components[i] is SimpleTextureComponent)
                {
                    SimpleTextureComponent tc = components[i] as SimpleTextureComponent;
                    if (tc.Channel == channel && tc.Pass == pass)
                        ret.Add(new KeyValuePair<TextureComponent, TextureMap>(components[i], null));
                }
                else
                {
                    ErrorHandler.inst().Warning("Unexpected texturing component encountered");
                }
            }
            return ret;
        }

        void Rasterize(KeyValuePair<TextureComponent, TextureMap> data, ref Baking.RasterizerData rasterData, Vector3[] pos, Vector3[] nor, Vector2[] uv)
        {
            Baking.MeshRasterizer.RasterizeModelSpaceTriangle(ref rasterData, pos, nor, uv, (ref Vector3 tPos, ref Vector3 tNor) =>
            {
                Color? value = data.Key.Sample(tPos, tNor, data.Value?.Texture);
                if (!value.HasValue)
                    return null;
                return ColorF.FromMonoGame(value.Value);
            });
        }

        //void Rasterize(List<TextureComponent> comps, ref Baking.RasterizerData rasterData, Vector3[] pos, Vector3[] nor, Vector2[] uv, TexturePass pass, TextureChannel channel)
        //{
        //    for (int i = 0; i < comps.Count; ++i)
        //    {
        //        if (comps[i] is BasicTextureComponent)
        //        {
        //            BasicTextureComponent comp = comps[i] as BasicTextureComponent;
        //            for (int img = 0; img < comp.TextureMaps.Count; ++img)
        //            {
        //                if (IsCanceled)
        //                    return;
        //
        //                if (comp.TextureMaps[i].Channel == channel && comp.TextureMaps[i].Pass == pass && comp.TextureMaps[i].Texture != null)
        //                {
        //                    Baking.MeshRasterizer.RasterizeModelSpaceTriangle(ref rasterData, pos, nor, uv, (Baking.MeshRasterizer.RasterizerBlend)((int)comp.TextureMaps[i].Blending), (ref Vector3 tPos, ref Vector3 tNor) =>
        //                    {
        //                        Color? value = comps[i].Sample(tPos, tNor, comp.TextureMaps[i].Texture);
        //                        if (!value.HasValue)
        //                            return null;
        //                        return ColorF.FromMonoGame(value.Value);
        //                    });
        //                }
        //            }
        //        }
        //        else if (pass == TexturePass.Base && channel == TextureChannel.Diffuse) // solid color
        //        {
        //            Baking.MeshRasterizer.RasterizeModelSpaceTriangle(ref rasterData, pos, nor, uv, Baking.MeshRasterizer.RasterizerBlend.Replace, (ref Vector3 tPos, ref Vector3 tNor) =>
        //            {
        //                Color? value = comps[i].Sample(tPos, tNor, null);
        //                if (!value.HasValue)
        //                    return null;
        //                return ColorF.FromMonoGame(value.Value);
        //            });
        //        }
        //    }
        //}

        List<KeyValuePair<TextureChannel, TexturePass>> GetPasses(List<TextureComponent> components)
        {
            List<KeyValuePair<TextureChannel, TexturePass>> ret = new List<KeyValuePair<TextureChannel, TexturePass>>();

            for (int i = 0; i < components.Count; ++i)
            {
                if (components[i] is BasicTextureComponent)
                {
                    BasicTextureComponent tc = components[i] as BasicTextureComponent;
                    for (int map = 0; map < tc.TextureMaps.Count; ++map)
                    {
                        var mapTexture = tc.TextureMaps[map];
                        KeyValuePair<TextureChannel, TexturePass> kvp = new KeyValuePair<TextureChannel, TexturePass>(mapTexture.Channel, mapTexture.Pass);
                        if (!ret.Contains(kvp))
                            ret.Add(kvp);
                    }
                }
                else if (components[i] is SimpleTextureComponent)
                {
                    SimpleTextureComponent tc = components[i] as SimpleTextureComponent;
                    KeyValuePair<TextureChannel, TexturePass> kvp = new KeyValuePair<TextureChannel, TexturePass>(tc.Channel, tc.Pass);
                    if (!ret.Contains(kvp))
                        ret.Add(kvp);
                }
            }

            return ret;
        }
    }
}
