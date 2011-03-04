using System.Reflection;

namespace SimpleDevelop.CodeCompletion
{
    class ConstantCompletionData : CompletionData
    {
        public ConstantCompletionData(MemberInfo memberInfo) : base(memberInfo)
        {
            Image = ConstantImage;
        }
    }
}
