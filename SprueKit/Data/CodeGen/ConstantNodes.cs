using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Color = Microsoft.Xna.Framework.Color;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;
using Quaternion = Microsoft.Xna.Framework.Quaternion;
using Matrix = Microsoft.Xna.Framework.Matrix;

namespace SprueKit.Data.CodeGen
{
    public static class SocketTypeID
    {
        // Core types
        public static int Boolean = 1 << 0;
        public static int Integer = 1 << 1;
        public static int Float = 1 << 2;
        // Monogame types
        public static int Vector2 = 1 << 3;
        public static int Vector3 = 1 << 4;
        public static int Vector4 = 1 << 5;
        public static int Quat = 1 << 6;
        public static int Matrix = 1 << 7;
        public static int Color = 1 << 9;
        public static int BBox = 1 << 10;
        // System.Object of some kind - basically a "user-object"
        public static int CSObject = 1 << 11;
        // System.Type, need this for casts
        public static int CSType = 1 << 12;
        // Need to know about it for switches
        public static int EnumObject = 1 << 13;
        public static int StringObject = 1 << 14;
    }

    // Implementing nodes can be reduced to expressions
    public interface IExpressionNode
    {

    }

    // Some sort of constant value, not VARIABLE, it's 5.5f NOT Orc.Health
    public interface IConstantNode
    {
    }

    // we can evaluate this node as a constant expression, must have ONE SINGLE output
    public interface IEvaluatableConstant
    {
    }

    public static class ConstantNodeHelpers
    {
        public static bool EverythingUpstreamIsConstant(Graph.GraphNode node, Graph.GraphSocket socket, bool isRecurseCall)
        {
            var sockets = node.Graph.Connections.Where(con => con.ToNode == node && con.ToSocket == socket);
            foreach (var outputSocket in sockets)
            {
                if (outputSocket.FromNode is IConstantNode)
                    return true;
                else if (outputSocket.FromNode is IEvaluatableConstant)
                {
                    bool anyFail = true;
                    foreach (var nextSocket in outputSocket.FromNode.InputSockets)
                    {
                        if (nextSocket.IsFlow == false)
                            anyFail &= EverythingUpstreamIsConstant(outputSocket.FromNode, nextSocket, true);
                    }
                    return anyFail;
                }
            }
            return false;
        }
    }

    public partial class IntConstant : Graph.GraphNode, IConstantNode
    {
        int value_ = 0;

        public override void Construct()
        {
            base.Construct();
            Name = "Integer";
            AddOutput(new Graph.GraphSocket(this) { Name = "Out" });
        }

        public override void Execute(object param) { OutputSockets[0].Data = value_; }
    }

    public partial class FloatConstant : Graph.GraphNode, IConstantNode
    {
        float value_ = 0.0f;

        public override void Construct()
        {
            base.Construct();
            Name = "Float";
            AddOutput(new Graph.GraphSocket(this) { Name = "Out" });
        }

        public override void Execute(object param) { OutputSockets[0].Data = value_; }
    }

    public partial class Vector2Constant : Graph.GraphNode, IConstantNode
    {
        Vector2 value_ = Vector2.Zero;

        public override void Construct()
        {
            base.Construct();
            Name = "Vector 2-D";
            AddOutput(new Graph.GraphSocket(this) { Name = "Out" });
        }

        public override void Execute(object param) { OutputSockets[0].Data = value_; }
    }

    public partial class Vector3Constant : Graph.GraphNode, IConstantNode
    {
        Vector3 value_ = Vector3.Zero;

        public override void Construct()
        {
            base.Construct();
            Name = "Vector 3-D";
            AddOutput(new Graph.GraphSocket(this) { Name = "Out" });
        }

        public override void Execute(object param) { OutputSockets[0].Data = value_; }
    }

    public partial class Vector4Constant : Graph.GraphNode, IConstantNode
    {
        Vector4 value_ = Vector4.Zero;

        public override void Construct()
        {
            base.Construct();
            Name = "Vector 4-D";
            AddOutput(new Graph.GraphSocket(this) { Name = "Out" });
        }

        public override void Execute(object param) { OutputSockets[0].Data = value_; }
    }

    public partial class RotatorConstant : Graph.GraphNode, IConstantNode
    {
        Quaternion value_ = Quaternion.Identity;

        public override void Construct()
        {
            base.Construct();
            Name = "Rotator";
            AddOutput(new Graph.GraphSocket(this) { Name = "Out" });
        }

        public override void Execute(object param) { OutputSockets[0].Data = value_; }
    }

    public partial class MatrixConstant : Graph.GraphNode, IConstantNode
    {
        Matrix value_ = Matrix.Identity;

        public override void Construct()
        {
            base.Construct();
            Name = "Matrix";
            AddOutput(new Graph.GraphSocket(this) { Name = "Out" });
        }

        public override void Execute(object param) { OutputSockets[0].Data = value_; }
    }

// Constants that can't be processed in constant handling

    public partial class EnumConstant : Graph.GraphNode
    {

    }

    public partial class StringConstant : Graph.GraphNode
    {
        string value_ = "";

        public override void Construct()
        {
            base.Construct();
            Name = "String";
            AddOutput(new Graph.GraphSocket(this) { Name = "Out" });
        }

        public override void Execute(object param) { OutputSockets[0].Data = value_; }
    }
}
