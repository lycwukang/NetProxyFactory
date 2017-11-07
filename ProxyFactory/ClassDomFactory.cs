using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Linq;

namespace Com.Proxy.Compile
{
    public class ClassDomFactory
    {
        private static string compileOutName = "dynamic_" + Guid.NewGuid().ToString("n");

        public static Type Compile(ClassDomProvider classDomProvider)
        {
            var compileCodeProvider = new CSharpCodeProvider();
            var cp = new CompilerParameters(classDomProvider.Assemblies.ToArray(), compileOutName);

            var result = compileCodeProvider.CompileAssemblyFromSource(cp, classDomProvider.ToString());
            if (result.Errors.Count > 0) throw new Exception(result.Errors[0].ErrorText);
            return result.CompiledAssembly.GetType(classDomProvider.FullName);
        }
    }
}