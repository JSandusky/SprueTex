using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreMeshFilters
{
    [DisplayName("Flip Winding")]
    [Description("Flips the triangle winding of the mesh")]
    public class FlipWinding : PluginLib.IModelFilter
    {
        public bool FilterMesh(PluginLib.IModelData data, PluginLib.IErrorPublisher reporter)
        {
            bool anyChanged = false;
            for (int i = 0; i < data.MeshData.Count; ++i)
            {
                if (data.MeshData[i].Geometry.Indices != null && data.MeshData[i].Geometry.Indices.Length > 0)
                {
                    anyChanged = true;
                    for (int t = 0; t < data.MeshData[i].Geometry.Indices.Length; t += 3)
                    {
                        int a = data.MeshData[i].Geometry.Indices[t];
                        int b = data.MeshData[i].Geometry.Indices[t + 2];

                        data.MeshData[i].Geometry.Indices[t] = b;
                        data.MeshData[i].Geometry.Indices[t + 2] = a;
                    }
                }
            }
            return anyChanged;
        }
    }
}
