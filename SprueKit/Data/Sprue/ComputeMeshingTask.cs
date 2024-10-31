using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using SprueKit.Tasks;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;
using System.IO;

namespace SprueKit.Data.Sprue
{
    public class ComputeMeshingTask : ComputeTaskItem
    {
        public override string TaskName { get { return "mesh voxelization"; } }

        static CancellationTokenSource source;

        SprueModel targetModel_;
        MeshData resultMesh_;

        static SprueBindings.ComputeShader surfaceNetsShader_;
        static SprueBindings.ComputeKernel surfaceNetsKernel_;
        static DensityFunctionCache cache_ = new DensityFunctionCache();

        public ComputeMeshingTask(SprueModelDocument doc, SprueModel target) : base(null)
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

        public override void TaskLaunch()
        {
            targetModel_.PrepareDensity();

            const int BLOCK_SIZE = 64;
            const int CELL_SIZE = BLOCK_SIZE + 1;
            const int BLOCK_COUNT = BLOCK_SIZE * BLOCK_SIZE * BLOCK_SIZE;
            const int CELL_COUNT = CELL_SIZE * CELL_SIZE * CELL_SIZE;

            SprueBindings.ComputeDevice device = GetComputeDevice();
            if (device == null)
            {
                ErrorHandler.inst().Error("Unable to aquire a compute device for GPU meshing");
                return;
            }

            MemoryStream shapeParameters = new MemoryStream();
            MemoryStream shapeTransforms = new MemoryStream();
            DensityShaderBuilder shaderBuilder = new DensityShaderBuilder(targetModel_, shapeParameters, shapeTransforms);

            var densityShader = cache_.GetDensityShader(targetModel_, device, shaderBuilder);
            if (densityShader == null)
            {
                // Do we have nothing to do?
                if (shapeParameters.Length == 0)
                    return;
                ErrorHandler.inst().Error("Unable to construct a compute kernel for GPU meshing");
                return;
            }
            var densityBuffer = device.CreateBuffer(sizeof(float) * CELL_COUNT, SprueBindings.ComputeBufferFlags.CBS_FloatData | SprueBindings.ComputeBufferFlags.CBS_Write | SprueBindings.ComputeBufferFlags.CBS_Read);

            SprueBindings.ComputeBuffer transformsBuffer = null;
            SprueBindings.ComputeBuffer paramsBuffer = null;
            shapeParameters.Seek(0, SeekOrigin.Begin);
            shapeTransforms.Seek(0, SeekOrigin.Begin);
            using (StreamReader bullshit1 = new StreamReader(shapeParameters))
            using (StreamReader bullshit2 = new StreamReader(shapeTransforms))
            {
                transformsBuffer = device.CreateBuffer((int)shapeTransforms.Length, SprueBindings.ComputeBufferFlags.CBS_FloatData | SprueBindings.ComputeBufferFlags.CBS_Read);
                paramsBuffer = device.CreateBuffer((int)shapeParameters.Length, SprueBindings.ComputeBufferFlags.CBS_FloatData | SprueBindings.ComputeBufferFlags.CBS_Read);
                transformsBuffer.SetData(shapeTransforms.ToArray());
                paramsBuffer.SetData(shapeParameters.ToArray());
            }

            float[] densityData = null;
            Vector4[] positionData = null;
            byte[] cornersMaskData = null;

            using (densityBuffer)
            using (transformsBuffer)
            using (paramsBuffer)
            {

                var kernel = densityShader.GetKernel("GenerateField");
                kernel.Bind(densityBuffer, 0);
                kernel.Bind(paramsBuffer, 1);
                kernel.Bind(transformsBuffer, 2);
                kernel.Execute(CELL_SIZE, CELL_SIZE, CELL_SIZE);
                device.Finish();

                //if (IsCanceled)
                //    return;

                var vertexkernel = GetSurfaceNetsKernel(device);
                if (vertexkernel == null)
                {
                    ErrorHandler.inst().Error("Failed compiling root-finding kernel");
                    return;
                }

                positionData = new Vector4[BLOCK_COUNT];
                cornersMaskData = new byte[BLOCK_COUNT];

                using (var vertexPosBuffer = device.CreateBuffer(Marshal.SizeOf<Vector4>() * BLOCK_COUNT, SprueBindings.ComputeBufferFlags.CBS_FloatData | SprueBindings.ComputeBufferFlags.CBS_Write))
                using (var cornersBuffer = device.CreateBuffer(sizeof(byte) * BLOCK_COUNT, SprueBindings.ComputeBufferFlags.CBS_Write))
                {
                    cornersBuffer.SetData(cornersMaskData);

                    vertexkernel.Bind(densityBuffer, 0);
                    vertexkernel.Bind(vertexPosBuffer, 1);
                    vertexkernel.Bind(cornersBuffer, 2);
                    vertexkernel.Execute(BLOCK_SIZE, BLOCK_SIZE, BLOCK_SIZE);
                    device.Finish();

                    vertexPosBuffer.ReadData(positionData);
                    cornersBuffer.ReadData(cornersMaskData);
                }

                densityData = new float[CELL_COUNT];
                densityBuffer.ReadData(densityData);
            }

            if (positionData != null && densityData != null && cornersMaskData != null)
            {
                // TODO: generate the index and vertex buffer data
                resultMesh_ = new Processing.NaiveSurfaceNets(densityData, positionData, cornersMaskData, targetModel_).GetMeshData(null);
                {
                    if (IsCanceled)
                        return;

                    // Run smoothing pass
                    SprueBindings.MeshData data = new SprueBindings.MeshData
                    {
                        Positions = new Vector3[resultMesh_.VertexCount],
                        Normals = new Vector3[resultMesh_.VertexCount],
                        Indices = new int[resultMesh_.IndexCount]
                    };
                    PushDisposable(data);

                    var vertices = resultMesh_.GetVertices();
                    var indices = resultMesh_.GetIndices();
                    for (int i = 0; i < data.Positions.Length; ++i)
                        data.Positions[i] = vertices[i].Position;
                    for (int i = 0; i < data.Normals.Length; ++i)
                        data.Normals[i] = vertices[i].Normal;
                    for (int i = 0; i < data.Indices.Length; ++i)
                        data.Indices[i] = indices[i];

                    data.WriteToAPI();
                    //data.Subdivide(true);
                    data.Smooth(1.0f);
                    data.ReadFromAPI();

                    List<PluginLib.VertexData> newVerts = new List<PluginLib.VertexData>();
                    for (int i = 0; i < data.Positions.Length; ++i)
                        newVerts.Add(new PluginLib.VertexData(data.Positions[i], data.Normals[i], new Vector2()));

                    resultMesh_.SetData(data.Indices.ToList(), newVerts);
                }
            }
            else
            {
                ErrorHandler.inst().Error("Unknown failure in GPU meshing");
                return;
            }
        }

        public override void TaskEnd()
        {
            targetModel_.MeshData = resultMesh_;
        }

        public override void CanceledEnd()
        {
            targetModel_.MeshData = resultMesh_;
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        SprueBindings.ComputeKernel GetSurfaceNetsKernel(SprueBindings.ComputeDevice device)
        {
            if (surfaceNetsKernel_ != null)
                return surfaceNetsKernel_;

            surfaceNetsShader_ = device.CreateShader();
            if (surfaceNetsShader_.Compile(WPFExt.GetEmbeddedFile("SprueKit.Data.Sprue.SurfaceNetsShader.cl"), ""))
            {
                surfaceNetsKernel_ = surfaceNetsShader_.GetKernel("CalculateVertexPositions");
                return surfaceNetsKernel_;
            }
            else
                ErrorHandler.inst().Error("Failed to compile surface nets root finding shader");

            return null;
        }

        class DensityFunctionCache
        {
            Dictionary<SprueModel, KeyValuePair<int, SprueBindings.ComputeShader>> shaders_ = new Dictionary<SprueModel, KeyValuePair<int, SprueBindings.ComputeShader>>();

            public SprueBindings.ComputeShader GetDensityShader(SprueModel model, SprueBindings.ComputeDevice device, DensityShaderBuilder builder)
            {
                int structuralHash = model.StructuralHash();
                KeyValuePair<int, SprueBindings.ComputeShader> shaderPair;
                if (shaders_.TryGetValue(model, out shaderPair))
                {
                    if (shaderPair.Key == structuralHash)
                    {
                        builder.WriteCode = false;
                        builder.BuildShader(model, false);
                        //ErrorHandler.inst().Debug("Reusing existing density shader");
                        return shaderPair.Value;
                    }
                    else
                        shaderPair.Value.Dispose(); // we need to recreate it
                }

                // TODO build the shader and everything
                builder.WriteCode = true;
                string customCode = builder.BuildShader(model, true);
                if (!builder.AnyDataWritten)
                    return null;

                var newShader = device.CreateShader();
                if (newShader.Compile(new List<string> { FunctionCode, customCode, ShaderCode }, ""))
                {
                    shaderPair = new KeyValuePair<int, SprueBindings.ComputeShader>(structuralHash, newShader);
                    shaders_[model] = shaderPair;
                    //ErrorHandler.inst().Debug("Constructed new density shader");
                    return shaderPair.Value;
                }
                else
                    ErrorHandler.inst().Error("Failed to compile compute shader");

                return null;
            }

            static string functionCode_;
            string FunctionCode
            {
                get
                {
                    if (functionCode_ == null)
                        functionCode_ = WPFExt.GetEmbeddedFile("SprueKit.Data.Sprue.DensityShader.cl");
                    return functionCode_;
                }
            }

            static string shaderCode_;
            string ShaderCode
            {
                get
                {
                    if (shaderCode_ == null)
                    { 
                        shaderCode_ = WPFExt.GetEmbeddedFile("SprueKit.Data.Sprue.GenerateField.cl");
                    }
                    return shaderCode_;
                }
            }
        }

        class DensityShaderBuilder
        {
            static string MainCode = "float DensityFunc(float3 pos, const global float* shapeData, const global float4* transformData) {{\r\nint paramIndex = 0; int transformIndex = 0;\r\n{0}\r\nreturn density;\r\n}}\r\n";

            SprueModel target_;
            public byte[] ParamData = null;
            public byte[] TransformData = null;

            BinaryWriter paramStream_;
            BinaryWriter transformStream_;
            bool anyCodeWritten_ = false;

            public bool AnyDataWritten = false;

            public DensityShaderBuilder(SprueModel model, MemoryStream paramTarget, MemoryStream transTarget)
            {
                target_ = model;
                paramStream_ = new BinaryWriter(paramTarget);
                transformStream_ = new BinaryWriter(transTarget);
            }

            public bool WriteCode { get; set; } = true;

            public string BuildShader(SprueModel model, bool writeCode)
            {
                StringBuilder builder = new StringBuilder();

                Visit(model, builder, false);

                //paramStream_.Dispose();
                //transformStream_.Dispose();

                if (WriteCode)
                    return string.Format(MainCode, builder.ToString());
                return null;
            }

            void Visit(SpruePiece piece, StringBuilder builder, bool symmetric)
            {
                Matrix adjMat = symmetric ? piece.GetSymmetricMatrix() : piece.InverseTransform;

                if (piece is SimplePiece)
                {
                    AnyDataWritten = true;
                    SimplePiece shape = piece as SimplePiece;
                    if (WriteCode)
                    {
                        string pieceCode = GetShapeCodeString(shape);
                        if (anyCodeWritten_)
                        {
                            switch (((SimplePiece)piece).CSGOperation)
                            {
                                case CSGOperation.Add:
                                    builder.AppendFormat("density = CSGAdd(density, {0});\r\n", pieceCode);// simplePiece->GetDensityHandler()->ToString()).str();
                                    break;
                                case CSGOperation.Subtract:
                                    builder.AppendFormat("density = CSGSubtract(density, {0});\r\n", pieceCode);// simplePiece->GetDensityHandler()->ToString()).str();
                                    break;
                                case CSGOperation.Intersect:
                                    builder.AppendFormat("density = CSGIntersect(density, {0});\r\n", pieceCode);// simplePiece->GetDensityHandler()->ToString()).str();
                                    break;
                            }
                        }
                        else
                        {
                            anyCodeWritten_ = true;
                            builder.AppendFormat("float density = {0};\r\n", pieceCode);// simplePiece->GetDensityHandler()->ToString()).str();
                        }
                    }

                    WritePieceParams(shape, paramStream_, transformStream_, adjMat);

                    if (piece.Symmetric != SymmetricAxis.None)
                    {
                        if (symmetric == false)
                            Visit(piece, builder, true);
                    }
                }
                else if (piece is ChainPiece)
                {
                    AnyDataWritten = true;
                    ChainPiece chain = piece as ChainPiece;

                    var chainPts = chain.GetPositions();
                    var chainCrosses = chain.GetCrosses();
                    paramStream_.Write((float)chainPts.Count);
                    for (int i = 0; i < chainPts.Count - 1; ++i)
                    {
                        if (!symmetric)
                        {
                            paramStream_.Write(chainPts[i]);
                        }
                        else
                        {
                            Vector3 w = piece.GetSymmetricVector(chainPts[i]);
                            paramStream_.Write(w);
                        }


                        paramStream_.Write(chainCrosses[i].X);
                        paramStream_.Write(chainCrosses[i].Y);
                        paramStream_.Write(chainCrosses[i].Z);

                        if (!symmetric)
                        {
                            paramStream_.Write(chainPts[i + 1].X);
                            paramStream_.Write(chainPts[i + 1].Y);
                            paramStream_.Write(chainPts[i + 1].Z);
                        }
                        else
                        {
                            Vector3 w = piece.GetSymmetricVector(chainPts[i+1]);
                            paramStream_.Write(w);
                        }

                        paramStream_.Write(chainCrosses[i+1].X);
                        paramStream_.Write(chainCrosses[i+1].Y);
                        paramStream_.Write(chainCrosses[i+1].Z);
                    }

                    WriteMatrix(transformStream_, chain.InverseTransform);

                    if (chainPts.Count > 0)
                    {
                        if (WriteCode)
                        {
                            if (anyCodeWritten_)
                                builder.Append("density = CSGAdd(density, Segment(pos, shapeData, transformData, &paramIndex, &transformIndex));\r\n");
                            else
                            {
                                anyCodeWritten_ = true;
                                builder.Append("float density = Segment(pos, shapeData, transformData, &paramIndex, &transformIndex);\r\n");
                            }
                        }
                    }

                    if (piece.Symmetric != SymmetricAxis.None)
                    {
                        if (symmetric == false)
                            Visit(piece, builder, true);
                    }

                    foreach (var bone in chain.Bones)
                    {
                        foreach (var child in bone.Children)
                            Visit(child, builder, false);
                    }
                }

                if (piece is ChainPiece || piece is SimplePiece)
                {
                    //if (piece.Symmetric != SymmetricAxis.None)
                    //    Visit(piece, builder, true);
                }

                foreach (var child in piece.Children)
                    Visit(child, builder, false);
            }

            string GetShapeCodeString(SimplePiece piece)
            {
                switch (piece.ShapeType)
                {
                    //return "CappedConeDensity(pos, shapeData, transformData, &paramIndex, &transformIndex)";
                    case ShapeFunctionType.Box:
                        return "BoxDensity(pos, shapeData, transformData, &paramIndex, &transformIndex)";
                    case ShapeFunctionType.Capsule:
                        return "CapsuleDensity(pos, shapeData, transformData, &paramIndex, &transformIndex)";
                    case ShapeFunctionType.Cone:
                        return "ConeDensity(pos, shapeData, transformData, &paramIndex, &transformIndex)";
                    case ShapeFunctionType.Cylinder:
                        return "CylinderDensity(pos, shapeData, transformData, &paramIndex, &transformIndex)";
                    case ShapeFunctionType.Ellipsoid:
                        return "EllipsoidDensity(pos, shapeData, transformData, &paramIndex, &transformIndex)";
                    case ShapeFunctionType.Plane:
                        return "PlaneDensity(pos, shapeData, transformData, &paramIndex, &transformIndex)";
                    case ShapeFunctionType.RoundedBox:
                        return "RoundedBoxDensity(pos, shapeData, transformData, &paramIndex, &transformIndex)";
                    case ShapeFunctionType.Sphere:
                        return "SphereDensity(pos, shapeData, transformData, &paramIndex, &transformIndex)";
                    case ShapeFunctionType.SuperShape:
                    case ShapeFunctionType.Torus:
                        return "TorusDensity(pos, shapeData, transformData, &paramIndex, &transformIndex)";
                }
                return null;
            }

            void WritePieceParams(SimplePiece piece, BinaryWriter paramStream, BinaryWriter transformStream, Matrix adjMat)
            {
                switch (piece.ShapeType)
                {
                    case ShapeFunctionType.Sphere:
                        paramStream.Write(piece.Params.X);
                        break;
                    case ShapeFunctionType.Capsule:
                    case ShapeFunctionType.Cone:
                    case ShapeFunctionType.Cylinder:
                        paramStream.Write(piece.Params.X);
                        paramStream.Write(piece.Params.Y);
                        break;
                    case ShapeFunctionType.Torus:
                        paramStream.Write(piece.Params.X);
                        paramStream.Write(piece.Params.Y);
                        break;
                    case ShapeFunctionType.Box:
                    case ShapeFunctionType.Ellipsoid:
                        paramStream.Write(piece.Params.X);
                        paramStream.Write(piece.Params.Y);
                        paramStream.Write(piece.Params.Z);
                        break;
                    case ShapeFunctionType.Plane:
                    case ShapeFunctionType.RoundedBox:
                        paramStream.Write(piece.Params.X);
                        paramStream.Write(piece.Params.Y);
                        paramStream.Write(piece.Params.Z);
                        paramStream.Write(piece.Params.W);
                        break;
                    case ShapeFunctionType.SuperShape:
                        break;
                }

                WriteMatrix(transformStream, adjMat);
                //Matrix transMat = Matrix.Transpose(piece.InverseTransform);
                //transformStream.Write(transMat.M11);
                //transformStream.Write(transMat.M12);
                //transformStream.Write(transMat.M13);
                //transformStream.Write(transMat.M14);
                //transformStream.Write(transMat.M21);
                //transformStream.Write(transMat.M22);
                //transformStream.Write(transMat.M23);
                //transformStream.Write(transMat.M24);
                //transformStream.Write(transMat.M31);
                //transformStream.Write(transMat.M32);
                //transformStream.Write(transMat.M33);
                //transformStream.Write(transMat.M34);
            }

            void WriteMatrix(BinaryWriter transformStream, Matrix mat)
            {
                Matrix transMat = Matrix.Transpose(mat);
                transformStream.Write(transMat.M11);
                transformStream.Write(transMat.M12);
                transformStream.Write(transMat.M13);
                transformStream.Write(transMat.M14);
                transformStream.Write(transMat.M21);
                transformStream.Write(transMat.M22);
                transformStream.Write(transMat.M23);
                transformStream.Write(transMat.M24);
                transformStream.Write(transMat.M31);
                transformStream.Write(transMat.M32);
                transformStream.Write(transMat.M33);
                transformStream.Write(transMat.M34);
            }
        }
    }
}
