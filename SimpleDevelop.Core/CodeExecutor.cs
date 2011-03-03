using System.Collections.Generic;

namespace SimpleDevelop.Core
{
    public static class CodeExecutor
    {
        public static void Execute<TParameters>(this ICodeExecutor<TParameters> executor, params string[] code)
        {
            executor.ThrowIfNull("executor");
            code.ThrowIfNull("code");

            executor.Execute((IEnumerable<string>)code);
        }
    }
}
