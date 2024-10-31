using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;

using SprueKit.Data.Graph;

namespace SprueKit.Data.CodeGen
{
    public interface ICompileErrorPrinter
    {
        void PrintError(bool isWarning, int line, int column, string text);
    }

    public class Compiler
    {
        public static CompiledScript Compile(Graph.Graph graph, Graph.GraphNode forNode)
        {
            return null;
        }

        static CompiledScript CompileString(string code, ICompileErrorPrinter printer)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateInMemory = true;
            parameters.ReferencedAssemblies.Add(Assembly.GetEntryAssembly().Location);

            var results = provider.CompileAssemblyFromSource(parameters, code);
            int errCt = 0;
            foreach (CompilerError error in results.Errors)
            {
                errCt += error.IsWarning ? 0 : 1;
                if (printer != null)
                {
                    printer.PrintError(error.IsWarning, error.Line, error.Column, error.ErrorText);
                }
                else
                {
                    // default to console printing
                    System.Console.WriteLine(string.Format("{0}: line {1} col {2}", error.IsWarning ? "WARNING" : "ERROR", error.Line, error.Column));
                    System.Console.WriteLine(error.ErrorText);
                    System.Console.WriteLine(" "); // space it out
                }
            }
            if (errCt != 0)
                return null;

            return new CodeGen.CompiledScript(results.CompiledAssembly, "");
        }

        // the always there preamble, user usings come right afterwards
        static string CodeHeader = @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;
using Quaternion = Microsoft.Xna.Framework.Quaternion;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Color = Microsoft.Xna.Framework.Color;";

    }

    public class CompiledScript
    {
        Assembly assembly_;
        string compiledTypeName_;
        Type compiledType_;
        CompiledScript next_ = null;

        /// <summary>
        /// Old scripts often need to rename alive (regardless their assemblies always will)
        /// </summary>
        /// <returns>The most recent iteration of a script</returns>
        public CompiledScript GetNewest()
        {
            var current = this;
            while (current.next_ != null)
                current = current.next_;
            return current;
        }

        public CompiledScript(Assembly asm, string compiledTypeName)
        {
            assembly_ = asm;
            compiledTypeName_ = compiledTypeName;
            compiledType_ = assembly_.GetType(compiledTypeName_);
        }

        public Action GetMethod(string name)
        {
            MethodInfo methodInfo = compiledType_.GetMethod(name);
            if (methodInfo == null)
                return null;
            return (Action)Delegate.CreateDelegate(typeof(Action), methodInfo);
        }

        public Action<T> GetMethod<T>(string name)
        {
            MethodInfo methodInfo = compiledType_.GetMethod(name);
            if (methodInfo == null)
                return null;
            return (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), methodInfo, true);
        }

        public Action<T, K> GetMethod<T, K>(string name)
        {
            MethodInfo methodInfo = compiledType_.GetMethod(name);
            if (methodInfo == null)
                return null;
            return (Action<T, K>)Delegate.CreateDelegate(typeof(Action<T,K>), methodInfo, true);
        }

        public Action<T, K, O> GetMethod<T, K, O>(string name)
        {
            MethodInfo methodInfo = compiledType_.GetMethod(name);
            if (methodInfo == null)
                return null;
            return (Action<T, K, O>)Delegate.CreateDelegate(typeof(Action<T, K, O>), methodInfo, true);
        }

        public Action<T, K, O, X> GetMethod<T, K, O, X>(string name)
        {
            MethodInfo methodInfo = compiledType_.GetMethod(name);
            if (methodInfo == null)
                return null;
            return (Action<T, K, O, X>)Delegate.CreateDelegate(typeof(Action<T, K, O, X>), methodInfo, true);
        }

        public Action<T, K, O, X, Y> GetMethod<T, K, O, X, Y>(string name)
        {
            MethodInfo methodInfo = compiledType_.GetMethod(name);
            if (methodInfo == null)
                return null;
            return (Action<T, K, O, X, Y>)Delegate.CreateDelegate(typeof(Action<T, K, O, X, Y>), methodInfo, true);
        }

        public Action<T, K, O, X, Y, Z> GetMethod<T, K, O, X, Y, Z>(string name)
        {
            MethodInfo methodInfo = compiledType_.GetMethod(name);
            if (methodInfo == null)
                return null;
            return (Action<T, K, O, X, Y, Z>)Delegate.CreateDelegate(typeof(Action<T, K, O, X, Y, Z>), methodInfo, true);
        }
    }

    public class CompiledScriptInstance<T> where T : class
    {

    }
}