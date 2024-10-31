using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace CoreMeshFilters
{
    [DisplayName("Flip Normals")]
    [Description("Flips the vertex normals of the mesh")]
    public class FlipNormals : PluginLib.IModelFilter
    {
        public bool FilterMesh(PluginLib.IModelData data, PluginLib.IErrorPublisher reporter)
        {
            bool anyChanged = false;
            for (int i = 0; i < data.MeshData.Count; ++i)
            {
                var norms = data.MeshData[i].Geometry.Normals;
                if (norms != null && norms.Length > 0)
                {
                    anyChanged = true;
                    for (int x = 0; x < norms.Length; ++x)
                        norms[x] = norms[x] * -1;
                }
            }
            return anyChanged;
        }
    }
}
