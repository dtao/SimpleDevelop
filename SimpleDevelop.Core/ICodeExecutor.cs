using System;
using System.Collections.Generic;

namespace SimpleDevelop.Core
{
    public interface ICodeExecutor<TParameters>
    {
        event EventHandler<BuildOutputEventArgs> BuildOutput;
        TParameters Parameters { get; }
        void Execute(IEnumerable<string> code);
    }
}
