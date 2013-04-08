using System.Linq;
using System.Reflection;

namespace SimpleDevelop.CodeCompletion
{
    class MethodCompletionData : CompletionData<MethodInfo>
    {
        public MethodCompletionData(MethodInfo methodInfo) : base(methodInfo)
        {
            Image = "Method";
        }

        public override int Rank
        {
            get { return 1; }
        }

        public override object Description
        {
            get { return string.Format("{0} ({1})", GetFriendlyTypeName(_memberInfo.ReturnType.Name), FormatParameters()); }
        }

        private string FormatParameters()
        {
            var parameters = from p in _memberInfo.GetParameters()
                             select string.Format("{0} {1}", GetFriendlyTypeName(p.ParameterType.Name), p.Name);
            return string.Join(", ", parameters);
        }
    }
}
