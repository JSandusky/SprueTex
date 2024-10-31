using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginLib;
using System.ComponentModel;

namespace CoreMeshFilters
{
    [DisplayName("Tesselate")]
    [Description("Applies triangle tesselation to the mesh")]
    public class TesselationFilter : PluginLib.IModelFilter
    {
        public bool FilterMesh(IModelData data, IErrorPublisher reporter)
        {
            throw new NotImplementedException();
        }
    }
}
