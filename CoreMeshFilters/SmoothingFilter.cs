using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginLib;
using System.ComponentModel;

namespace CoreMeshFilters
{
    [DisplayName("Smooth")]
    [Description("Smooths the mesh")]
    public class SmoothingFilter : PluginLib.IModelFilter
    {
        public bool FilterMesh(IModelData data, IErrorPublisher reporter)
        {
            return false;
        }
    }
}
