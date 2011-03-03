using System.CodeDom.Compiler;

using Microsoft.CSharp;

namespace SimpleDevelop.Core
{
    public class CSharpCodeExecutor : CodeDomExecutor
    {
        protected override CodeDomProvider CreateProvider()
        {
            return new CSharpCodeProvider();
        }
    }
}
