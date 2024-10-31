using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Graphics
{
    public enum MeshPrimitiveType
    {
        Sphere,
        Box,
        Cylinder,
        Plane,
        Custom
        //Teapot
    }

    public static class Primitives
    {
        static List<Data.MeshData> primitves_;

        public static Data.MeshData GetPrimitive(MeshPrimitiveType type)
        {
            if (primitves_ == null)
            {
                primitves_ = new List<Data.MeshData>();
                var mdl = SprueBindings.ModelData.LoadModel(App.ContentPath("Models/Sphere.obj"), ErrorHandler.inst());
                primitves_.Add(Data.BindingUtil.ToMesh(mdl));

                mdl = SprueBindings.ModelData.LoadModel(App.ContentPath("Models/Box.obj"), ErrorHandler.inst());
                primitves_.Add(Data.BindingUtil.ToMesh(mdl));

                mdl = SprueBindings.ModelData.LoadModel(App.ContentPath("Models/Cylinder.obj"), ErrorHandler.inst());
                primitves_.Add(Data.BindingUtil.ToMesh(mdl));

                mdl = SprueBindings.ModelData.LoadModel(App.ContentPath("Models/Plane.obj"), ErrorHandler.inst());
                primitves_.Add(Data.BindingUtil.ToMesh(mdl));

                mdl = SprueBindings.ModelData.LoadModel(App.ContentPath("Models/Teapot.obj"), ErrorHandler.inst());
                primitves_.Add(Data.BindingUtil.ToMesh(mdl));

            }
            
            return primitves_[(int)type];
        }
    }
}
