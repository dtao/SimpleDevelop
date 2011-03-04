using System.Reflection;

namespace SimpleDevelop.CodeCompletion
{
    class StructCompletionData : CompletionData
    {
        public StructCompletionData(MemberInfo memberInfo) : base(memberInfo)
        {
            Image = StructImage;
        }
    }
}
