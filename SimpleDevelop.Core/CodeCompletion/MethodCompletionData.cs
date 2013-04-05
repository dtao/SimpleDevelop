using System.Reflection;

namespace SimpleDevelop.CodeCompletion
{
    class MethodCompletionData : CompletionData<MethodInfo>
    {
        public MethodCompletionData(MethodInfo methodInfo) : base(methodInfo)
        {
            Image = "MethodImage";
        }

        public override object Description
        {
            get { return string.Format("{0} {1}", _memberInfo.ReturnType.Name, _memberInfo.Name); }
        }
    }
}
