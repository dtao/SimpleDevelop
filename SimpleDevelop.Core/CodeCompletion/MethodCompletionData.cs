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
            get { return string.Format("{0} {1}", GetFriendlyTypeName(_memberInfo.ReturnType.Name), _memberInfo.Name); }
        }
    }
}
