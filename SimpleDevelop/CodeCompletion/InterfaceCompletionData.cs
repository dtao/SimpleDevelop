using System.Reflection;

namespace SimpleDevelop.CodeCompletion
{
    class InterfaceCompletionData : CompletionData
    {
        public InterfaceCompletionData(MemberInfo memberInfo) : base(memberInfo)
        {
            Image = InterfaceImage;
        }
    }
}
