using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SimpleDevelop.Core
{
    public abstract class CodeDomExecutor : ICodeExecutor<CodeDomParameters>
    {
        CodeDomProvider _codeProvider;
        CodeDomParameters _parameters;

        public CodeDomExecutor()
        {
            _codeProvider = CreateProvider();
            _parameters = new CodeDomParameters();
        }

        public event EventHandler<BuildOutputEventArgs> BuildOutput;

        public CodeDomParameters Parameters
        {
            get { return _parameters; }
        }

        public void Execute(IEnumerable<string> code)
        {
            var options = new CompilerParameters
            {
                GenerateExecutable = true,
                MainClass = Parameters.MainClass,
            };

            foreach (string reference in Parameters.References)
            {
                options.ReferencedAssemblies.Add(reference);
            }

            CompilerResults results = _codeProvider.CompileAssemblyFromSource(options, code.ToArray());

            for (int i = 0; i < results.Output.Count; i++)
            {
                OnBuildOutput(results.Output[i]);
            }

            if (results.Errors.Count > 0)
            {
                for (int i = 0; i < results.Errors.Count; i++)
                {
                    OnBuildOutput(results.Errors[i].ErrorText);
                }

                return;
            }

            Process.Start(results.PathToAssembly);
        }

        protected abstract CodeDomProvider CreateProvider();

        protected void OnBuildOutput(string message)
        {
            EventHandler<BuildOutputEventArgs> handler = BuildOutput;
            if (handler != null)
            {
                handler(this, new BuildOutputEventArgs(message));
            }
        }
    }
}
